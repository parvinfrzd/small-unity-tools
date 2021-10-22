using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

using Jam3.Inputs;

namespace WC {

    /// <summary>
    /// The ARRaycastManagerEditor provides raycasting for the editor AR simulation
    /// </summary>
    public class ARRaycastManagerEditor : Singleton<ARRaycastManagerEditor> {

        // private Ray _ray;
        private RaycastHit _hit;
        private MeshCollider _inputCollider;

        public void SetTrackable(GameObject trackable) {
            _inputCollider = trackable.GetComponent<MeshCollider>();
        }

        public bool Raycast(Ray ray, out Vector3 position) {
            // If we click on the UI return to avoid a raycast
            if (EventSystem.current.IsPointerOverGameObject()) {
                position = Vector3.zero;
                return false;
            }
            bool intersected = _inputCollider.Raycast(ray, out _hit, 100);
            position = _hit.point;
            return intersected;
        }
    }
}
