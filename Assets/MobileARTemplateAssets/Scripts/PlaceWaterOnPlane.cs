using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceWaterOnPlane : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public GameObject waterPlane;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
            return;

        if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            // POSISI: nempel lantai + offset tipis
            waterPlane.transform.position = hitPose.position + Vector3.up * 0.01f;

            // ROTASI: PAKSA DATAR (air selalu horizontal)
            waterPlane.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
