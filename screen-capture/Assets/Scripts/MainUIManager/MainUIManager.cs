using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace WC.UI {
    public class MainUIManager : Singleton<MainUIManager> {
        public Button screenCaptureButton;
        void Start() {
            screenCaptureButton.onClick.AddListener(ScreenCaptureManager.Instance.OnClickButtonTakeScreenshot);
        }

        void OnDestroy() {
            screenCaptureButton.onClick.RemoveAllListeners();
        }
    }
}
