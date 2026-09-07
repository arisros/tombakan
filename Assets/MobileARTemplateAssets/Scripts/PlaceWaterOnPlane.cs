using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PlaceWaterOnPlane : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    public ARPointCloudManager pointCloudManager;
    public GameObject waterPlane;
    public FishSpawner fishSpawner;

    static List<ARRaycastHit> hits = new();

    void Update()
    {
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
            enabled = false; // one-shot placement; prevents mid-game re-positioning
        }
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
                {
                    feather.enabled = false;
                }
                if (collider)
                    collider.enabled = false;
            }
        }

        if (pointCloudManager != null)
        {
            pointCloudManager.enabled = false;
        }
    }

    /// <summary>
    /// Resets placement so the player can pick a new AR surface.
    /// Called by the "Posisi ulang" button in the pre-game HUD.
    /// </summary>
    public void Reposition()
    {
        waterPlane.SetActive(false);
        if (fishSpawner != null) fishSpawner.ClearAll();

        if (planeManager != null)
            planeManager.enabled = true;
        if (pointCloudManager != null)
            pointCloudManager.enabled = true;

        enabled = true; // re-arm Update() to accept the next tap
    }
}
