 using System.Collections;
 using UnityEngine;

 // A script to draw the bounding box of a box collider
 // https://answers.unity.com/questions/287081/box-collider-vertexes-in-world-space.html

 namespace Jam3.Utils {

   //  [ExecuteInEditMode()]
   public class ShowBounds : MonoBehaviour {

     public Color color = Color.green;

#if UNITY_EDITOR
     private Vector3[] markers = new Vector3[8];
     public bool useLineRenderer = true;
#else
     private bool useLineRenderer = false;
#endif

     private BoxCollider boxCollider;
     private LineRenderer lineRenderer;

#if UNITY_EDITOR
     void Awake() {
       boxCollider = GetComponent<BoxCollider>();
       for (int i = 0; i < 8; i++) {
         markers[i] = new Vector3(0, 0, 0);
       }

       if (useLineRenderer) {
         GameObject gameObject = new GameObject();
         gameObject.name = "ShowBounds";
         gameObject.AddComponent<LineRenderer>();
         gameObject.hideFlags = HideFlags.HideAndDontSave;
         lineRenderer = gameObject.GetComponent<LineRenderer>();
         lineRenderer.positionCount = 22;
         lineRenderer.widthMultiplier = 0.02f;
         Material material = Resources.Load<Material>("Dev/Mat_ShowBounds");
         lineRenderer.material = material;
       }
     }

     void Update() {
       if (boxCollider) {
         // Get local collider center
         Vector3 boxColliderCenter = boxCollider.center;
         // Get local collider extents
         Vector3 boxColliderExtents = boxCollider.size;

         for (int i = 0; i < 8; i++) {
           // Get one of vertice offset from center
           Vector3 ext = boxColliderExtents;
           ext.Scale(new Vector3((i & 1) == 0 ? 1 : -1f, (i & 2) == 0 ? 1f : -1f, (i & 4) == 0 ? 1f : -1f));
           ext.Scale(new Vector3(0.5f, 0.5f, 0.5f)); // Scale down half so it matches the collider size
           // Calculate local vertice position
           Vector3 vertPositionLocal = boxColliderCenter + ext;
           // Move sphere to global vertice position
           markers[i] = boxCollider.transform.TransformPoint(vertPositionLocal);
         }

         Vector3 frontTopLeft = markers[0];
         Vector3 frontTopRight = markers[1];
         Vector3 frontBottomLeft = markers[2];
         Vector3 frontBottomRight = markers[3];

         Vector3 backTopLeft = markers[4];
         Vector3 backTopRight = markers[5];
         Vector3 backBottomLeft = markers[6];
         Vector3 backBottomRight = markers[7];

         // Front square
         Debug.DrawLine(frontTopLeft, frontTopRight, color);
         Debug.DrawLine(frontTopRight, frontBottomRight, color);
         Debug.DrawLine(frontBottomRight, frontBottomLeft, color);
         Debug.DrawLine(frontBottomLeft, frontTopLeft, color);

         // Back square
         Debug.DrawLine(backTopLeft, backTopRight, color);
         Debug.DrawLine(backTopRight, backBottomRight, color);
         Debug.DrawLine(backBottomRight, backBottomLeft, color);
         Debug.DrawLine(backBottomLeft, backTopLeft, color);

         //  Edges
         Debug.DrawLine(frontTopLeft, backTopLeft, color);
         Debug.DrawLine(frontTopRight, backTopRight, color);
         Debug.DrawLine(frontBottomLeft, backBottomLeft, color);
         Debug.DrawLine(frontBottomRight, backBottomRight, color);

         if (useLineRenderer) {
           // Front square
           lineRenderer.SetPosition(0, frontTopLeft);
           lineRenderer.SetPosition(1, frontTopRight);
           lineRenderer.SetPosition(2, frontTopRight);
           lineRenderer.SetPosition(3, frontBottomRight);
           lineRenderer.SetPosition(4, frontBottomRight);
           lineRenderer.SetPosition(5, frontBottomLeft);
           lineRenderer.SetPosition(6, frontTopLeft);

           // Back square
           lineRenderer.SetPosition(7, backTopLeft);
           lineRenderer.SetPosition(8, backTopRight);
           lineRenderer.SetPosition(9, backTopRight);
           lineRenderer.SetPosition(10, backBottomRight);
           lineRenderer.SetPosition(11, backBottomRight);
           lineRenderer.SetPosition(12, backBottomLeft);
           lineRenderer.SetPosition(13, backBottomLeft);
           lineRenderer.SetPosition(14, backTopLeft);
           lineRenderer.SetPosition(15, backBottomLeft);

           //  Missing parts
           lineRenderer.SetPosition(16, frontBottomLeft);
           lineRenderer.SetPosition(17, frontBottomRight);
           lineRenderer.SetPosition(18, backBottomRight);
           lineRenderer.SetPosition(19, backTopRight);
           lineRenderer.SetPosition(20, backTopRight);
           lineRenderer.SetPosition(21, frontTopRight);
         }
       }
     }
#endif

     public void ToggleVisibility(bool enabled) {
#if UNITY_EDITOR
       if (lineRenderer != null)
         lineRenderer.enabled = enabled;
#endif
     }

     public void ToggleColor(bool over) {
#if UNITY_EDITOR
       if (lineRenderer != null)
         lineRenderer.material.color = over ? Color.green : Color.white;
#endif
     }

#if UNITY_EDITOR
     void OnDestroy() {
       if (useLineRenderer && lineRenderer != null)
         Destroy(lineRenderer.gameObject);
     }
#endif
   }
 }
