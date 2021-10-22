using System;
using System.Collections;
using System.Collections.Generic;

using Jam3.Inputs;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

namespace WC {

    public enum ActiveCameraType { Main, AR }

    public class CameraManager : Singleton<CameraManager> {

        public ActiveCameraType activeCameraType = ActiveCameraType.Main;
        public GameObject mainCamera;
        public GameObject arCamera;
        public Action onCameraChange = delegate { };
        private Camera _camera;
        private PostProcessVolume _postProcessVolume;

        void Start() {
            OnSceneLoaded();
        }

        // Find the main camera when the scene changes
        void UpdateMainCamera() {
            if (mainCamera == null) {
                GameObject camera = GameObject.Find(Constants.GameObjects.MAIN_CAMERA);
                if (camera != null) {
                    mainCamera = camera;
                }
            }
        }

        // Find the ar camera when the scene changes
        void UpdateARCamera() {
            if (arCamera == null) {
                GameObject camera = GameObject.Find(Constants.GameObjects.AR_CAMERA);
                if (camera != null) {
                    arCamera = camera;
                    _postProcessVolume = arCamera.GetComponent<PostProcessVolume>();
                }
            }
        }

        // When the scene is loaded, update references
        void OnSceneLoaded() {
            UpdateMainCamera();
            UpdateARCamera();
            ToggleCameras();
#if UNITY_EDITOR
            ToggleOrbitControls(true);
#endif
        }

        void ToggleCamera(GameObject camera, bool active) {
            if (camera != null) {
                camera.SetActive(active);
                camera.GetComponent<Camera>().enabled = active;
                camera.GetComponent<AudioListener>().enabled = active;
                if (active) {
                    _camera = camera.GetComponent<Camera>();
                }
            }
        }

        void ToggleCameras() {
            ToggleCamera(arCamera, activeCameraType == ActiveCameraType.AR);
            ToggleCamera(mainCamera, activeCameraType == ActiveCameraType.Main);
        }

        public void SetActiveCamera(ActiveCameraType cameraType) {
            activeCameraType = cameraType;
            UpdateMainCamera();
            UpdateARCamera();
            ToggleCameras();
        }

        public void SetOrbitControlsConfig(OrbitControlsConfig config) {
            OrbitControls orbitControls = mainCamera.GetComponent<OrbitControls>();
            orbitControls.rotationSpeedX = config.rotationSpeedX;
            orbitControls.rotationSpeedY = config.rotationSpeedY;
            orbitControls.rotationEasing = config.rotationEasing;
            orbitControls.polarAngleMin = config.polarAngleMin;
            orbitControls.polarAngleMax = config.polarAngleMax;
            orbitControls.thetaAngleMin = config.thetaAngleMin;
            orbitControls.thetaAngleMax = config.thetaAngleMax;
            orbitControls.zoomSpeed = config.zoomSpeed;
            orbitControls.zoomEasing = config.zoomEasing;
            orbitControls.zoomDistanceMin = config.zoomDistanceMin;
            orbitControls.zoomDistanceMax = config.zoomDistanceMax;
            orbitControls.SetDistance(config.zoomDistanceStart, false);
        }

        public void ToggleOrbitControls(bool enabled) {
            mainCamera.GetComponent<OrbitControls>().enabled = enabled;
            mainCamera.GetComponent<InputEvents>().enabled = enabled;
        }

        public Camera GetActiveCamera() {
            return _camera;
        }

        public void TogglePostProcessing() {
            if (_postProcessVolume != null) {
                _postProcessVolume.weight = _postProcessVolume.weight == 0 ? 1 : 0;
            }
        }

        void OnDestroy() {
        }
    }
}
