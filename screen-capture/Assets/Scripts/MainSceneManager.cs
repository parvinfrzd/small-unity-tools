using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace WC {
    public class MainSceneManager : MonoBehaviour {

        public Button startButton;
        public Button planeDetectionOnButton;
        public Button planeDetectionOffButton;
        public Button resetButton;
        public Button toggleLogButton;

        void Start() {
            ARManager.Instance.TogglePlaneDetection(false);
            ARManager.Instance.onStarted += OnStarted;
        }

        public void StartAR() {
            StartCoroutine(ARManager.Instance.StartAR());
        }

        void OnStarted() {
            startButton.interactable = false;
            resetButton.interactable = true;
            planeDetectionOnButton.interactable = true;
        }

        public void TogglePlaneDetection(bool enabled) {
            ARManager.Instance.TogglePlaneDetection(enabled);
            ARAnchorCreator.Instance.Toggle(enabled);
            planeDetectionOnButton.interactable = !enabled;
            planeDetectionOffButton.interactable = enabled;
        }

        public void Reset() {
            GameObject sceneContent = GameObject.Find(Constants.GameObjects.SCENE_CONTENT);
            if (!(sceneContent != null)) {
                Console.LogWarning("AddSceneContent: Couldn't find SceneContent");
            }
            else {
                // Move back to the scene root
                sceneContent.transform.parent = null;
                sceneContent.transform.localPosition = Vector3.zero;
            }

            ARAnchorCreator.Instance.Reset();
            ARManager.Instance.Reset();
        }

        public void ToggleLog() {
            ARManager.Instance.ToggleLog();
        }

        public void TogglePostProcessing() {
            CameraManager.Instance.TogglePostProcessing();
        }
    }
}

