using System;
using System.Collections;
using System.Collections.Generic;

using Jam3.Inputs;

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace WC {


    /// <summary>
    /// The ARAnchorCreator creates and manages the AR anchors
    /// for devices and the editor
    /// </summary>
    [RequireComponent(typeof(ARRaycastManager))]
    public class ARAnchorCreator : Singleton<ARAnchorCreator> {

        public GameObject placedPrefab;
        [HideInInspector]
        public Action onAnchorAdded = delegate { };
        public ARPlacementIndicator arPlacementIndicator;

        private ARRaycastManager _raycastManager;
        private ARRaycastManagerEditor _raycastManagerEditor;
        private ARAnchorManager _anchorManager;
        private ARPlaneManager _planeManager;
        private GameObject _spawnedObject;
        private InputEvents _inputEvents;
        private List<ARAnchor> _anchors = new List<ARAnchor>();
        private List<ARRaycastHit> _raycastHits = new List<ARRaycastHit>();
        private bool _placementEnabled = false;
        private Vector3 _raycastIntersectionPoint = new Vector3();
        private Vector3 _rayPosition = new Vector3(0.5f, 0.5f, 0f);
        private bool _debugAnchorPlacement = false;

        protected override void Awake() {
            base.Awake();
            _raycastManager = GetComponent<ARRaycastManager>();
            _raycastManagerEditor = GetComponent<ARRaycastManagerEditor>();
            _anchorManager = GetComponent<ARAnchorManager>();
            _planeManager = GetComponent<ARPlaneManager>();
            _inputEvents = gameObject.GetComponent<InputEvents>();
        }

        void Start() {
            _inputEvents.onTouchStart.AddListener(OnTouchStart);
        }

        ARAnchor CreateAnchor(in ARRaycastHit hit) {
            return _anchorManager.AddAnchor(hit.pose);
        }

        void RemoveAllAnchors() {
            foreach (var anchor in _anchors) {
                _anchorManager.RemoveAnchor(anchor);
            }
            _anchors.Clear();
        }

        public void Reset() {
            RemoveAllAnchors();
        }

        public void AddGameObject(GameObject child) {
#if UNITY_EDITOR
            // In the editor we simulate adding the child to the anchor
            GameObject anchor = GameObject.Find(Constants.GameObjects.AR_TRACKABLES);
            if (anchor != null) {
                child.transform.parent = anchor.transform;
                child.transform.localPosition = Vector3.zero;
            }
#else
            if (_anchors.Count > 0) {
                ARAnchor anchor = _anchors[0];
                child.transform.parent = anchor.transform;
                child.transform.localPosition = Vector3.zero;
            }
#endif
        }

        public void Toggle(bool enabled) {
            _placementEnabled = enabled;
            if (_inputEvents != null) {
                _inputEvents.Toggle(enabled);
            }

            GameObject sceneContent = GameObject.Find(Constants.GameObjects.SCENE_CONTENT);
            arPlacementIndicator.ToggleVisiblity(enabled);

            if (enabled) {
                arPlacementIndicator.AddGameObject(sceneContent);
            }
        }

        void OnTouchStart(TouchInput[] touchInput) {
#if UNITY_EDITOR
            if (InputEventManager.mouseButtonType == MouseButtonType.Left) {
                AddAnchorEditor();
            }
#else
            // Block raycast if intersected with UI
            if(EventSystem.current.currentSelectedGameObject == null) {
                AddAnchor();
            }
#endif
        }

        void AddAnchor() {
            Camera camera = CameraManager.Instance.GetActiveCamera();
            Ray ray = camera.ViewportPointToRay(_rayPosition);
            if (_raycastManager.Raycast(ray, _raycastHits, TrackableType.PlaneWithinPolygon)) {
                RemoveAllAnchors();

                // Raycast hits are sorted by distance, so the first one
                // will be the closest hit.
                Pose hitPose = _raycastHits[0].pose;

                ARAnchor anchor = CreateAnchor(_raycastHits[0]);

                if (anchor) {
                    _anchors.Add(anchor);

                    if (_debugAnchorPlacement) {
                        if (_spawnedObject == null) {
                            _spawnedObject = Instantiate(placedPrefab, Vector3.zero, Quaternion.identity);
                        }

                        // Reset transform when we attach to parent anchor
                        _spawnedObject.transform.parent = anchor.transform;
                        _spawnedObject.transform.localPosition = Vector3.zero;
                        _spawnedObject.transform.localRotation = Quaternion.identity;
                    }

                    onAnchorAdded();
                }
                else {
                    Console.Log("Error creating anchor");
                }
            }
        }

        void AddAnchorEditor() {
            Camera camera = CameraManager.Instance.GetActiveCamera();
            Ray ray = camera.ViewportPointToRay(_rayPosition);
            if (_raycastManagerEditor.Raycast(ray, out _raycastIntersectionPoint)) {
                GameObject anchor = GameObject.Find(Constants.GameObjects.AR_TRACKABLES);
                anchor.transform.localPosition = _raycastIntersectionPoint;

                if (_debugAnchorPlacement) {
                    if (_spawnedObject == null) {
                        _spawnedObject = Instantiate(placedPrefab, Vector3.zero, Quaternion.identity);
                    }

                    // Reset transform when we attach to parent anchor
                    _spawnedObject.transform.parent = anchor.transform;
                    _spawnedObject.transform.localPosition = Vector3.zero;
                    _spawnedObject.transform.localRotation = Quaternion.identity;
                }

                onAnchorAdded();
            }
        }

        void Update() {
            if (_placementEnabled) {
                Camera camera = CameraManager.Instance.GetActiveCamera();
                Ray ray = camera.ViewportPointToRay(_rayPosition);
#if UNITY_EDITOR
                if (_raycastManagerEditor.Raycast(ray, out _raycastIntersectionPoint)) {
                    arPlacementIndicator.SetPosition(_raycastIntersectionPoint);
                }
#else
                if (_raycastManager.Raycast(ray, _raycastHits, TrackableType.PlaneWithinPolygon)) {
                     arPlacementIndicator.SetPosition(_raycastHits[0].pose.position);
                }
#endif
            }
        }

        void OnDestroy() {
            if (_inputEvents != null) {
                _inputEvents.onTouchStart.RemoveListener(OnTouchStart);
            }
        }
    }
}
