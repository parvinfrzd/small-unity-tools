using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Jam3.Inputs {

    [System.Serializable]
    public class TouchEvent : UnityEvent<TouchInput[]> { };

    [System.Serializable]
    public class GuestureEvent : UnityEvent<Vector2> { };

    [System.Serializable]
    public class HoverEvent : UnityEvent<bool> { };

    [System.Serializable]
    public class ZoomEvent : UnityEvent<float> { };

    public class InputEvents : MonoBehaviour {

        // Types of raycasts
        public enum RaycastType {
            Physics3D,
            Graphics,
        }

        // Raycast max distance
        [Range(0, 200)]
        public float hoverCameraDistance = 100;

        // Touch Events
        [Header("Touch events")]
        public bool touchRequiresHover = false;

        public bool touchStart = false;
        public TouchEvent onTouchStart = new TouchEvent();

        public bool touchMove = false;
        public TouchEvent onTouchMove = new TouchEvent();

        public bool touchEnd = false;
        public TouchEvent onTouchEnd = new TouchEvent();

        // Guestures
        [Header("Guestures")]
        public bool swipe = false;
        public GuestureEvent onSwipe = new GuestureEvent();

        public bool hoverScreenCenter = false;
        public bool hover = false;
        public HoverEvent onHover = new HoverEvent();

        public bool zoom = false;
        public ZoomEvent onZoom = new ZoomEvent();

        public bool isPanning = false;
        public bool isDragging = false;

        // Private
        private bool isEnabled = false;
        private Collider inputCollider;
        private Ray ray;
        private Camera inputCamera;
        private Vector2 screenCenter = new Vector2(0, 0);
#pragma warning disable 0414
        private RaycastHit raycastHit;
#pragma warning restore 0414
        private bool isHovering;

        // Events triggered flags
        private bool touchStartTriggered = false;
        private bool touchEndTriggered = false;
        private bool hoverEventTriggered = false;
        private bool swipeTriggered = false;

        private RaycastType raycastType = RaycastType.Physics3D;
        private EventSystem eventSystem;
        private PointerEventData graphicsPointerEventData;
        private Vector2 graphicsPointerPosition = Vector2.zero;
        private GraphicRaycaster graphicsRaycaster;
        private List<RaycastResult> graphicsRaycastResults = new List<RaycastResult>();

        void Awake() {
            // Determine what type of raycasting we need to do for hover
            if (gameObject.GetComponent<RectTransform>() != null) {
                raycastType = RaycastType.Graphics;
                GameObject eventSystemObject = GameObject.Find("EventSystem");
                if (eventSystemObject != null) {
                    eventSystem = eventSystemObject.GetComponent<EventSystem>();
                    graphicsPointerEventData = new PointerEventData(eventSystem);
                }
                else {
                    Debug.LogWarning("No event system found for Graphics Raycaster");
                }
                graphicsRaycaster = GetComponent<GraphicRaycaster>();
                if (graphicsRaycaster == null) {
                    gameObject.AddComponent<GraphicRaycaster>();
                    graphicsRaycaster = GetComponent<GraphicRaycaster>();
                }
            }
            else {
                raycastType = RaycastType.Physics3D;
            }

            if (raycastType == RaycastType.Physics3D) {
                inputCollider = gameObject.GetComponent<MeshCollider>();
                if (inputCollider == null) {
                    inputCollider = gameObject.GetComponent<Collider>();
                    if (inputCollider == null) {
                        inputCollider = gameObject.AddComponent<BoxCollider>();
                        if (inputCollider == null) {
                            Debug.LogError(string.Format("InputEvents requires a Collider or BoxCollider component: {0}", gameObject.name));
                            DestroyImmediate(this);
                        }
                    }
                }
            }

            // Enable hover if touchRequiresHover is set
            if (touchRequiresHover) {
                hover = true;
            }
        }

        public void Toggle(bool enabled) {
            this.isEnabled = enabled;
        }

        void LateUpdate() {
            if (!isEnabled) return;

            InputEventManager.Update();

            if (inputCamera == null && Application.isPlaying) {
                inputCamera = Camera.main;
            }

            // To enable hover raycast, check if mouse has moved position

            // Hover events
            // isHovering = false;

            if (ShouldRaycast()) {
                if (inputCamera != null) {
                    switch (raycastType) {
                        case RaycastType.Physics3D: {
                                ray = inputCamera.ScreenPointToRay(InputEventManager.hoverPosition);
                                isHovering = inputCollider.Raycast(ray, out raycastHit, hoverCameraDistance);
                                break;
                            }
                        case RaycastType.Graphics: {
                                switch (InputEventManager.touchType) {
                                    case TouchTypes.TouchStart:
                                    case TouchTypes.TouchMove:
                                    case TouchTypes.TouchEnd: {
                                            isHovering = raycastGraphics();
                                            break;
                                        }
                                    default: {
                                            break;
                                        }
                                }
                                break;
                            }
                        default: {

                                break;
                            }
                    }
                }
            }

            // Touch events
            if (touchRequiresHover) {
                if (isHovering) {
                    CallTouchEvents();
                }
            }
            else {
                CallTouchEvents();
            }

            CallHoverEvents();

            // Zoom is different
            if (zoom && InputEventManager.isZooming) {
                onZoom.Invoke(InputEventManager.zoomDelta);
            }

            InputEventManager.Reset();
        }

        bool ShouldRaycast() {

            // Debug.Log("x: " + InputEventManager.touchInputs[0].deltaPosition.x + " old x: " + InputEventManager.touchPrevInputs[0].deltaPosition.x);

            return InputEventManager.touchType == TouchTypes.TouchStart ||
              InputEventManager.touchType == TouchTypes.TouchMove ||
              InputEventManager.touchType == TouchTypes.TouchEnd ||
              InputEventManager.isDragging;
        }

        void CallTouchEvents() {

            isPanning = InputEventManager.isPanning;
            isDragging = InputEventManager.isDragging;

            // Guesture events
            switch (InputEventManager.guestureType) {
                case GuestureTypes.Swipe: {
                        if (!swipeTriggered) {
                            swipeTriggered = true;
                            onSwipe.Invoke(InputEventManager.swipeGuesture.direction);
                        }
                        break;
                    }
                default: {
                        break;
                    }
            }

            switch (InputEventManager.touchType) {
                case TouchTypes.TouchStart: {
                        if (!touchStartTriggered) {
                            onTouchStart.Invoke(InputEventManager.touchInputs);
                        }
                        touchStartTriggered = true;
                        touchEndTriggered = false;
                        swipeTriggered = false;
                        break;
                    }
                case TouchTypes.TouchMove: {
                        onTouchMove.Invoke(InputEventManager.touchInputs);
                        break;
                    }
                case TouchTypes.TouchEnd: {
                        touchStartTriggered = false;
                        if (!touchEndTriggered) {
                            onTouchEnd.Invoke(InputEventManager.touchInputs);
                            touchEndTriggered = true;
                        }
                        break;
                    }
                default: {
                        break;
                    }
            }
        }

        private bool raycastGraphics() {
            graphicsPointerPosition.x = InputEventManager.touchInputs[0].position.x;
            graphicsPointerPosition.y = InputEventManager.touchInputs[0].position.y;

            if (graphicsPointerEventData == null) {
                Debug.LogWarning("graphicsPointerEventData is Null");
                return false;
            }
            graphicsPointerEventData.position = graphicsPointerPosition;

            graphicsRaycastResults.Clear();
            graphicsRaycaster.Raycast(graphicsPointerEventData, graphicsRaycastResults);

            bool intersects = false;
            int length = graphicsRaycastResults.Count;
            for (int i = 0; i < length; i++) {
                RaycastResult result = graphicsRaycastResults[i];
                if (result.gameObject.name == gameObject.name) {
                    intersects = true;
                }
            }

            return intersects;
        }

        void CallHoverEvents() {
            // Hover events
            if (hover) {
                if (isHovering && !hoverEventTriggered) {
                    onHover.Invoke(isHovering);
                    hoverEventTriggered = true;
                }
                else if (!isHovering && hoverEventTriggered) {
                    onHover.Invoke(isHovering);
                    hoverEventTriggered = false;
                }
            }
        }

        public void Dispose() {
            Toggle(false);
            onTouchStart.RemoveAllListeners();
            onTouchMove.RemoveAllListeners();
            onTouchEnd.RemoveAllListeners();
            onSwipe.RemoveAllListeners();
            onHover.RemoveAllListeners();
            onZoom.RemoveAllListeners();
        }
    }
}
