using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WC {

    /// <summary>
    /// The ARPlacementIndicator shows a visual indicator where the user
    /// will place the AR Anchor
    /// </summary>
    public class ARPlacementIndicator : MonoBehaviour {

        public MeshRenderer _renderer;

        public void ToggleVisiblity(bool visible) {
            _renderer.enabled = visible;
        }

        public void AddGameObject(GameObject content) {
            content.transform.parent = gameObject.transform;
            content.transform.localPosition = Vector3.zero;
        }

        public void SetPosition(Vector3 position) {
            gameObject.transform.localPosition = position;
        }
    }
}
