using System;
using System.Collections.Generic;

using UnityEngine;

namespace WC {
    public class Constants {

        public class GameObjects {
            public const string AR_CAMERA = "ARCamera";
            public const string MAIN_CAMERA = "MainCamera";
            public const string SCENE_CONTENT = "SceneContentAR";
#if UNITY_EDITOR
            public const string AR_TRACKABLES = "ARTrackablesEditor(Clone)";
#else
            public const string AR_TRACKABLES = "Trackables";
#endif
        }
    }
}
