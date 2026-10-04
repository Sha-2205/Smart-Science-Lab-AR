using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SmartScienceLab.PhysicsLab.ProjectileMotion
{
    public class ProjectileExperimentManager : MonoBehaviour
    {
        public enum ExperimentState
        {
            ScanningSurface,
            LauncherPlaced,
            InFlight,
            Completed
        }

        [Header("AR References")]
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARPlaneManager planeManager;
        [SerializeField] private Camera arCamera;

        [Header("Experiment Prefabs")]
        [SerializeField] private GameObject launcherPrefab;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private GameObject targetZonePrefab;

        [Header("Target Condition Settings")]
        [SerializeField] private float targetDistance = 5.0f; // Target distance in meters
        [SerializeField] private float targetTolerance = 0.4f; // Target tolerance window (±0.4 m)

        [Header("UI Reference")]
        [SerializeField] private PhysicsLabUI labUI;

        // Runtime Instances
        private LauncherController launcherInstance;
        private GameObject targetZoneInstance;
        private Projectile activeProjectile;
        private ExperimentState currentState = ExperimentState.ScanningSurface;

        private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

        public ExperimentState CurrentState => currentState;
        public float TargetDistance => targetDistance;

        private void Awake()
        {
            if (arCamera == null)
            {
                arCamera = Camera.main;
            }

            if (raycastManager == null)
            {
                raycastManager = FindAnyObjectByType<ARRaycastManager>();
            }

            if (planeManager == null)
            {
                planeManager = FindAnyObjectByType<ARPlaneManager>();
            }

            if (labUI == null)
            {
                labUI = FindAnyObjectByType<PhysicsLabUI>();
            }
        }

        private void Start()
        {
            if (labUI != null)
            {
                labUI.OnAngleChanged += HandleAngleChanged;
                labUI.OnVelocityChanged += HandleVelocityChanged;
                labUI.OnLaunchPressed += HandleLaunchPressed;
                labUI.OnRedoPressed += HandleRedoPressed;

                labUI.SetTargetDisplay(targetDistance, targetTolerance);
                labUI.SetStatusText("Scan room to detect a horizontal surface.");
                labUI.SetParameterControlsVisible(false);
                labUI.SetResultsVisible(false);
            }

            currentState = ExperimentState.ScanningSurface;
        }

        private void OnDestroy()
        {
            if (labUI != null)
            {
                labUI.OnAngleChanged -= HandleAngleChanged;
                labUI.OnVelocityChanged -= HandleVelocityChanged;
                labUI.OnLaunchPressed -= HandleLaunchPressed;
                labUI.OnRedoPressed -= HandleRedoPressed;
            }
        }

        private void Update()
        {
            switch (currentState)
            {
                case ExperimentState.ScanningSurface:
                    HandleSurfaceScanning();
                    break;
                case ExperimentState.LauncherPlaced:
                    // Ready for launch or adjusting parameters
                    break;
                case ExperimentState.InFlight:
                    // Projectile is moving
                    break;
                case ExperimentState.Completed:
                    // Waiting for student to review results and press REDO
                    break;
            }
        }

        private static readonly List<RaycastResult> s_UIHits = new List<RaycastResult>();

        private bool IsPointerOverUI(Vector2 screenPosition)
        {
            if (EventSystem.current == null) return false;

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };

            s_UIHits.Clear();
            EventSystem.current.RaycastAll(eventData, s_UIHits);
            return s_UIHits.Count > 0;
        }

        private void HandleSurfaceScanning()
        {
            bool hasPlanes = planeManager != null && planeManager.trackables.count > 0;
            if (hasPlanes && labUI != null)
            {
                labUI.SetStatusText("Surface detected! Tap on a plane to place the launcher.");
            }

            // Detect user tap via new Input System (Touchscreen on Android, Pointer in Editor)
            Vector2 screenPoint = Vector2.zero;
            bool inputDetected = false;

            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.wasPressedThisFrame)
                {
                    inputDetected = true;
                    screenPoint = touch.position.ReadValue();
                }
            }

            if (!inputDetected && Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                inputDetected = true;
                screenPoint = Pointer.current.position.ReadValue();
            }

            if (!inputDetected || raycastManager == null) return;

            // Ignore tap if over screen UI
            if (IsPointerOverUI(screenPoint)) return;

            hits.Clear();
            if (raycastManager.Raycast(screenPoint, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                PlaceLauncherAndTarget(hitPose);
            }
        }

        private void PlaceLauncherAndTarget(Pose surfacePose)
        {
            if (arCamera == null) arCamera = Camera.main;

            // Determine forward heading: face away from the AR camera projected on the plane
            Vector3 camPos = arCamera != null ? arCamera.transform.position : surfacePose.position - Vector3.forward;
            Vector3 planeForward = Vector3.ProjectOnPlane(surfacePose.position - camPos, surfacePose.up).normalized;
            if (planeForward.sqrMagnitude < 0.001f)
            {
                planeForward = Vector3.forward;
            }

            // Spawn or reposition Launcher
            if (launcherInstance == null && launcherPrefab != null)
            {
                GameObject launcherObj = Instantiate(launcherPrefab, surfacePose.position, Quaternion.LookRotation(planeForward, surfacePose.up));
                launcherInstance = launcherObj.GetComponent<LauncherController>();
            }
            else if (launcherInstance != null)
            {
                launcherInstance.AlignWithSurface(surfacePose.position, planeForward, surfacePose.up);
            }

            if (launcherInstance != null && labUI != null)
            {
                launcherInstance.SetAngle(labUI.GetCurrentAngle());
            }

            // Spawn or reposition Target Zone
            Vector3 targetPosition = surfacePose.position + planeForward * targetDistance;
            if (targetZoneInstance == null && targetZonePrefab != null)
            {
                targetZoneInstance = Instantiate(targetZonePrefab, targetPosition, Quaternion.LookRotation(planeForward, surfacePose.up));
            }
            else if (targetZoneInstance != null)
            {
                targetZoneInstance.transform.SetPositionAndRotation(targetPosition, Quaternion.LookRotation(planeForward, surfacePose.up));
            }

            currentState = ExperimentState.LauncherPlaced;

            if (labUI != null)
            {
                labUI.SetStatusText("Launcher placed! Set angle & velocity, then press Launch.");
                labUI.SetParameterControlsVisible(true);
                labUI.SetLaunchButtonInteractable(true);
            }
        }

        private void HandleAngleChanged(float angle)
        {
            if (launcherInstance != null)
            {
                launcherInstance.SetAngle(angle);
            }
        }

        private void HandleVelocityChanged(float velocity)
        {
            // Velocity updated
        }

        private void HandleLaunchPressed()
        {
            if (currentState != ExperimentState.LauncherPlaced || launcherInstance == null || projectilePrefab == null)
            {
                return;
            }

            float angle = labUI != null ? labUI.GetCurrentAngle() : 45f;
            float velocity = labUI != null ? labUI.GetCurrentVelocity() : 10f;

            // Instantiate projectile ball at launcher nozzle
            Vector3 spawnPos = launcherInstance.NozzlePosition;
            GameObject projObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            activeProjectile = projObj.GetComponent<Projectile>();

            if (activeProjectile != null)
            {
                activeProjectile.OnLanded += HandleProjectileLanded;
                activeProjectile.Launch(
                    velocity,
                    angle,
                    launcherInstance.LaunchForward,
                    launcherInstance.SurfaceNormal,
                    spawnPos
                );
            }

            currentState = ExperimentState.InFlight;

            if (labUI != null)
            {
                labUI.SetStatusText("Observing ballistic flight...");
                labUI.SetParameterControlsVisible(false);
                labUI.SetLaunchButtonInteractable(false);
            }
        }

        private void HandleProjectileLanded(Projectile p)
        {
            if (p != null)
            {
                p.OnLanded -= HandleProjectileLanded;
            }

            currentState = ExperimentState.Completed;

            float angle = p.LaunchAngle;
            float v0 = p.LaunchSpeed;
            float measH = p.MeasMaxHeight;
            float theoH = p.TheoMaxHeight;
            float measR = p.MeasRange;
            float theoR = p.TheoRange;
            float measT = p.MeasTimeOfFlight;
            float theoT = p.TheoTimeOfFlight;

            // Deterministic educational evaluation relative to target condition
            float deltaR = measR - targetDistance;
            string evalTitle;
            string evalExplanation;
            string evalHint;

            if (Mathf.Abs(deltaR) <= targetTolerance)
            {
                evalTitle = "<color=#2ECC71>Target Achieved!</color>";
                evalExplanation = $"The projectile landed at {measR:F2} m (target was {targetDistance:F1} m ± {targetTolerance:F1} m). " +
                                  $"The combination of angle ({angle:F0}°) and initial velocity ({v0:F1} m/s) yielded the necessary horizontal and vertical components.";
                evalHint = "Kinematic Insight: On level ground (y0 = y_final), theoretical maximum range occurs at approximately 45° where sin(2θ) = 1. " +
                           "Any achievable range below maximum can also be reached using complementary launch angles (θ and 90° - θ) with appropriate velocity.";
            }
            else if (deltaR < -targetTolerance)
            {
                // Undershot target
                evalTitle = $"<color=#E74C3C>Undershot Target by {Mathf.Abs(deltaR):F2} m</color>";

                if (angle > 65f)
                {
                    evalExplanation = $"At a steep launch angle of {angle:F0}°, most initial velocity is converted into vertical velocity (vy = {p.TheoVy:F1} m/s). " +
                                      $"While this yields a high peak ({measH:F2} m) and long flight time ({measT:F2} s), the horizontal velocity (vx = {p.TheoVx:F1} m/s) is too low to cover {targetDistance:F1} m.";
                    evalHint = "Adjusting Strategy: Try lowering the launch angle closer towards 45° to divert more velocity into the horizontal component, or increase initial velocity.";
                }
                else if (angle < 30f)
                {
                    evalExplanation = $"At a shallow launch angle of {angle:F0}°, vertical velocity (vy = {p.TheoVy:F1} m/s) is small. " +
                                      $"Gravity pulls the projectile to the ground in a short flight time ({measT:F2} s), stopping horizontal translation before reaching {targetDistance:F1} m.";
                    evalHint = "Adjusting Strategy: Try increasing the launch angle towards 45° to provide greater flight time for horizontal travel, or increase initial velocity.";
                }
                else
                {
                    evalExplanation = $"At {angle:F0}°, initial velocity ({v0:F1} m/s) was insufficient to achieve the {targetDistance:F1} m target. " +
                                      $"Horizontal range scales quadratically with initial velocity (R = v0²·sin(2θ) / g).";
                    evalHint = "Adjusting Strategy: Increase the initial velocity slider, or fine-tune the angle towards 45° to maximize range for this velocity.";
                }
            }
            else
            {
                // Overshot target
                evalTitle = $"<color=#E67E22>Overshot Target by {deltaR:F2} m</color>";
                evalExplanation = $"The projectile traveled {measR:F2} m, exceeding the {targetDistance:F1} m target. " +
                                  $"The horizontal velocity component and flight time together imparted greater horizontal displacement than required.";
                evalHint = "Adjusting Strategy: Try decreasing the initial velocity slider, or adjust the angle away from 45° to reduce horizontal range.";
            }

            if (labUI != null)
            {
                labUI.SetStatusText("Experiment completed. Review measurements and press REDO to adjust parameters.");
                labUI.DisplayResults(
                    angle, v0,
                    measH, theoH,
                    measR, theoR,
                    measT, theoT,
                    evalTitle, evalExplanation, evalHint
                );
            }
        }

        public void HandleRedoPressed()
        {
            // Strict REDO Behavior:
            // 1. Destroy active projectile
            // 2. Clear all trajectory visual markers and billboards
            // 3. Clear results card
            // 4. Return to Parameter Setup
            // 5. Preserves AR Session, detected planes, and launcher transform completely!

            if (activeProjectile != null)
            {
                ProjectileTrail trail = activeProjectile.GetComponent<ProjectileTrail>();
                if (trail != null)
                {
                    trail.ClearVisuals();
                }

                Destroy(activeProjectile.gameObject);
                activeProjectile = null;
            }

            if (labUI != null)
            {
                labUI.SetResultsVisible(false);
                labUI.SetParameterControlsVisible(true);
                labUI.SetLaunchButtonInteractable(true);
                labUI.SetStatusText("Parameters ready. Adjust angle or velocity, then press Launch.");
            }

            currentState = ExperimentState.LauncherPlaced;
        }
    }
}
