using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TrailSpawn : MonoBehaviour
{
    public GameObject trailObject;
    public GameObject trailHeight;
    public GameObject trailDist;
    public GameObject trailAngle;
    public float spawnInterval = 0f;
    public Rigidbody parent;

    private bool HeightLock = false;
    private bool DistLock = false;
    private bool AngleLock = false;

    void Start()
    {
        StartCoroutine(SpawnTrail());
    }

    void Update()
    {
        if (!HeightLock && parent.velocity.y >= -0.1 && parent.velocity.y <= 0.1 && parent.position.y > 1)
        {
            Instantiate(trailHeight, transform.position, Quaternion.identity);
            HeightLock = true;
        }
        if (!DistLock && parent.position.y <= 0.1 && parent.position.y >= -0.1 && parent.position.z > 1)
        {
            Instantiate(trailDist, transform.position, Quaternion.identity);
            DistLock = true;
        }
        // if (!AngleLock && parent.velocity.y > 0)
        // {
        //     Instantiate(trailAngle, transform.position, Quaternion.identity);
        //     AngleLock = true;
        // }
    }

    IEnumerator SpawnTrail()
    {
        while (true)
        {
            if (parent.transform.position.y >= 0)
            {
                Instantiate(trailObject, transform.position, Quaternion.identity);
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
