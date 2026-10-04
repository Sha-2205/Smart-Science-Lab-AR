using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;

public class SpawnManager : MonoBehaviour
{
    public GameObject objectToBeSpawned;
    public ARRaycastManager raycastManager;

    private void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            List<ARRaycastHit> gotHit = new List<ARRaycastHit>();
            raycastManager.Raycast(Input.GetTouch(0).position, gotHit, UnityEngine.XR.ARSubsystems.TrackableType.Planes);
            if (gotHit.Count > 0)
            {
                GameObject.Instantiate(objectToBeSpawned, gotHit[0].pose.position, gotHit[0].pose.rotation);
            }
            if (raycastManager.Raycast(Input.GetTouch(0).position, gotHit, UnityEngine.XR.ARSubsystems.TrackableType.Planes))
            {
                bool isTouchOverUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);

                if (!isTouchOverUI)
                {
                    SpawnPrefab(gotHit[0].pose.position);
                }
            }
        }
    }
    private void SpawnPrefab(Vector3 position)
    {
        Instantiate(objectToBeSpawned, position, Quaternion.identity);
    }
}
