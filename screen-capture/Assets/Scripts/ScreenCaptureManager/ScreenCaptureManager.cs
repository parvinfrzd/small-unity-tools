
using System.IO;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine;
using UnityEngine.UI;

namespace WC {
    public class ScreenCaptureManager : Singleton<ScreenCaptureManager> {
        public string capturedAssetPath { get; private set; }
        public PostProcessVolume volume;
        public Text text;

        private bool _isProcessing = false;
        private int _newWidth = 1080;
        private int _newHeight = 1920;
        private Camera _mainCamera;
        private Texture2D _logoImage;
        private Overlay _overlay;
        private Grayscale _grayscale;

        private void Start() {
            LoadLogoTexture();
            SetEffects();

            string[] files = Directory.GetFiles(CapturesDirectory);
            foreach (string file in files) {
                //delete existing screenshots from before
                Debug.Log("Deleting leftover capture: " + file);
                File.Delete(file);
            }
        }

        public Camera UpdateCamera(ActiveCameraType cameraType) {
            if (cameraType == ActiveCameraType.AR) {
                GameObject cameraObj = GameObject.Find(Constants.GameObjects.AR_CAMERA);
                _mainCamera = cameraObj.GetComponent<Camera>();
            }
            else {
                GameObject cameraObj = GameObject.Find(Constants.GameObjects.MAIN_CAMERA);
                _mainCamera = cameraObj.GetComponent<Camera>();
            }
            return _mainCamera;
        }

        public void OnClickButtonTakeScreenshot() {
            if (_isProcessing)
                return;
            else {
                EnableOverlay(true);
                CaptureScreenshot();
            }
        }

        private void SetEffects() {
            volume.profile.TryGetSettings(out _overlay);
            volume.profile.TryGetSettings(out _grayscale);
            //TODO: remove this line in case we want grayscale
            _grayscale.enabled.value = false;
        }

        private string CapturesDirectory {
            get {
                string path = Path.Combine(Application.persistentDataPath, "Captures");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        private void LoadLogoTexture() {
            // load logo texture from resource folder
            _logoImage = Resources.Load("Images/T_Jam3") as Texture2D;
        }

        private void CaptureScreenshot() {
            _isProcessing = true;

            string filename = "Wild Cities AR" + System.DateTime.Now.Hour + System.DateTime.Now.Minute + System.DateTime.Now.Second + ".jpg";
            capturedAssetPath = Path.Combine(CapturesDirectory, filename);

            int width = Screen.width;
            int height = Screen.height;
            Texture2D screenshot = new Texture2D(width, height, TextureFormat.ARGB32, false);
            TextureScale.Bilinear(screenshot, _newWidth, _newHeight);
            screenshot.Apply();

            RenderTexture renderTex = new RenderTexture(_newWidth, _newHeight, 24);
            _mainCamera.targetTexture = renderTex;
            RenderTexture.active = renderTex;
            _mainCamera.Render();

            screenshot.ReadPixels(new Rect(0, 0, _newWidth, _newHeight), 0, 0);
            screenshot.Apply();
            RenderTexture.active = null;
            _mainCamera.targetTexture = null;

            //in IOS
            NativeGallery.SaveImageToGallery(screenshot, "captures", filename);
#if UNITY_EDITOR
            File.WriteAllBytes(capturedAssetPath, screenshot.EncodeToPNG());
#endif
            //release
            renderTex.Release();
            EnableOverlay(false);
            DestroyImmediate(screenshot);
            text.text = "screenshot done";
            _isProcessing = false;
        }

        void EnableOverlay(bool enabled) {
            _overlay.enabled.value = enabled;
        }
    }
}
