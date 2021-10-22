using System.Collections;
using System.Collections.Generic;
using Jam3.Inputs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OrbitControls : MonoBehaviour {

  public bool isEnabled = true;
  public bool isRotateEnabled = true;
  public bool isZoomEnabled = true;
  public float distance = 10.0f;

  [Header("Rotation")]
  [Range(0, 180)]
  public float rotationSpeedX = 180;

  [Range(0, 180)]
  public float rotationSpeedY = 180;

  [Range(0, 1)]
  public float rotationEasing = 0.25f;

  [Header("Zoom")]
  [Range(0, 20)]
  public float zoomSpeed = 2;

  [Range(0, 20)]
  public float zoomDistanceMin = 1;

  [Range(0, 100)]
  public float zoomDistanceMax = 50;

  public float finalDistance = 0;

  [Range(-90, 0)]
  public float polarAngleMin = -90;

  [Range(0, 90)]
  public float polarAngleMax = 90;

  [Range(-90, 0)]
  public float thetaAngleMin = -90;

  [Range(-90, 0)]
  public float thetaAngleMax = 90;

  public Vector3 target = Vector3.zero;
  private Vector3 targetOffset;

  [Range(0, 1)]
  public float zoomEasing = 0.15f;

  [Range(0, 100)]
  public float panSpeed = 70f;

  public UnityEvent onTouchEnd = new UnityEvent();
  public UnityEvent onTouchMove = new UnityEvent();
  public UnityEvent onTouchStart = new UnityEvent();
  public UnityEvent onZoom = new UnityEvent();

  private Vector2 touchStartPosition = Vector2.zero;
  private Vector2 prevTouchStartPosition = Vector2.zero;
  private Vector2 direction = Vector2.zero;
  public Vector2 rotation = Vector2.zero;
  private Vector2 rotationOffset = Vector2.zero;
  private Quaternion quaternion = Quaternion.identity;
  public Vector2 finalRotation = Vector2.zero;
  private Vector2 savedRotation = Vector2.zero;
  private InputEvents inputEvents;
  private bool isMoving = false;

  private const float EASE_THRESHOLD = 0.0001f;

  void Awake() {
    Vector3 angles = transform.eulerAngles;
    rotation.x = angles.x;
    rotation.y = angles.y;

    inputEvents = gameObject.GetComponent<InputEvents>();

    // Attach input events
    if (inputEvents == null) {
      inputEvents = gameObject.AddComponent<InputEvents>();
    }

    inputEvents.touchStart = true;
    inputEvents.touchMove = true;
    inputEvents.zoom = true;
    inputEvents.onTouchStart.AddListener(OnTouchStart);
    inputEvents.onTouchEnd.AddListener(OnTouchEnd);
    inputEvents.onTouchMove.AddListener(OnTouchMove);
    inputEvents.onZoom.AddListener(OnZoom);
    inputEvents.Toggle(true);
  }

  void OnTouchStart(TouchInput[] touchInput) {
    if (!isEnabled || !isRotateEnabled) return;
    prevTouchStartPosition.x = touchStartPosition.x;
    prevTouchStartPosition.y = touchStartPosition.y;
    touchStartPosition.x = touchInput[0].normal.x;
    touchStartPosition.y = touchInput[0].normal.y;
    rotationOffset.x = rotation.x;
    rotationOffset.y = rotation.y;
    targetOffset.Set(target.x, target.y, target.z);
    onTouchStart.Invoke();
  }

  void OnTouchMove(TouchInput[] touchInput) {
    if (!isEnabled || !isRotateEnabled) return;
    if (!isMoving) {
      OnTouchStart(touchInput);
      isMoving = true;
    }
    onTouchMove.Invoke();
    direction.x = touchStartPosition.x - touchInput[0].normal.x;
    direction.y = touchStartPosition.y - touchInput[0].normal.y;

    if (!inputEvents.isPanning) {
      rotation.x = rotationOffset.x + (direction.y * rotationSpeedX);
      rotation.y = rotationOffset.y + (-direction.x * rotationSpeedY);

      rotation.x = Mathf.Clamp(rotation.x, polarAngleMin, polarAngleMax);

      // Clamp y is based off the current saved rotation from CameraData
      rotation.y = Mathf.Clamp(rotation.y, savedRotation.y + thetaAngleMin, savedRotation.y + thetaAngleMax);
    }
#if UNITY_EDITOR
    // Only allow panning in editor
    else if (Debug.isDebugBuild) {
      Vector3 direction = Vector3.Normalize(transform.position - target);
      Vector3 cross = Vector3.Cross(direction, Vector3.up);
      float tx = targetOffset.x + (touchStartPosition.x - touchInput[0].normal.x) * panSpeed * cross.x;
      float ty = targetOffset.y + (touchStartPosition.y - touchInput[0].normal.y) * panSpeed;
      float tz = targetOffset.z + (touchStartPosition.x - touchInput[0].normal.x) * panSpeed * cross.z;
      target.Set(tx, ty, tz);
    }
#endif
  }

  void OnTouchEnd(TouchInput[] touchInput) {
    if (!isRotateEnabled) return;
    isMoving = false;
    onTouchEnd.Invoke();
  }

  void OnZoom(float delta) {
    UpdateZoom(delta);
    onZoom.Invoke();
  }

  public void UpdateZoom(float delta, bool force = false) {
    if (!force) {
      if (!isEnabled || !isZoomEnabled) return;
    }
    distance = Mathf.Clamp(distance - delta * zoomSpeed, zoomDistanceMin, zoomDistanceMax);
  }

  public void SetRotation(float rotationX, float rotationY, bool ease = true) {
    rotation.x = rotationX;
    rotation.y = rotationY;
    savedRotation.x = rotationX;
    savedRotation.y = rotationY;
    if (!ease) {
      finalRotation.x = rotationX;
      finalRotation.y = rotationY;
    }
  }

  public void SetDistance(float distance, bool ease = true) {
    this.distance = Mathf.Clamp(distance, zoomDistanceMin, zoomDistanceMax);
    if (!ease) {
      finalDistance = distance;
    }
  }

  void Update() {
    finalRotation.x += (rotation.x - finalRotation.x) * rotationEasing;
    finalRotation.y += (rotation.y - finalRotation.y) * rotationEasing;

    finalRotation.x = Mathf.Clamp(finalRotation.x, polarAngleMin, polarAngleMax);
    finalDistance += (distance - finalDistance) * zoomEasing;

    quaternion = Quaternion.Euler(finalRotation.x, finalRotation.y, 0);

    Vector3 negativeDistance = new Vector3(0.0f, 0.0f, -finalDistance);
    Vector3 position = quaternion * negativeDistance + target;

    transform.rotation = quaternion;
    transform.position = position;

    if (!isEnabled && !isRotateEnabled) {
      touchStartPosition.x = prevTouchStartPosition.x;
      touchStartPosition.y = prevTouchStartPosition.y;
    }
  }

  void OnDestroy() {
    if (inputEvents != null) {
      inputEvents.onTouchStart.RemoveAllListeners();
      inputEvents.onTouchEnd.RemoveAllListeners();
      inputEvents.onTouchMove.RemoveAllListeners();
      inputEvents.onZoom.RemoveAllListeners();
    }
  }
}
