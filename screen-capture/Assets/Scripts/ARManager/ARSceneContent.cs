using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WC {

    /// <summary>
    /// The ARSceneContent hides the gameObject when a scene is changed
    /// The gameObject is then parented to the trackables gameObject when ready
    /// </summary>
    public class ARSceneContent : MonoBehaviour {
        static Vector3 offset = new Vector3(1000, 1000, 1000);
        void Awake() {
            // Hide the scene content until it's been parented by the ARSceneContentManager
            transform.position = offset;
        }
    }
}
