using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PlaceWaterOnPlane : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    public ARPointCloudManager pointCloudManager;
    public GameObject waterPlane;

    [Header("Reposition UI (optional/null-safe)")]
    public GameObject repositionButton;   // shown after placement; onClick → BeginReposition()

    static List<ARRaycastHit> hits = new();

    bool _placed;

    void Update()
    {
        if (_placed) return;   // locked after placement until BeginReposition() is called

        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
            return;

        if (
            raycastManager.Raycast(
                touch.position,
                hits,
                UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon
            )
        )
        {
            Pose hitPose = hits[0].pose;

            waterPlane.transform.position = hitPose.position;
            waterPlane.SetActive(true);

            DisableARPlanes();
            _placed = true;
            if (repositionButton != null) repositionButton.SetActive(true);
        }
    }

    /// <summary>Re-enables AR plane scanning so the player can tap a new surface.</summary>
    public void BeginReposition()
    {
        _placed = false;
        if (repositionButton != null) repositionButton.SetActive(false);
        if (planeManager != null)     planeManager.enabled = true;
    }

    void DisableARPlanes()
    {
        if (planeManager != null)
        {
            planeManager.enabled = false;

            foreach (var plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(false);

                var meshVis = plane.GetComponent<ARPlaneMeshVisualizer>();
                if (meshVis)
                    meshVis.enabled = false;

                var collider = plane.GetComponent<MeshCollider>();
                var feather = plane.GetComponent("ARFeatheredPlaneMeshVisualizer") as Behaviour;
                if (feather != null)
                    feather.enabled = false;
                if (collider)
                    collider.enabled = false;
            }
        }

        if (pointCloudManager != null)
            pointCloudManager.enabled = false;
    }
}
