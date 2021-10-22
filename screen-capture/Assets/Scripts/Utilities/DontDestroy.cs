using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WC {
    public class DontDestroy : MonoBehaviour {
        void Awake() {
            DontDestroyOnLoad(gameObject);
        }
    }
}
