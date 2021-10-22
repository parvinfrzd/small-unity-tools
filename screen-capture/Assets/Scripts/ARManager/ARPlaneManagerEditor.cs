using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

namespace WC {

    /// <summary>
    /// The ARPlaneManagerEditor simulates the behaviour of the ARPlaneManager
    /// </summary>
    public class ARPlaneManagerEditor : MonoBehaviour {
        public GameObject planePrefab;
        public Action planesChanged = delegate { };

        private GameObject _plane;
        public ARRaycastManagerEditor _arRaycastManagerEditor;

        void Awake() {
            _arRaycastManagerEditor = gameObject.GetComponent<ARRaycastManagerEditor>();
            _plane = Instantiate(planePrefab, Vector3.zero, Quaternion.identity);
            _arRaycastManagerEditor.SetTrackable(_plane);
            SetPlaneVisibility(false);
        }

        public void StartTracking(bool enabled) {
            if (enabled) {
                SetPlaneVisibility(false);
                StartCoroutine(OnPlaneFound());
            }
            else {
                SetPlaneVisibility(enabled);
            }
        }

        IEnumerator OnPlaneFound() {
            yield return new WaitForSeconds(1);
            SetPlaneVisibility(true);
            planesChanged();
        }

        void SetPlaneVisibility(bool visible) {
            _plane.SetActive(visible);
        }
    }
}
