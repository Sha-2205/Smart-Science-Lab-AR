using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;

namespace UnityEngine.XR.ARFoundation.Samples
{
    [RequireComponent(typeof(ARPlaneManager))]
    public class PlaneDetectionController : MonoBehaviour
    {
        [Tooltip("The TMP Text element used to display plane detection messages.")]
        [SerializeField]
        private TextMeshProUGUI m_TogglePlaneDetectionText;

        public TextMeshProUGUI togglePlaneDetectionText
        {
            get { return m_TogglePlaneDetectionText; }
            set { m_TogglePlaneDetectionText = value; }
        }

        public void TogglePlaneDetection()
        {
            m_ARPlaneManager.enabled = !m_ARPlaneManager.enabled;

            string planeDetectionMessage = "";
            if (m_ARPlaneManager.enabled)
            {
                planeDetectionMessage = "Disable Plane Detection and Hide Existing";
                SetAllPlanesActive(true);
            }
            else
            {
                planeDetectionMessage = "Enable Plane Detection and Show Existing";
                SetAllPlanesActive(false);
            }

            if (togglePlaneDetectionText != null)
                togglePlaneDetectionText.SetText(planeDetectionMessage);
        }

        void SetAllPlanesActive(bool value)
        {
            foreach (var plane in m_ARPlaneManager.trackables)
                plane.gameObject.SetActive(value);
        }

        void Awake()
        {
            m_ARPlaneManager = GetComponent<ARPlaneManager>();
        }

        ARPlaneManager m_ARPlaneManager;
    }
}