using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameManager gm;
    public float angleOfIncidence;
    public float initVelocity;
    public float initForce;

    public float maxHeight = 1;
    public float maxDist;
    public float totalTime;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ThrowButton();
        }
    }

    public void ThrowButton()
    {
        angleOfIncidence = gm.inputAngle;
        ApplyForce(initForce, angleOfIncidence);
    }

    void FixedUpdate()
    {
        if (initVelocity <= 0)
        {
            initVelocity = rb.velocity.y;
            float gravity = Mathf.Abs(Physics.gravity.y);
            totalTime = 2f * initVelocity * Mathf.Sin(angleOfIncidence * Mathf.Deg2Rad) / gravity;

            maxDist = initVelocity * Mathf.Cos(angleOfIncidence * Mathf.Deg2Rad) * totalTime;
            maxHeight = Mathf.Pow(initVelocity * Mathf.Sin(angleOfIncidence * Mathf.Deg2Rad), 2) / (2 * gravity);
        }
        // Debug.Log("vel:" + initVelocity);
    }

    public void ApplyForce(float _force, float _angleOfIncidence)
    {
        if (angleOfIncidence >= 0)
        {
            // Convert angle of incidence to radians and then to direction vector
            float angleRad = _angleOfIncidence * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(0f, Mathf.Sin(angleRad), Mathf.Cos(angleRad)).normalized;
            rb.AddForce(direction * _force, mode: ForceMode.VelocityChange);
        }
    }
}
