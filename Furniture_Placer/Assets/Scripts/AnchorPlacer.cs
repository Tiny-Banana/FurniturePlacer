using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class AnchorPlacer : MonoBehaviour
{
    [SerializeField] private GameObject prefabToAnchor;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private GameObject selectedObject = null;
    private bool isDragging = false;
    private Vector2 touchPosition;
    private ARRaycastManager arRaycastManager;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool planeVisualizer = false;
    private ARPlaneManager planeManager;
    private GestureManager gestureManager;
    private bool isRotating = false; 


    void Start() 
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
        planeManager.SetTrackablesActive(planeVisualizer);
        gestureManager = GetComponent<GestureManager>();  


        gestureManager.OnRotate += (sender, args) => {
        if (isDragging)
        {
            selectedObject.transform.Rotate(Vector3.up, args.Angle, Space.World);
        }
     }; 
    }

 
    // Update is called once per frame
    void Update()
    {
      foreach (var plane in planeManager.trackables) {
          plane.gameObject.SetActive(planeVisualizer);
      }

    
      if (Input.touchCount == 0)
        return;

    

    // Handle rotation gesture if two fingers are touching
    if (Input.touchCount == 2)
    {
      isRotating = true;
    }
    else
    {
        isRotating = false; // End rotation when less than two touches
    }

    // Single touch handling for dragging and placement
    if (Input.touchCount == 1 && !isRotating)
    {
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            if (isDragging)
            {
                isDragging = false;
                selectedObject = null;
                return;
            }

            if (SelectObject(touch.position))
            {
                isDragging = true;
            }
            else
            {
                PlaceObject(touch.position);
            }
        }

        // Only update dragging if no rotation is occurring
        if (isDragging && selectedObject != null && !isRotating)
        {
            UpdateDraggedObjectPosition(touch.position);
        }
    }
    }

    private bool SelectObject(Vector2 touchPosition) {  
       Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (spawnedObjects.Contains(hit.collider.gameObject))
            {
                selectedObject = hit.collider.gameObject; // Set the tapped object as selected
                return true;
            }
        }
        selectedObject = null;
        return false;
    }

    private void PlaceObject(Vector2 touchPosition) {
       if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                var hitPose = hits[0].pose;
                GameObject spawnedObject = Instantiate(prefabToAnchor, hitPose.position, hitPose.rotation);
                spawnedObjects.Add(spawnedObject);
            }
        }
    }

    private void UpdateDraggedObjectPosition(Vector2 touchPosition)
    {
        if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hits[0].pose;
            selectedObject.transform.position = hitPose.position;
            selectedObject.transform.rotation = hitPose.rotation;
        }
    }

    public void ChangeAnchor(GameObject anchor) {
        prefabToAnchor = anchor;
    }
   
    public void TogglePlaneVisualizer() {
        planeVisualizer = !planeVisualizer;
        planeManager.SetTrackablesActive(planeVisualizer);
    }
}