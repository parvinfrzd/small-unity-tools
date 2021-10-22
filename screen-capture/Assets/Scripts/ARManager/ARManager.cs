using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;


namespace WC {

    /// <summary>
    /// The ARManager controls the ARSession
    /// </summary>
    [RequireComponent(typeof(ARSession))]
    public class ARManager : Singleton<ARManager> {

        public GameObject trackablesMockPrefab;
        public ARCameraManager arCameraManager;
        public ARPlaneManager arPlaneManager;
        public ARPlaneManagerEditor arPlaneManagerEditor;
        public Action onPlaneDetected = delegate { };
        public Action onStarted = delegate { };

        [SerializeField]
        Text logText;
        private List<string> logMessages = new List<string>();

        private ARSession _arSession;
        private GameObject _trackablesMock;
        private bool _requestingPermission = false;

        override protected void Awake() {
            base.Awake();
            _arSession = gameObject.GetComponent<ARSession>();
            BindEvents(true);
        }

        void Start() {

#if UNITY_EDITOR
            if (trackablesMockPrefab != null) {
                // Create a trackables gameObject to simulate what AR Foundation does
                // This is the parent for the anchor
                _trackablesMock = Instantiate(trackablesMockPrefab, Vector3.zero, Quaternion.identity);
                _trackablesMock.transform.parent = gameObject.transform;
                _trackablesMock.transform.localPosition = Vector3.zero;
                _trackablesMock.transform.localRotation = Quaternion.identity;
            }
            else {
                Console.Log("trackablesMockPrefab is null" + trackablesMockPrefab);
            }
#endif
        }

        public IEnumerator StartAR() {
#if UNITY_EDITOR
            PermissionsManager.Instance.SetPermission(PermissionType.Camera, PermissionStatus.Enabled);
            yield return null;
            CameraManager.Instance.SetActiveCamera(ActiveCameraType.Main);
            ScreenCaptureManager.Instance.UpdateCamera(ActiveCameraType.Main);
            if (onStarted != null) {
                onStarted();
            }
#else
            // If permission already accepted from previous session
            if (arCameraManager.permissionGranted) {
                PermissionsManager.Instance.SetPermission(PermissionType.Camera, PermissionStatus.Enabled);
            } else {
                _requestingPermission = true;
            }

            yield return ARSession.CheckAvailability();

            if (ARSession.state == ARSessionState.Ready) {
                _arSession.enabled = true;
                CameraManager.Instance.SetActiveCamera(ActiveCameraType.AR);
                ScreenCaptureManager.Instance.UpdateCamera(ActiveCameraType.AR);
                if (onStarted != null) {
                    onStarted();
                }
            }
#endif
        }

        void OnApplicationPause(bool pauseStatus) {
            if (!pauseStatus && _requestingPermission) {
                PermissionStatus status = arCameraManager.permissionGranted ? PermissionStatus.Enabled : PermissionStatus.Disabled;
                PermissionsManager.Instance.SetPermission(PermissionType.Camera, status);
                _requestingPermission = false;
            }
        }

        public void TogglePlaneDetection(bool enabled) {
            arPlaneManager.enabled = enabled;
            // Toggle plane visiblity
            foreach (ARPlane plane in arPlaneManager.trackables) {
                plane.gameObject.SetActive(enabled);
            }
#if UNITY_EDITOR
            arPlaneManagerEditor.StartTracking(enabled);
#endif
        }

        void OnARSessionStateChanged(ARSessionStateChangedEventArgs eventArgs) {

            Log("ARSession.state: " + ARSession.state + "granted: " + arCameraManager.permissionGranted);

            switch (ARSession.state) {
                case ARSessionState.None:
                    // The AR System has not been initialized and availability is unknown.
                    Log("ARSessionState.None");
                    break;
                case ARSessionState.Unsupported:
                    // The current device doesn't support AR.
                    Log("ARSessionState.Unsupported");
                    break;
                case ARSessionState.CheckingAvailability:
                    // The system is checking the availability of AR on the current device.
                    Log("ARSessionState.CheckingAvailability");
                    break;
                case ARSessionState.NeedsInstall:
                    // The current device supports AR, but AR support requires additional software to be installed.
                    Log("ARSessionState.NeedsInstall");
                    break;
                case ARSessionState.Installing:
                    // AR software is being installed.
                    Log("ARSessionState.Installing");
                    break;
                case ARSessionState.Ready:
                    // AR is supported and ready.
                    Log("ARSessionState.Ready");
                    break;
                case ARSessionState.SessionInitializing:
                    // An AR session is initalising
                    Log("ARSessionState.SessionInitializing");
                    break;
                case ARSessionState.SessionTracking:
                    // An AR session is running and is tracking (that is, the device is able to determine its position and orientation in the world)
                    Log("ARSessionState.SessionTracking");
                    break;
                default:
                    break;
            }
        }


        void OnPlanesChanged(ARPlanesChangedEventArgs args) {
            if (args.added.Count > 0) {
                onPlaneDetected();
            }
        }

        void OnPlanesChangedEditor() {
            onPlaneDetected();
        }

        void BindEvents(bool bind) {
            if (_arSession != null) {
                if (bind) {
                    ARSession.stateChanged += OnARSessionStateChanged;
                }
                else {
                    ARSession.stateChanged -= OnARSessionStateChanged;
                }
            }

            if (arPlaneManager != null) {
                if (bind) {
                    arPlaneManager.planesChanged += OnPlanesChanged;
                }
                else {
                    arPlaneManager.planesChanged -= OnPlanesChanged;
                }
            }
#if UNITY_EDITOR
            if (arPlaneManagerEditor != null) {
                if (bind) {
                    arPlaneManagerEditor.planesChanged += OnPlanesChangedEditor;
                }
                else {
                    arPlaneManagerEditor.planesChanged -= OnPlanesChangedEditor;
                }
            }
#endif
        }

        void OnDestroy() {
            BindEvents(false);
        }

        public void Reset() {
            _arSession.Reset();
        }

        public void ToggleLog() {
            logText.enabled = !logText.enabled;
        }

        void Log(string message) {
            logMessages.Add(message);

            if (logMessages.Count > 20) {
                logMessages.RemoveAt(0);
            }

            string output = "";
            for (int i = 0; i < logMessages.Count; i++) {
                output += $"\n{logMessages[i]}\n";
            }
            logText.text = output;
        }
    }
}
