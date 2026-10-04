#if AR_FOUNDATION_PRESENT
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets
{
    /// <summary>
    /// Spawns an object at an <see cref="IARInteractor"/>'s raycast hit position when a trigger is activated.
    /// Handles AR tracking validation, plane detection confirmation, UI raycast suppression, and reliable touch-to-plane placement.
    /// </summary>
    public class ARInteractorSpawnTrigger : MonoBehaviour
    {
        /// <summary>
        /// The type of trigger to use to spawn an object.
        /// </summary>
        public enum SpawnTriggerType
        {
            /// <summary>
            /// Spawn an object when the interactor activates its select input
            /// but no selection actually occurs.
            /// </summary>
            SelectAttempt,

            /// <summary>
            /// Spawn an object when an input is performed.
            /// </summary>
            InputAction,
        }

        [SerializeField]
        [Tooltip("The AR ray interactor that determines where to spawn the object.")]
        XRRayInteractor m_ARInteractor;

        /// <summary>
        /// The AR ray interactor that determines where to spawn the object.
        /// </summary>
        public XRRayInteractor arInteractor
        {
            get => m_ARInteractor;
            set => m_ARInteractor = value;
        }

        [SerializeField]
        [Tooltip("Whether to require that the AR Interactor hits an AR Plane with a horizontal up alignment in order to spawn anything.")]
        bool m_RequireHorizontalUpSurface = true;

        /// <summary>
        /// Whether to require that the <see cref="IARInteractor"/> hits an <see cref="ARPlane"/> with an alignment of
        /// <see cref="PlaneAlignment.HorizontalUp"/> in order to spawn anything.
        /// </summary>
        public bool requireHorizontalUpSurface
        {
            get => m_RequireHorizontalUpSurface;
            set => m_RequireHorizontalUpSurface = value;
        }

        [SerializeField]
        [Tooltip("The type of trigger to use to spawn an object, either when the Interactor's select action occurs or " +
            "when a button input is performed.")]
        SpawnTriggerType m_SpawnTriggerType;

        /// <summary>
        /// The type of trigger to use to spawn an object.
        /// </summary>
        public SpawnTriggerType spawnTriggerType
        {
            get => m_SpawnTriggerType;
            set => m_SpawnTriggerType = value;
        }

        [SerializeField]
        XRInputButtonReader m_SpawnObjectInput = new XRInputButtonReader("Spawn Object");

        /// <summary>
        /// The input used to trigger spawn, if <see cref="spawnTriggerType"/> is set to <see cref="SpawnTriggerType.InputAction"/>.
        /// </summary>
        public XRInputButtonReader spawnObjectInput
        {
            get => m_SpawnObjectInput;
            set => XRInputReaderUtility.SetInputProperty(ref m_SpawnObjectInput, value, this);
        }

        [SerializeField]
        [Tooltip("When enabled, spawn will not be triggered if an object is currently selected.")]
        bool m_BlockSpawnWhenInteractorHasSelection = true;

        /// <summary>
        /// When enabled, spawn will not be triggered if an object is currently selected.
        /// </summary>
        public bool blockSpawnWhenInteractorHasSelection
        {
            get => m_BlockSpawnWhenInteractorHasSelection;
            set => m_BlockSpawnWhenInteractorHasSelection = value;
        }

        /// <summary>
        /// Calls the methods in its invocation list when an object is spawned.
        /// </summary>
        /// <remarks>
        /// The first event parameter corresponds to the spawn position in world space
        /// and the second event parameter corresponds to the vector normal to the surface.
        /// </remarks>
        public UnityEvent<Vector3, Vector3> objectSpawnTriggered
        {
            get => m_ObjectSpawnTriggered;
            set => m_ObjectSpawnTriggered = value;
        }

        [Header("Events")]
        [SerializeField]
        [Tooltip("Calls the methods in its invocation list when an object is spawned.")]
        UnityEvent<Vector3, Vector3> m_ObjectSpawnTriggered = new UnityEvent<Vector3, Vector3>();

        [Header("AR Managers")]
        [SerializeField]
        ARRaycastManager m_RaycastManager;

        [SerializeField]
        ARPlaneManager m_PlaneManager;

        [SerializeField]
        ARCameraManager m_CameraManager;

        [SerializeField]
        ARSession m_ARSession;

        static readonly List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
        static readonly List<RaycastResult> s_UIHits = new List<RaycastResult>();

        int m_LastPlaneCount = -1;
        TrackingState m_LastTrackingState = TrackingState.None;
        ARSessionState m_LastSessionState = ARSessionState.None;

        void OnEnable()
        {
            m_SpawnObjectInput.EnableDirectActionIfModeUsed();
            ResolveARManagers();

            ARSession.stateChanged += OnARSessionStateChanged;
        }

        void OnDisable()
        {
            m_SpawnObjectInput.DisableDirectActionIfModeUsed();

            ARSession.stateChanged -= OnARSessionStateChanged;
        }

        void Start()
        {
            ResolveARManagers();
            LogDiagnosticsInitial();
        }

        void ResolveARManagers()
        {
            if (m_RaycastManager == null)
                m_RaycastManager = FindAnyObjectByType<ARRaycastManager>();

            if (m_PlaneManager == null)
                m_PlaneManager = FindAnyObjectByType<ARPlaneManager>();

            if (m_CameraManager == null)
                m_CameraManager = FindAnyObjectByType<ARCameraManager>();

            if (m_ARSession == null)
                m_ARSession = FindAnyObjectByType<ARSession>();
        }

        void LogDiagnosticsInitial()
        {
            m_LastSessionState = ARSession.state;
            Debug.Log($"[AR DEBUG] Session state: {m_LastSessionState}");

            int planeCount = m_PlaneManager != null ? m_PlaneManager.trackables.count : 0;
            m_LastPlaneCount = planeCount;
            Debug.Log($"[AR DEBUG] Plane count: {m_LastPlaneCount}");
        }

        void OnARSessionStateChanged(ARSessionStateChangedEventArgs args)
        {
            if (args.state != m_LastSessionState)
            {
                m_LastSessionState = args.state;
                Debug.Log($"[AR DEBUG] Session state: {m_LastSessionState}");
            }
        }

        void Update()
        {
            // Track AR tracking state changes
            if (m_ARSession != null && m_ARSession.subsystem != null)
            {
                var trackingState = m_ARSession.subsystem.trackingState;
                if (trackingState != m_LastTrackingState)
                {
                    m_LastTrackingState = trackingState;
                    Debug.Log($"[AR DEBUG] Tracking state: {m_LastTrackingState}");
                }
            }

            // Track plane count changes
            if (m_PlaneManager != null)
            {
                int currentPlaneCount = m_PlaneManager.trackables.count;
                if (currentPlaneCount != m_LastPlaneCount)
                {
                    m_LastPlaneCount = currentPlaneCount;
                    Debug.Log($"[AR DEBUG] Plane count: {m_LastPlaneCount}");
                }
            }

            // Do not spawn if interactor is currently selecting an object
            if (m_BlockSpawnWhenInteractorHasSelection && m_ARInteractor != null && m_ARInteractor.hasSelection)
                return;

            // Detect user tap
            bool hasTap = false;
            Vector2 screenPosition = Vector2.zero;

            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.wasPressedThisFrame)
                {
                    hasTap = true;
                    screenPosition = touch.position.ReadValue();
                }
            }

            if (!hasTap && Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                hasTap = true;
                screenPosition = Pointer.current.position.ReadValue();
            }

            if (!hasTap && m_SpawnTriggerType == SpawnTriggerType.InputAction && m_SpawnObjectInput.ReadWasPerformedThisFrame())
            {
                hasTap = true;
                screenPosition = Pointer.current != null ? Pointer.current.position.ReadValue() : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            }

            if (!hasTap && m_SpawnTriggerType == SpawnTriggerType.SelectAttempt && m_ARInteractor != null && m_ARInteractor.logicalSelectState.wasCompletedThisFrame)
            {
                hasTap = true;
                screenPosition = Pointer.current != null ? Pointer.current.position.ReadValue() : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            }

            if (!hasTap)
                return;

            // 1. Check if tap is over screen-space UI
            if (IsPointerOverUI(screenPosition))
                return;

            // 2. Wait until AR Session is tracking
            if (ARSession.state != ARSessionState.SessionTracking)
            {
                Debug.Log($"[AR DEBUG] Tap ignored: AR session is not tracking yet (current state: {ARSession.state}).");
                return;
            }

            // 3. Verify at least one real plane is detected
            if (m_PlaneManager == null || m_PlaneManager.trackables.count == 0)
            {
                Debug.Log("[AR DEBUG] Tap ignored: No planes detected yet. Move phone across a textured surface.");
                return;
            }

            // 4. Perform ARRaycastManager.Raycast using PlaneWithinPolygon
            if (m_RaycastManager == null)
            {
                ResolveARManagers();
                if (m_RaycastManager == null)
                {
                    Debug.LogError("[AR DEBUG] Missing ARRaycastManager in the scene.");
                    return;
                }
            }

            s_Hits.Clear();
            m_RaycastManager.Raycast(screenPosition, s_Hits, TrackableType.PlaneWithinPolygon);
            Debug.Log($"[AR DEBUG] Raycast hits: {s_Hits.Count}");

            if (s_Hits.Count == 0)
                return;

            // 5. Obtain valid pose and plane alignment
            var hit = s_Hits[0];
            var hitPose = hit.pose;

            ARPlane hitPlane = null;
            if (m_PlaneManager != null)
                hitPlane = m_PlaneManager.GetPlane(hit.trackableId);

            if (m_RequireHorizontalUpSurface && hitPlane != null && hitPlane.alignment != PlaneAlignment.HorizontalUp)
            {
                Debug.Log($"[AR DEBUG] Hit plane alignment is {hitPlane.alignment}, but HorizontalUp is required.");
                return;
            }

            Vector3 spawnNormal = hitPlane != null ? hitPlane.normal : hitPose.up;

            // 6. Spawn or reposition cube at hit pose
            m_ObjectSpawnTriggered.Invoke(hitPose.position, spawnNormal);
        }

        bool IsPointerOverUI(Vector2 screenPosition)
        {
            if (EventSystem.current == null)
                return false;

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };

            s_UIHits.Clear();
            EventSystem.current.RaycastAll(eventData, s_UIHits);
            return s_UIHits.Count > 0;
        }
    }
}
#endif
