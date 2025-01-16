using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class Raycast_script : MonoBehaviour
{
    public GameObject spawn_prefab;

    private GameObject spawned_object;
    private bool object_spawned;
    private ARRaycastManager arRaycastManager;
    private ARPlaneManager arPlaneManager;

    private Vector2 first_touch;
    private Vector2 second_touch;

    private float distance_current;
    private float distance_previous;

    private bool first_pinch = true;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        object_spawned = false;
        arRaycastManager = GetComponent<ARRaycastManager>();
        arPlaneManager = GetComponent<ARPlaneManager>();

        if (arRaycastManager == null)
        {
            Debug.LogError("ARRaycastManager is missing from the GameObject.");
        }

        if (arPlaneManager == null)
        {
            Debug.LogError("ARPlaneManager is missing from the GameObject.");
        }
    }

    void Update()
    {
        HandleSingleTouch();
        HandlePinchToScale();
    }

    private void HandleSingleTouch()
    {
        if (Input.touchCount == 1 && !object_spawned)
        {
            Touch touch = Input.GetTouch(0);

            // Raycast to detect planes (horizontal and vertical included in PlaneWithinPolygon)
            if (arRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
            {
                var hitPose = hits[0].pose;
                var hitTrackableId = hits[0].trackableId;

                // Get the ARPlane associated with the hit
                var plane = arPlaneManager.GetPlane(hitTrackableId);

                if (plane != null)
                {
                    // Check plane alignment
                    if (plane.alignment == PlaneAlignment.HorizontalUp || plane.alignment == PlaneAlignment.HorizontalDown)
                    {
                        Debug.Log("Horizontal plane detected.");
                    }
                    else if (plane.alignment == PlaneAlignment.Vertical)
                    {
                        Debug.Log("Vertical plane detected.");
                    }

                    // Spawn object at the hit position
                    spawned_object = Instantiate(spawn_prefab, hitPose.position, hitPose.rotation);
                    object_spawned = true;
                }
            }
        }
    }

    private void HandlePinchToScale()
    {
        if (Input.touchCount == 2 && object_spawned)
        {
            first_touch = Input.GetTouch(0).position;
            second_touch = Input.GetTouch(1).position;

            distance_current = Vector2.Distance(first_touch, second_touch);

            if (first_pinch)
            {
                distance_previous = distance_current;
                first_pinch = false;
            }

            if (Mathf.Abs(distance_current - distance_previous) > Mathf.Epsilon) // Avoid small unnecessary changes
            {
                float scale_factor = distance_current / distance_previous;
                Vector3 new_scale = spawned_object.transform.localScale * scale_factor;
                spawned_object.transform.localScale = new_scale;

                distance_previous = distance_current;
            }
        }
        else
        {
            first_pinch = true; // Reset pinch state when touch count is less than 2
        }
    }
}
