using System;
using UnityEngine;

namespace SmartScienceLab.PhysicsLab.ProjectileMotion
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        [Header("State")]
        private Rigidbody rb;
        private Vector3 launchOrigin;
        private Vector3 launchForward;
        private Vector3 planeNormal = Vector3.up;

        private float launchSpeed;
        private float launchAngle;
        private bool isLaunched = false;
        private bool hasLanded = false;
        private bool apexDetected = false;

        private float elapsedTime = 0f;
        private float maxObservedHeight = 0f;
        private float measuredRange = 0f;
        private float measuredFlightTime = 0f;

        // Kinematic theoretical calculations
        public float LaunchAngle => launchAngle;
        public float LaunchSpeed => launchSpeed;
        public float Gravity => Mathf.Abs(Physics.gravity.y);

        public float TheoVx => launchSpeed * Mathf.Cos(launchAngle * Mathf.Deg2Rad);
        public float TheoVy => launchSpeed * Mathf.Sin(launchAngle * Mathf.Deg2Rad);
        public float TheoTimeOfFlight => (2f * TheoVy) / Mathf.Max(Gravity, 0.001f);
        public float TheoMaxHeight => (TheoVy * TheoVy) / (2f * Mathf.Max(Gravity, 0.001f));
        public float TheoRange => (launchSpeed * launchSpeed * Mathf.Sin(2f * launchAngle * Mathf.Deg2Rad)) / Mathf.Max(Gravity, 0.001f);

        // Measured values
        public float MeasMaxHeight => maxObservedHeight;
        public float MeasRange => measuredRange;
        public float MeasTimeOfFlight => measuredFlightTime;
        public bool HasLanded => hasLanded;
        public bool IsLaunched => isLaunched;

        // Events
        public event Action<float, Vector3> OnApexReached;
        public event Action<Projectile> OnLanded;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void Launch(float speed, float angleDegrees, Vector3 forwardDir, Vector3 surfaceNormal, Vector3 origin)
        {
            launchSpeed = speed;
            launchAngle = Mathf.Clamp(angleDegrees, 5f, 85f);
            launchForward = Vector3.ProjectOnPlane(forwardDir, surfaceNormal).normalized;
            planeNormal = surfaceNormal.normalized;
            launchOrigin = origin;

            transform.position = origin;

            float rad = launchAngle * Mathf.Deg2Rad;
            Vector3 initialVelocity = (launchForward * Mathf.Cos(rad) + planeNormal * Mathf.Sin(rad)) * launchSpeed;

            rb.isKinematic = false;
            rb.linearVelocity = initialVelocity;

            isLaunched = true;
            hasLanded = false;
            apexDetected = false;
            elapsedTime = 0f;
            maxObservedHeight = 0f;
        }

        private void FixedUpdate()
        {
            if (!isLaunched || hasLanded) return;

            elapsedTime += Time.fixedDeltaTime;

            // Height relative to the launch contact plane
            Vector3 currentDisp = transform.position - launchOrigin;
            float currentRelativeHeight = Vector3.Dot(currentDisp, planeNormal);

            if (currentRelativeHeight > maxObservedHeight)
            {
                maxObservedHeight = currentRelativeHeight;
            }

            // Apex detection: vertical velocity relative to surface normal crosses zero
            float verticalVelocity = Vector3.Dot(rb.linearVelocity, planeNormal);
            if (!apexDetected && verticalVelocity <= 0.05f && elapsedTime > 0.05f)
            {
                apexDetected = true;
                OnApexReached?.Invoke(maxObservedHeight, transform.position);
            }

            // Ground plane crossing check (after initial takeoff)
            if (elapsedTime > 0.15f && currentRelativeHeight <= 0f)
            {
                // Clamp position to plane for clean landing readout
                Vector3 planarDisp = Vector3.ProjectOnPlane(transform.position - launchOrigin, planeNormal);
                transform.position = launchOrigin + planarDisp;
                CompleteLanding();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isLaunched || hasLanded) return;

            if (elapsedTime > 0.1f)
            {
                CompleteLanding();
            }
        }

        private void CompleteLanding()
        {
            if (hasLanded) return;
            hasLanded = true;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;

            Vector3 finalDisp = transform.position - launchOrigin;
            Vector3 planarDisp = Vector3.ProjectOnPlane(finalDisp, planeNormal);
            measuredRange = planarDisp.magnitude;
            measuredFlightTime = elapsedTime;

            OnLanded?.Invoke(this);
        }
    }
}
