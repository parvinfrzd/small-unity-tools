using System;

using UnityEngine;

namespace Jam3.Inputs {

    // Types of touches
    public enum TouchTypes {
        None,
        TouchStart,
        TouchMove,
        TouchEnd
    }

    // Types of guestures
    public enum GuestureTypes {
        None,
        Pinch,
        Swipe
    }

    public enum MouseButtonType {
        Left,
        Right,
        Middle
    }

    public class TouchInput {
        public TouchTypes type;
        public Vector2 position = new Vector2(0, 0);
        public Vector2 deltaPosition = new Vector2(0, 0);
        public float velocity = 0;
        public Vector2 normal = new Vector2(0, 0);
        public Vector2 screenSpace = new Vector2(0, 0);
        public float time = 0;
    }

    public class SwipeGuesture {
        // Has the event been triggered
        public bool trigger = false;
        public bool swipeX = false;
        public bool swipeY = false;
        public Vector2 direction = Vector2.zero;

        // How far the user has to move (px) before we set a swipe event
        public static float threshold = 200;
        private float distX;
        private float distY;

        public void Update(float x1, float y1, float x2, float y2) {
            distX = Math.Abs(x2 - x1);
            distY = Math.Abs(y2 - y1);
            swipeX = distX >= threshold;
            swipeY = distY >= threshold;
            direction.x = swipeX ? x2 > x1 ? 1 : -1 : 0;
            direction.y = swipeY ? y2 > y1 ? 1 : -1 : 0;
            trigger = swipeX || swipeY;
        }

        public void Reset() {
            trigger = false;
            swipeX = false;
            swipeY = false;
            distX = 0;
            distY = 0;
        }
    }

    public class InputEventManager {

        // Event type
        public static TouchTypes touchType;

        // Guesture type
        public static GuestureTypes guestureType;

        public static bool isDragging;
        public static bool isPanning;
        public static bool isZooming;
        public static float zoomDelta = 0;

        public static Vector3 hoverPosition = Vector3.zero;

        public static MouseButtonType mouseButtonType = MouseButtonType.Left;

        private static TouchInput[] touchStartInputs = new TouchInput[] {
      new TouchInput(),
      new TouchInput(),
      new TouchInput(),
      new TouchInput(),
      new TouchInput(),
    };

        public static TouchInput[] touchPrevInputs = new TouchInput[] {
      new TouchInput(),
      new TouchInput(),
      new TouchInput(),
      new TouchInput(),
      new TouchInput(),
    };

        public static TouchInput[] touchInputs = new TouchInput[] {
      new TouchInput(),
      new TouchInput(),
      new TouchInput(),
      new TouchInput(),
      new TouchInput(),
    };

        // Guestures
        public static SwipeGuesture swipeGuesture = new SwipeGuesture();

        // Manager needs update?
        private static bool needsUpdate = true;

        private enum MouseButtons {
            Left,
            Right,
            Middle
        }

        // String names of input types
        //private static string[] inputEvents = new string[] { "Stationary", "TouchStart", "TouchMove", "TouchEnd" };
        private static bool isDown = false;

        private static float GetDistance(Vector2 previousPosition, Vector2 currentPosition) {
            return Vector2.Distance(previousPosition, currentPosition);
        }

        private static float GetVelocity(Vector2 previousPosition, Vector2 currentPosition, float previousTime, float currentTime) {
            float distance = Vector2.Distance(previousPosition, currentPosition);
            float time = (previousTime - currentTime / 1000);
            return distance / time;
        }

        private static TouchTypes GetTouchType(TouchPhase phase) {
            switch (phase) {
                case TouchPhase.Began: {
                        return TouchTypes.TouchStart;
                    }
                case TouchPhase.Moved: {
                        return TouchTypes.TouchMove;
                    }
                case TouchPhase.Ended: {
                        return TouchTypes.TouchEnd;
                    }
                default: {
                        return TouchTypes.None;
                    }
            }
        }

        public static void Update() {
            if (!needsUpdate) return;

            int totalInputs = 0;

            if (Input.touchSupported) {
                // Device
                totalInputs = touchInputs.Length;
                int totalNewInputs = Mathf.Min(Input.touchCount, touchInputs.Length);

                isDragging = Input.touchCount == 1;
                isZooming = Input.touchCount == 2;
                isPanning = Input.touchCount == 2;

                // Set previous
                for (int i = 0; i < totalInputs; i++) {
                    SetTouchInput(touchPrevInputs[i], touchInputs[i].type, touchInputs[i].position, touchInputs[i].deltaPosition, Time.fixedTime);
                }

                // Set hover position
                if (totalNewInputs > 0) {
                    SetHoverInput(Input.touches[0].position);
                }

                float velocity = 0;
                for (int i = 0; i < totalNewInputs; i++) {
                    Touch touch = Input.touches[i];

                    if (touch.phase == TouchPhase.Moved) {
                        velocity = GetVelocity(touchPrevInputs[0].position, touch.position, touchPrevInputs[0].time, touchInputs[0].time);
                    }

                    SetTouchInput(touchInputs[i], GetTouchType(touch.phase), touch.position, touch.deltaPosition, Time.fixedTime, velocity);
                }

                if (touchInputs[0].type == TouchTypes.TouchStart) {
                    SetTouchInput(touchStartInputs[0], TouchTypes.TouchStart, touchInputs[0].position, touchInputs[0].deltaPosition, Time.fixedTime);
                }

                if (touchInputs[0].type == TouchTypes.TouchEnd) {
                    swipeGuesture.Update(
                      touchStartInputs[0].position.x,
                      touchStartInputs[0].position.y,
                      touchInputs[0].position.x,
                      touchInputs[0].position.y);

                    if (swipeGuesture.trigger) {
                        guestureType = GuestureTypes.Swipe;
                    }

                    swipeGuesture.Reset();

                    zoomDelta = 0;
                }

                if (isZooming) {
                    // https://unity3d.com/learn/tutorials/topics/mobile-touch/pinch-zoom
                    TouchInput touch0 = touchInputs[0];
                    TouchInput touch1 = touchInputs[1];

                    // Find the position in the previous frame of each touch.
                    Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
                    Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

                    // Find the magnitude of the vector (the distance) between the touches in each frame.
                    float prevTouchDeltaMag = (touch0PrevPos - touch1PrevPos).magnitude;
                    float touchDeltaMag = (touch0.position - touch1.position).magnitude;

                    // Find the difference in the distances between each frame.
                    float deltaMagnitudeDiff = touchDeltaMag - prevTouchDeltaMag;

                    zoomDelta = deltaMagnitudeDiff / 1000;
                }
                else {
                    zoomDelta = 0;
                }

                // Base the touch type off the first input
                touchType = touchInputs[0].type;

                // Debug.Log(inputEvents[(int)touchType]);

            }
            else {
                // Desktop
                float deltaX = Input.GetAxis("Mouse X");
                float deltaY = Input.GetAxis("Mouse Y");
                float deltaZoom = Input.GetAxis("Mouse ScrollWheel");

                totalInputs = 1;

                bool moved = (deltaX != 0 || deltaY != 0);
                isDragging = moved;

                if (zoomDelta != deltaZoom) {
                    zoomDelta = deltaZoom;
                    isZooming = true;
                }

                // Set previous input
                SetTouchInput(touchPrevInputs[0], touchInputs[0].type, touchInputs[0].position, touchInputs[0].deltaPosition, Time.fixedTime);

                // Set hover position
                SetHoverInput(Input.mousePosition);

                // Touch move
                if ((Input.GetMouseButton((int)MouseButtons.Left) && isDown && moved) || (Input.GetMouseButton((int)MouseButtons.Right) && isDown)) {
                    touchType = TouchTypes.TouchMove;
                    float velocity = GetVelocity(touchPrevInputs[0].position, Input.mousePosition, touchPrevInputs[0].time, touchInputs[0].time);
                    SetTouchInput(touchInputs[0], TouchTypes.TouchMove, Input.mousePosition, Vector2.zero, Time.fixedTime, velocity);
                }

                // Touch Start
                if (Input.GetMouseButtonDown((int)MouseButtons.Left) || Input.GetMouseButtonDown((int)MouseButtons.Right)) {
                    touchType = TouchTypes.TouchStart;
                    SetTouchInput(touchStartInputs[0], TouchTypes.TouchStart, Input.mousePosition, Vector2.zero, Time.fixedTime);
                    SetTouchInput(touchPrevInputs[0], TouchTypes.TouchStart, Input.mousePosition, Vector2.zero, Time.fixedTime);
                    SetTouchInput(touchInputs[0], TouchTypes.TouchEnd, Input.mousePosition, Vector2.zero, Time.fixedTime, 0);
                    isDown = true;
                }

                if (Input.GetMouseButtonDown((int)MouseButtons.Left)) {
                    mouseButtonType = MouseButtonType.Left;
                }
                else if (Input.GetMouseButtonDown((int)MouseButtons.Right)) {
                    mouseButtonType = MouseButtonType.Right;
                }
                else if (Input.GetMouseButtonDown((int)MouseButtons.Middle)) {
                    mouseButtonType = MouseButtonType.Middle;
                }

                if (Input.GetMouseButtonDown((int)MouseButtons.Right)) {
                    isPanning = true;
                }

                // Touch End
                if (Input.GetMouseButtonUp((int)MouseButtons.Left) || Input.GetMouseButtonUp((int)MouseButtons.Right)) {
                    touchType = TouchTypes.TouchEnd;
                    float velocity = GetVelocity(touchPrevInputs[0].position, Input.mousePosition, touchPrevInputs[0].time, touchInputs[0].time);
                    SetTouchInput(touchInputs[0], TouchTypes.TouchEnd, Input.mousePosition, Vector2.zero, Time.fixedTime, velocity);
                    swipeGuesture.Update(
                      touchStartInputs[0].position.x,
                      touchStartInputs[0].position.y,
                      Input.mousePosition.x,
                      Input.mousePosition.y);

                    if (swipeGuesture.trigger) {
                        guestureType = GuestureTypes.Swipe;
                    }

                    swipeGuesture.Reset();

                    isDown = false;
                    isPanning = false;
                }

                // Debug.Log(inputEvents[(int)touchType]);
            }

            needsUpdate = false;
        }

        private static void SetHoverInput(Vector2 position) {
            hoverPosition.x = position.x;
            hoverPosition.y = position.y;
        }

        private static void SetTouchInput(TouchInput touchInput, TouchTypes type, Vector2 position, Vector2 deltaPosition, float time, float velocity = 0) {
            touchInput.position.x = position.x;
            touchInput.position.y = position.y;
            touchInput.deltaPosition = deltaPosition;
            touchInput.normal.x = Mathf.Clamp(touchInput.position.x / Screen.width, 0, 1);
            touchInput.normal.y = Mathf.Clamp(touchInput.position.y / Screen.height, 0, 1);
            touchInput.screenSpace.x = Mathf.Lerp(-1, 1, touchInput.normal.x);
            touchInput.screenSpace.y = Mathf.Lerp(-1, 1, touchInput.normal.y);
            touchInput.time = time;
            touchInput.velocity = velocity;
            touchInput.type = type;
        }

        public static void Reset() {
            if (needsUpdate) return;
            needsUpdate = true;

            zoomDelta = 0;
            isZooming = false;

            if (touchType == TouchTypes.TouchEnd) {
                touchType = TouchTypes.None;
                guestureType = GuestureTypes.None;
            }
        }
    }
}
