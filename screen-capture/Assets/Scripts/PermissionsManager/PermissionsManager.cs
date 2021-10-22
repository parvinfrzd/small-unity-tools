using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WC {

    public enum PermissionType { Camera, Microphone, SpeechRecognition }
    public enum PermissionStatus { Enabled, Disabled, None }

    public class PermissionsManager : Singleton<PermissionsManager> {
        public PermissionStatus cameraStatus = PermissionStatus.None;
        public PermissionStatus microphoneStatus = PermissionStatus.None;
        public PermissionStatus speechRecognitionStatus = PermissionStatus.None;
        public Action onCameraPermissionChangedEvent;
        public Action onMicrophonePermissionChangedEvent;
        public Action onSpeechRecognitionPermissionChangedEvent;

        public void SetPermission(PermissionType type, PermissionStatus status) {
            switch (type) {
                case PermissionType.Camera:
                    cameraStatus = status;
                    if (onCameraPermissionChangedEvent != null)
                        onCameraPermissionChangedEvent();
                    break;
                case PermissionType.Microphone:
                    microphoneStatus = status;
                    if (onMicrophonePermissionChangedEvent != null)
                        onMicrophonePermissionChangedEvent();
                    break;
                case PermissionType.SpeechRecognition:
                    speechRecognitionStatus = status;
                    if (onSpeechRecognitionPermissionChangedEvent != null)
                        onSpeechRecognitionPermissionChangedEvent();
                    break;
                default:
                    break;
            }
        }
    }
}

