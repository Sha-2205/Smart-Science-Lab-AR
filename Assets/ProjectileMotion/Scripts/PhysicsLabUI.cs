using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SmartScienceLab.PhysicsLab.ProjectileMotion
{
    public class PhysicsLabUI : MonoBehaviour
    {
        [Header("Instruction / Status")]
        [SerializeField] private TMP_Text statusBannerText;

        [Header("Parameter Control Panel")]
        [SerializeField] private GameObject parameterPanel;
        [SerializeField] private Slider angleSlider;
        [SerializeField] private TMP_Text angleValueText;
        [SerializeField] private Slider velocitySlider;
        [SerializeField] private TMP_Text velocityValueText;
        [SerializeField] private TMP_Text targetDistanceText;
        [SerializeField] private Button launchButton;

        [Header("Results & Evaluation Panel")]
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private TMP_Text resultParamsText;
        [SerializeField] private TMP_Text resultHeightText;
        [SerializeField] private TMP_Text resultRangeText;
        [SerializeField] private TMP_Text resultTimeText;
        [SerializeField] private TMP_Text evaluationTitleText;
        [SerializeField] private TMP_Text evaluationExplanationText;
        [SerializeField] private TMP_Text evaluationHintText;
        [SerializeField] private Button redoButton;

        // Callbacks for manager
        public event Action<float> OnAngleChanged;
        public event Action<float> OnVelocityChanged;
        public event Action OnLaunchPressed;
        public event Action OnRedoPressed;

        private void Awake()
        {
            if (parameterPanel == null || resultsPanel == null || statusBannerText == null)
            {
                BuildRuntimeUIHierarchy();
            }

            if (angleSlider != null)
            {
                angleSlider.minValue = 10f;
                angleSlider.maxValue = 80f;
                angleSlider.value = 45f;
                angleSlider.onValueChanged.AddListener(HandleAngleSliderChanged);
            }

            if (velocitySlider != null)
            {
                velocitySlider.minValue = 2f;
                velocitySlider.maxValue = 20f;
                velocitySlider.value = 10f;
                velocitySlider.onValueChanged.AddListener(HandleVelocityChanged);
            }

            if (launchButton != null)
            {
                launchButton.onClick.AddListener(() => OnLaunchPressed?.Invoke());
            }

            if (redoButton != null)
            {
                redoButton.onClick.AddListener(() => OnRedoPressed?.Invoke());
            }

            UpdateSliderReadouts();
            SetResultsVisible(false);
            SetParameterControlsVisible(false);
        }

        public void SetStatusText(string message)
        {
            if (statusBannerText != null)
            {
                statusBannerText.text = message;
            }
        }

        public void SetTargetDisplay(float targetMeters, float toleranceMeters)
        {
            if (targetDistanceText != null)
            {
                targetDistanceText.text = $"Target Distance: <b>{targetMeters:F1} m</b> (±{toleranceMeters:F1} m)";
            }
        }

        public void SetParameterControlsVisible(bool visible)
        {
            if (parameterPanel != null)
            {
                parameterPanel.SetActive(visible);
            }
        }

        public void SetResultsVisible(bool visible)
        {
            if (resultsPanel != null)
            {
                resultsPanel.SetActive(visible);
            }
        }

        public void SetLaunchButtonInteractable(bool interactable)
        {
            if (launchButton != null)
            {
                launchButton.interactable = interactable;
            }
        }

        public void DisplayResults(
            float angle,
            float velocity,
            float measHeight,
            float theoHeight,
            float measRange,
            float theoRange,
            float measTime,
            float theoTime,
            string evalTitle,
            string evalExplanation,
            string evalHint)
        {
            if (resultParamsText != null)
            {
                resultParamsText.text = $"Launch Angle: <b>{angle:F0}°</b>   |   Initial Velocity: <b>{velocity:F1} m/s</b>";
            }

            if (resultHeightText != null)
            {
                resultHeightText.text = $"Peak Height (H): <b>{measHeight:F2} m</b>  <color=#88AACC>(Theo: {theoHeight:F2} m)</color>";
            }

            if (resultRangeText != null)
            {
                resultRangeText.text = $"Total Range (R): <b>{measRange:F2} m</b>  <color=#88AACC>(Theo: {theoRange:F2} m)</color>";
            }

            if (resultTimeText != null)
            {
                resultTimeText.text = $"Flight Time (T): <b>{measTime:F2} s</b>  <color=#88AACC>(Theo: {theoTime:F2} s)</color>";
            }

            if (evaluationTitleText != null)
            {
                evaluationTitleText.text = evalTitle;
            }

            if (evaluationExplanationText != null)
            {
                evaluationExplanationText.text = evalExplanation;
            }

            if (evaluationHintText != null)
            {
                evaluationHintText.text = $"<b>Educational Hint:</b> {evalHint}";
            }

            SetResultsVisible(true);
            SetParameterControlsVisible(false);
        }

        private void HandleAngleSliderChanged(float val)
        {
            if (angleValueText != null)
            {
                angleValueText.text = $"Launch Angle: <b>{val:F0}°</b>";
            }
            OnAngleChanged?.Invoke(val);
        }

        private void HandleVelocityChanged(float val)
        {
            if (velocityValueText != null)
            {
                velocityValueText.text = $"Initial Velocity: <b>{val:F1} m/s</b>";
            }
            OnVelocityChanged?.Invoke(val);
        }

        private void UpdateSliderReadouts()
        {
            if (angleSlider != null && angleValueText != null)
            {
                angleValueText.text = $"Launch Angle: <b>{angleSlider.value:F0}°</b>";
            }
            if (velocitySlider != null && velocityValueText != null)
            {
                velocityValueText.text = $"Initial Velocity: <b>{velocitySlider.value:F1} m/s</b>";
            }
        }

        public float GetCurrentAngle() => angleSlider != null ? angleSlider.value : 45f;
        public float GetCurrentVelocity() => velocitySlider != null ? velocitySlider.value : 10f;

        #region Programmatic UI Construction Fallback

        private void BuildRuntimeUIHierarchy()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
            }
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            // 1. Header Bar
            GameObject headerObj = CreatePanel(transform, "HeaderBar", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, 0), new Vector2(0, 130), new Color(0.06f, 0.09f, 0.14f, 0.95f));

            CreateText(headerObj.transform, "LabTitle", "SMART SCIENCE LAB", 32, FontStyles.Bold,
                new Color(0.25f, 0.85f, 1f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, -18), new Vector2(-40, 40), TextAlignmentOptions.Center);

            CreateText(headerObj.transform, "ExperimentTitle", "PHYSICS LAB — PROJECTILE MOTION", 22, FontStyles.Normal,
                new Color(0.85f, 0.92f, 1f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, -68), new Vector2(-40, 35), TextAlignmentOptions.Center);

            // 2. Status Banner
            GameObject statusObj = CreatePanel(transform, "StatusBanner", new Vector2(0.04f, 1), new Vector2(0.96f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -150), new Vector2(0, 80), new Color(0.10f, 0.15f, 0.22f, 0.90f));

            statusBannerText = CreateText(statusObj.transform, "StatusText", "Scan room to detect a horizontal surface.", 24, FontStyles.Normal,
                new Color(1f, 0.85f, 0.25f), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(-30, 0), TextAlignmentOptions.Center);

            // 3. Parameter Controls Panel (Bottom Card)
            parameterPanel = CreatePanel(transform, "ParameterPanel", new Vector2(0.04f, 0), new Vector2(0.96f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 30), new Vector2(0, 480), new Color(0.07f, 0.11f, 0.17f, 0.95f));

            targetDistanceText = CreateText(parameterPanel.transform, "TargetText", "Target Distance: <b>5.0 m</b> (±0.4 m)", 26, FontStyles.Normal,
                new Color(0.3f, 0.9f, 0.7f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, -25), new Vector2(-40, 35), TextAlignmentOptions.Center);

            // Angle Section
            angleValueText = CreateText(parameterPanel.transform, "AngleText", "Launch Angle: <b>45°</b>", 25, FontStyles.Normal,
                Color.white, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, -75), new Vector2(-40, 35), TextAlignmentOptions.Left);

            angleSlider = CreateSimpleSlider(parameterPanel.transform, "AngleSlider", new Vector2(0, -125), 10f, 80f, 45f);

            // Velocity Section
            velocityValueText = CreateText(parameterPanel.transform, "VelocityText", "Initial Velocity: <b>10.0 m/s</b>", 25, FontStyles.Normal,
                Color.white, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, -190), new Vector2(-40, 35), TextAlignmentOptions.Left);

            velocitySlider = CreateSimpleSlider(parameterPanel.transform, "VelocitySlider", new Vector2(0, -240), 2f, 20f, 10f);

            // Launch Button
            launchButton = CreateButton(parameterPanel.transform, "LaunchButton", "LAUNCH PROJECTILE",
                new Vector2(0, -370), new Vector2(-50, 85), new Color(0.12f, 0.58f, 0.95f), Color.white, 30);

            // 4. Results & Educational Feedback Panel (Bottom Card)
            resultsPanel = CreatePanel(transform, "ResultsPanel", new Vector2(0.03f, 0), new Vector2(0.97f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 25), new Vector2(0, 720), new Color(0.06f, 0.09f, 0.15f, 0.97f));

            CreateText(resultsPanel.transform, "ResultsHeader", "EXPERIMENT MEASUREMENTS", 27, FontStyles.Bold,
                new Color(0.3f, 0.85f, 1f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, -20), new Vector2(-40, 35), TextAlignmentOptions.Center);

            resultParamsText = CreateText(resultsPanel.transform, "ResultParams", "Launch Angle: 45°   |   Initial Velocity: 10.0 m/s", 21, FontStyles.Normal,
                new Color(0.8f, 0.9f, 1f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, -65), new Vector2(-40, 30), TextAlignmentOptions.Center);

            resultHeightText = CreateText(resultsPanel.transform, "ResultHeight", "Peak Height (H): 2.55 m", 21, FontStyles.Normal,
                Color.white, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, -105), new Vector2(-50, 30), TextAlignmentOptions.Left);

            resultRangeText = CreateText(resultsPanel.transform, "ResultRange", "Total Range (R): 10.19 m", 21, FontStyles.Normal,
                Color.white, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, -145), new Vector2(-50, 30), TextAlignmentOptions.Left);

            resultTimeText = CreateText(resultsPanel.transform, "ResultTime", "Flight Time (T): 1.44 s", 21, FontStyles.Normal,
                Color.white, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, -185), new Vector2(-50, 30), TextAlignmentOptions.Left);

            // Divider Line
            CreatePanel(resultsPanel.transform, "Divider", new Vector2(0.04f, 1), new Vector2(0.96f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -230), new Vector2(0, 3), new Color(0.2f, 0.3f, 0.45f, 0.8f));

            // Educational Evaluation Section
            evaluationTitleText = CreateText(resultsPanel.transform, "EvalTitle", "Target Evaluation", 26, FontStyles.Bold,
                new Color(0.2f, 0.9f, 0.5f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, -255), new Vector2(-40, 35), TextAlignmentOptions.Center);

            evaluationExplanationText = CreateText(resultsPanel.transform, "EvalExplanation", "Explanation of flight mechanics and component velocities.", 20, FontStyles.Normal,
                new Color(0.9f, 0.95f, 1f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, -300), new Vector2(-50, 130), TextAlignmentOptions.TopLeft);

            evaluationHintText = CreateText(resultsPanel.transform, "EvalHint", "Educational hint on parameter correction.", 20, FontStyles.Normal,
                new Color(1f, 0.88f, 0.35f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, -445), new Vector2(-50, 110), TextAlignmentOptions.TopLeft);

            // REDO Button
            redoButton = CreateButton(resultsPanel.transform, "RedoButton", "REDO EXPERIMENT",
                new Vector2(0, -620), new Vector2(-50, 85), new Color(0.94f, 0.46f, 0.16f), Color.white, 30);
        }

        private GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 sizeDelta, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Image img = obj.GetComponent<Image>();
            img.color = color;

            return obj;
        }

        private TMP_Text CreateText(Transform parent, string name, string text, float fontSize, FontStyles style,
            Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 sizeDelta, TextAlignmentOptions alignment)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;

            return tmp;
        }

        private Slider CreateSimpleSlider(Transform parent, string name, Vector2 anchoredPos, float min, float max, float val)
        {
            GameObject sliderObj = new GameObject(name, typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(parent, false);

            RectTransform rt = sliderObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 1);
            rt.anchorMax = new Vector2(0.95f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(0, 45);

            Slider slider = sliderObj.GetComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = val;

            // Background track
            GameObject bg = CreatePanel(sliderObj.transform, "Background", new Vector2(0, 0.3f), new Vector2(1, 0.7f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, new Color(0.2f, 0.28f, 0.38f, 1f));

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRt = fillArea.GetComponent<RectTransform>();
            fillAreaRt.anchorMin = new Vector2(0, 0.3f);
            fillAreaRt.anchorMax = new Vector2(1, 0.7f);
            fillAreaRt.sizeDelta = Vector2.zero;

            GameObject fill = CreatePanel(fillArea.transform, "Fill", new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, new Color(0.2f, 0.75f, 1f, 1f));

            // Handle Slide Area
            GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderObj.transform, false);
            RectTransform handleAreaRt = handleArea.GetComponent<RectTransform>();
            handleAreaRt.anchorMin = new Vector2(0, 0);
            handleAreaRt.anchorMax = new Vector2(1, 1);
            handleAreaRt.sizeDelta = Vector2.zero;

            GameObject handle = CreatePanel(handleArea.transform, "Handle", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(50, 50), Color.white);

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.direction = Slider.Direction.LeftToRight;

            return slider;
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 sizeDelta,
            Color btnColor, Color textColor, float fontSize)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.04f, 1);
            rt.anchorMax = new Vector2(0.96f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Image img = btnObj.GetComponent<Image>();
            img.color = btnColor;

            Button btn = btnObj.GetComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.highlightedColor = btnColor * 1.15f;
            colors.pressedColor = btnColor * 0.85f;
            btn.colors = colors;

            CreateText(btnObj.transform, "Label", label, fontSize, FontStyles.Bold, textColor,
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);

            return btn;
        }

        #endregion
    }
}
