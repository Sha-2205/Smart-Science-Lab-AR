using UnityEngine;

namespace SmartScienceLab.PhysicsLab.ProjectileMotion
{
    public class LauncherController : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform barrelPivot;
        [SerializeField] private Transform nozzlePoint;
        [SerializeField] private Transform groundRing;

        [Header("State")]
        private float currentAngle = 45f;

        public Vector3 NozzlePosition => nozzlePoint != null ? nozzlePoint.position : transform.position;
        public Vector3 LaunchForward => Vector3.ProjectOnPlane(transform.forward, transform.up).normalized;
        public Vector3 SurfaceNormal => transform.up;
        public float CurrentAngle => currentAngle;

        private void Awake()
        {
            if (barrelPivot == null)
            {
                // Fallback to find child named BarrelPivot or Barrel
                Transform pivot = transform.Find("BarrelPivot");
                if (pivot != null) barrelPivot = pivot;
                else barrelPivot = transform;
            }

            if (nozzlePoint == null)
            {
                Transform nozzle = transform.Find("Nozzle");
                if (nozzle == null && barrelPivot != null)
                {
                    nozzle = barrelPivot.Find("Nozzle");
                }
                nozzlePoint = nozzle != null ? nozzle : barrelPivot;
            }
        }

        public void SetAngle(float angleDegrees)
        {
            currentAngle = Mathf.Clamp(angleDegrees, 5f, 85f);
            if (barrelPivot != null)
            {
                // In Unity local space, negative X rotation pitches the barrel upwards
                barrelPivot.localRotation = Quaternion.Euler(-currentAngle, 0f, 0f);
            }
        }

        public void AlignWithSurface(Vector3 position, Vector3 forwardDirection, Vector3 surfaceNormal)
        {
            transform.position = position;
            Vector3 planarForward = Vector3.ProjectOnPlane(forwardDirection, surfaceNormal).normalized;
            if (planarForward.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(planarForward, surfaceNormal);
            }
        }
    }
}
