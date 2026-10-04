using UnityEngine;
using TMPro;

namespace SmartScienceLab.PhysicsLab.ProjectileMotion
{
    public class BillboardDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text displayText;
        private Camera targetCamera;

        private void Awake()
        {
            if (displayText == null)
            {
                displayText = GetComponentInChildren<TMP_Text>();
            }
        }

        private void Start()
        {
            targetCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null) return;
            }

            // Face the camera so text is readable from any viewing angle
            transform.rotation = Quaternion.LookRotation(transform.position - targetCamera.transform.position);
        }

        public void SetText(string text)
        {
            if (displayText == null)
            {
                displayText = GetComponentInChildren<TMP_Text>();
            }

            if (displayText != null)
            {
                displayText.text = text;
            }
        }
    }
}
