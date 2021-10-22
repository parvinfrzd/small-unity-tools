using System;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component {
    private static T _instance;
    protected static bool created = false;
    public static T Instance {
        get {
            if (_instance != null) {
                return _instance;
            }
            else {
                _instance = FindObjectOfType<T>();
                if (_instance == null) {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake() {
        created = true;
        if (_instance == null) {
            _instance = this as T;
        }
    }
}
