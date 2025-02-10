using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class ARObjectPlacer : MonoBehaviour
{
    [SerializeField] private GameObject spawnPrefab;
    [SerializeField] private bool disablePlaneDetectionAfterSpawn = true;

    private GameObject spawnedObject;
    private ARRaycastManager arRaycastManager;
    private ARPlaneManager arPlaneManager;
    private bool isObjectSpawned;
    private Vector2[] touchPositions = new Vector2[2];
    private float previousTouchDistance;

    private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Awake()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        arPlaneManager = GetComponent<ARPlaneManager>();
    }

    private void Update()
    {
        if (!isObjectSpawned && TryGetTouchPosition(out Vector2 touchPosition))
        {
            HandleObjectPlacement(touchPosition);
        }

        if (isObjectSpawned && Input.touchCount == 2)
        {
            HandleObjectScaling();
        }
    }

    private bool TryGetTouchPosition(out Vector2 touchPosition)
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }
        touchPosition = default;
        return false;
    }

    private void HandleObjectPlacement(Vector2 touchPosition)
    {
        if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hits[0].pose;
            var plane = arPlaneManager.GetPlane(hits[0].trackableId);

            if (plane != null)
            {
                spawnedObject = Instantiate(spawnPrefab, hitPose.position, hitPose.rotation);
                isObjectSpawned = true;
                
                if (disablePlaneDetectionAfterSpawn)
                {
                    TogglePlaneDetection(false);
                }

                Debug.Log($"Object placed on {plane.alignment} plane");
            }
        }
    }

    private void HandleObjectScaling()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
        {
            touchPositions[0] = touch0.position;
            touchPositions[1] = touch1.position;

            float currentTouchDistance = Vector2.Distance(touchPositions[0], touchPositions[1]);
            
            if (previousTouchDistance > 0)
            {
                float scaleFactor = currentTouchDistance / previousTouchDistance;
                spawnedObject.transform.localScale *= scaleFactor;
            }
            previousTouchDistance = currentTouchDistance;
        }
        else if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended)
        {
            previousTouchDistance = 0;
        }
    }

    private void TogglePlaneDetection(bool enable)
    {
        arPlaneManager.enabled = enable;
        foreach (var plane in arPlaneManager.trackables)
        {
            plane.gameObject.SetActive(enable);
        }
    }

    public void ResetSession()
    {
        if (spawnedObject != null) Destroy(spawnedObject);
        isObjectSpawned = false;
        TogglePlaneDetection(true);
    }
}