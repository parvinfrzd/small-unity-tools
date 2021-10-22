using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace WC {

    /// <summary>
    /// The ARSceneContentManager adds and removes the SceneContent
    /// gameobject when a scene is changed.
    /// </summary>
    public class ARSceneContentManager : Singleton<ARSceneContentManager> {
        override protected void Awake() {
            base.Awake();
            ARAnchorCreator.Instance.onAnchorAdded += OnAnchorAdded;
        }

        void OnSceneLoading() {
            RemoveSceneContent();
        }

        void OnSceneLoaded() {
            AddSceneContent();
        }

        void OnAnchorAdded() {
            // When a new anchor is added move the SceneContent to the new anchor transform
            AddSceneContent();
        }

        public void AddSceneContent() {
            GameObject sceneContent = GameObject.Find(Constants.GameObjects.SCENE_CONTENT);
            if (!(sceneContent != null)) {
                Console.LogWarning("AddSceneContent: Couldn't find SceneContent");
                return;
            }
            ARAnchorCreator.Instance.AddGameObject(sceneContent);
        }

        public void RemoveSceneContent() {
            GameObject sceneContent = GameObject.Find(Constants.GameObjects.SCENE_CONTENT);
            if (sceneContent != null) {
                // Only destroy if it's parented
                if (sceneContent.transform.parent != null) {
                    Destroy(sceneContent);
                }
            }
        }

        void OnDestroy() {
            ARAnchorCreator.Instance.onAnchorAdded -= OnAnchorAdded;
        }
    }
}
