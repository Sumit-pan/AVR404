using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TrackedImageFoodManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private FoodDatabase foodDatabase;
    [SerializeField] private FoodInfoUI foodInfoUI;

    [Header("UI")]
    [SerializeField] private GameObject scanUI;
    [SerializeField] private GameObject mainUI;
    [SerializeField] private string scanPanelName = "Scan";
    [SerializeField] private string trackedFoodPanelName = "FoodInfo";
    [SerializeField] private string themePanelName = "Main";

    [Header("Modes")]
    [SerializeField] private string scanModeName = "Scan";
    [SerializeField] private string trackedFoodModeName = "FoodInfo";
    [SerializeField] private string themeModeName = "Main";

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.1f, 0f);

    [Header("Display Animation")]
    [SerializeField] private bool rotateSpawnedFood = true;
    [SerializeField] private float rotationSpeed = 45f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    private Dictionary<string, GameObject> spawnedFoods = new Dictionary<string, GameObject>();
    private string currentMarkerName = "";
    private bool scanningEnabled;
    private FoodTheme currentTheme = FoodTheme.None;
    private Button scanBackButton;
    private Button foodInfoBackButton;
    private RectTransform scanThemeChromeRoot;
    private Image scanThemeWashImage;
    private Image scanThemeAccentImage;
    private Image scanThemeBadgeImage;
    private TMP_Text scanThemeBadgeText;
    private Image scanThemeHintImage;
    private TMP_Text scanThemeHintText;

    private void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void Start()
    {
        EnsureThemeBackButtons();
        EnsureScanThemeChrome();
        ApplyThemeChrome(FoodTheme.None);
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        if (!scanningEnabled)
            return;

        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            UpdateTrackedImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateTrackedImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            string markerName = trackedImage.referenceImage.name;

            if (spawnedFoods.ContainsKey(markerName))
            {
                Destroy(spawnedFoods[markerName]);
                spawnedFoods.Remove(markerName);
            }

            if (currentMarkerName == markerName)
            {
                ResetTrackingUIIfNothingTracked();
            }
        }
    }

    private void UpdateTrackedImage(ARTrackedImage trackedImage)
    {
        if (!scanningEnabled)
            return;

        string markerName = trackedImage.referenceImage.name;

        if (trackedImage.trackingState != TrackingState.Tracking)
        {
            if (spawnedFoods.TryGetValue(markerName, out GameObject spawnedFood) && spawnedFood != null)
                spawnedFood.SetActive(false);

            if (currentMarkerName == markerName)
                ResetTrackingUIIfNothingTracked();

            return;
        }

        FoodItem item = foodDatabase.GetFoodByMarkerName(markerName);

        if (item == null)
            return;

        currentMarkerName = markerName;

        if (!spawnedFoods.ContainsKey(markerName))
        {
            Vector3 spawnPosition = trackedImage.transform.position + spawnOffset;
            Quaternion spawnRotation = trackedImage.transform.rotation;

            GameObject spawnedFood = Instantiate(item.foodPrefab, spawnPosition, spawnRotation);
            spawnedFood.transform.SetParent(trackedImage.transform);
            ConfigureSpawnedFood(spawnedFood);

            spawnedFoods.Add(markerName, spawnedFood);
        }
        else if (spawnedFoods[markerName] != null)
        {
            spawnedFoods[markerName].SetActive(true);
        }

        if (foodInfoUI != null)
            foodInfoUI.SetFood(item);

        UpdateInteractiveFood();

        if (InteractionController.IsInitialized)
            InteractionController.EnableMode(trackedFoodModeName);

        if (UIController.IsInitialized)
        {
            UIController.ShowUI(trackedFoodPanelName);
        }
        else
        {
            if (scanUI != null)
                scanUI.SetActive(false);

            if (mainUI != null)
                mainUI.SetActive(true);
        }
    }

    public void BeginScanning(FoodTheme theme)
    {
        if (foodDatabase == null)
        {
            Debug.LogError("Cannot begin scanning without a FoodDatabase.");
            return;
        }

        currentTheme = theme;
        foodDatabase.SetActiveTheme(theme);
        scanningEnabled = true;
        ApplyThemeChrome(theme);

        if (foodInfoUI != null)
            foodInfoUI.ApplyTheme(theme);

        ClearSpawnedFoods();
        ResetTrackingUI();
        RefreshTrackedImagesImmediately();
    }

    public void ResetTrackingUI()
    {
        currentMarkerName = "";

        if (InteractionController.IsInitialized)
            InteractionController.EnableMode(scanModeName);

        if (UIController.IsInitialized)
        {
            UIController.ShowUI(scanPanelName);
        }
        else
        {
            if (scanUI != null)
                scanUI.SetActive(true);

            if (mainUI != null)
                mainUI.SetActive(false);
        }

        if (foodInfoUI != null)
            foodInfoUI.SetFood(null);

        UpdateInteractiveFood();
    }

    public void ReturnToThemeSelection()
    {
        scanningEnabled = false;
        currentTheme = FoodTheme.None;

        if (foodDatabase != null)
            foodDatabase.SetActiveTheme(FoodTheme.None);

        ClearSpawnedFoods();
        currentMarkerName = "";

        if (foodInfoUI != null)
        {
            foodInfoUI.ApplyTheme(FoodTheme.None);
            foodInfoUI.SetFood(null);
        }

        ApplyThemeChrome(FoodTheme.None);

        if (InteractionController.IsInitialized)
            InteractionController.EnableMode(themeModeName);

        if (UIController.IsInitialized)
        {
            UIController.ShowUI(themePanelName);
        }
        else
        {
            if (scanUI != null)
                scanUI.SetActive(false);

            if (mainUI != null)
                mainUI.SetActive(false);
        }
    }

    private void ConfigureSpawnedFood(GameObject spawnedFood)
    {
        if (!rotateSpawnedFood || spawnedFood == null)
            return;

        FoodDisplayRotator rotator = spawnedFood.GetComponent<FoodDisplayRotator>();
        if (rotator == null)
            rotator = spawnedFood.AddComponent<FoodDisplayRotator>();

        rotator.Configure(rotationAxis, rotationSpeed);
    }

    private void UpdateInteractiveFood()
    {
        foreach (KeyValuePair<string, GameObject> entry in spawnedFoods)
        {
            if (entry.Value == null)
                continue;

            FoodDisplayRotator rotator = entry.Value.GetComponent<FoodDisplayRotator>();
            if (rotator == null)
                continue;

            bool isCurrentFood = !string.IsNullOrEmpty(currentMarkerName) &&
                entry.Key == currentMarkerName &&
                entry.Value.activeInHierarchy;

            rotator.SetInteractionEnabled(isCurrentFood);
        }
    }

    private void ClearSpawnedFoods()
    {
        foreach (KeyValuePair<string, GameObject> entry in spawnedFoods)
        {
            if (entry.Value != null)
                Destroy(entry.Value);
        }

        spawnedFoods.Clear();
    }

    private void ResetTrackingUIIfNothingTracked()
    {
        if (HasAnyActiveTrackedImage())
            return;

        ResetTrackingUI();
    }

    private bool HasAnyActiveTrackedImage()
    {
        if (trackedImageManager == null)
            return false;

        foreach (ARTrackedImage trackedImage in trackedImageManager.trackables)
        {
            if (trackedImage != null && trackedImage.trackingState == TrackingState.Tracking)
                return true;
        }

        return false;
    }

    private void RefreshTrackedImagesImmediately()
    {
        if (trackedImageManager == null)
            return;

        foreach (ARTrackedImage trackedImage in trackedImageManager.trackables)
        {
            if (trackedImage != null && trackedImage.trackingState == TrackingState.Tracking)
                UpdateTrackedImage(trackedImage);
        }
    }

    private void EnsureThemeBackButtons()
    {
        if (scanUI != null && scanBackButton == null)
            scanBackButton = CreateThemeBackButton(scanUI.transform, "ScanBackToThemes", new Vector2(-18f, -18f));

        if (mainUI != null && foodInfoBackButton == null)
            foodInfoBackButton = CreateThemeBackButton(mainUI.transform, "FoodInfoBackToThemes", new Vector2(-18f, -18f));
    }

    private void EnsureScanThemeChrome()
    {
        if (scanUI == null || scanThemeChromeRoot != null)
            return;

        scanThemeChromeRoot = CreateUIObject("ScanThemeChrome", scanUI.transform).GetComponent<RectTransform>();
        Stretch(scanThemeChromeRoot);
        scanThemeChromeRoot.SetAsLastSibling();

        scanThemeWashImage = scanThemeChromeRoot.gameObject.AddComponent<Image>();
        scanThemeWashImage.raycastTarget = false;

        RectTransform accentRect = CreateUIObject("ThemeAccentBar", scanThemeChromeRoot).GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 1f);
        accentRect.anchorMax = new Vector2(1f, 1f);
        accentRect.pivot = new Vector2(0.5f, 1f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(0f, 16f);

        scanThemeAccentImage = accentRect.gameObject.AddComponent<Image>();
        scanThemeAccentImage.raycastTarget = false;

        RectTransform badgeRect = CreateUIObject("ThemeBadge", scanThemeChromeRoot).GetComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(0f, 1f);
        badgeRect.anchorMax = new Vector2(0f, 1f);
        badgeRect.pivot = new Vector2(0f, 1f);
        badgeRect.anchoredPosition = new Vector2(18f, -30f);
        badgeRect.sizeDelta = new Vector2(220f, 48f);

        scanThemeBadgeImage = badgeRect.gameObject.AddComponent<Image>();
        scanThemeBadgeImage.raycastTarget = false;

        RectTransform badgeLabelRect = CreateUIObject("Label", badgeRect).GetComponent<RectTransform>();
        Stretch(badgeLabelRect);

        TextMeshProUGUI badgeLabel = badgeLabelRect.gameObject.AddComponent<TextMeshProUGUI>();
        badgeLabel.font = TMP_Settings.defaultFontAsset;
        badgeLabel.fontSize = 21f;
        badgeLabel.fontStyle = FontStyles.Bold;
        badgeLabel.color = Color.white;
        badgeLabel.alignment = TextAlignmentOptions.Center;
        badgeLabel.raycastTarget = false;
        scanThemeBadgeText = badgeLabel;

        RectTransform hintRect = CreateUIObject("ThemeHint", scanThemeChromeRoot).GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.5f, 1f);
        hintRect.anchorMax = new Vector2(0.5f, 1f);
        hintRect.pivot = new Vector2(0.5f, 1f);
        hintRect.anchoredPosition = new Vector2(0f, -34f);
        hintRect.sizeDelta = new Vector2(460f, 74f);

        scanThemeHintImage = hintRect.gameObject.AddComponent<Image>();
        scanThemeHintImage.raycastTarget = false;

        RectTransform hintLabelRect = CreateUIObject("Label", hintRect).GetComponent<RectTransform>();
        Stretch(hintLabelRect, new Vector2(18f, 10f), new Vector2(-18f, -10f));

        TextMeshProUGUI hintLabel = hintLabelRect.gameObject.AddComponent<TextMeshProUGUI>();
        hintLabel.font = TMP_Settings.defaultFontAsset;
        hintLabel.fontSize = 20f;
        hintLabel.enableWordWrapping = true;
        hintLabel.alignment = TextAlignmentOptions.Center;
        hintLabel.raycastTarget = false;
        scanThemeHintText = hintLabel;
    }

    private void ApplyThemeChrome(FoodTheme theme)
    {
        FoodThemePalette palette = FoodThemePalettes.Get(theme);
        bool hasTheme = theme != FoodTheme.None;

        EnsureScanThemeChrome();

        if (scanThemeChromeRoot != null)
            scanThemeChromeRoot.gameObject.SetActive(hasTheme);

        if (scanThemeWashImage != null)
            scanThemeWashImage.color = WithAlpha(palette.BackgroundTint, 58);

        if (scanThemeAccentImage != null)
            scanThemeAccentImage.color = palette.Accent;

        if (scanThemeBadgeImage != null)
            scanThemeBadgeImage.color = palette.Accent;

        if (scanThemeBadgeText != null)
            scanThemeBadgeText.text = hasTheme ? $"{palette.DisplayName} Menu" : string.Empty;

        if (scanThemeHintImage != null)
            scanThemeHintImage.color = hasTheme ? WithAlpha(palette.Surface, 236) : Color.clear;

        if (scanThemeHintText != null)
        {
            scanThemeHintText.text = hasTheme
                ? $"Point your camera at a marker to reveal {palette.DisplayName.ToLowerInvariant()} dishes."
                : string.Empty;
            scanThemeHintText.color = palette.PrimaryText;
        }

        ApplyBackButtonTheme(scanBackButton, palette, hasTheme);
        ApplyBackButtonTheme(foodInfoBackButton, palette, hasTheme);
    }

    private Button CreateThemeBackButton(Transform parent, string objectName, Vector2 anchoredPosition)
    {
        RectTransform buttonRect = CreateUIObject(objectName, parent).GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(1f, 1f);
        buttonRect.anchoredPosition = anchoredPosition;
        buttonRect.sizeDelta = new Vector2(150f, 54f);

        Image buttonImage = buttonRect.gameObject.AddComponent<Image>();
        buttonImage.color = new Color32(58, 44, 35, 220);

        Button button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(ReturnToThemeSelection);

        RectTransform labelRect = CreateUIObject("Label", buttonRect).GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = "Themes";
        label.fontSize = 22f;
        label.fontStyle = FontStyles.Bold;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;

        return button;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static void ApplyBackButtonTheme(Button button, FoodThemePalette palette, bool useTheme)
    {
        if (button == null)
            return;

        Color32 fillColor = useTheme ? palette.Accent : new Color32(58, 44, 35, 220);
        if (button.targetGraphic != null)
            button.targetGraphic.color = fillColor;

        ColorBlock colors = button.colors;
        colors.normalColor = fillColor;
        colors.highlightedColor = Lighten(fillColor, 18);
        colors.pressedColor = Lighten(fillColor, -20);
        colors.selectedColor = fillColor;
        button.colors = colors;

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
            label.color = Color.white;
    }

    private static void Stretch(RectTransform rectTransform)
    {
        Stretch(rectTransform, Vector2.zero, Vector2.zero);
    }

    private static void Stretch(RectTransform rectTransform, Vector2 offsetMin, Vector2 offsetMax)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;
    }

    private static Color32 WithAlpha(Color32 color, byte alpha)
    {
        return new Color32(color.r, color.g, color.b, alpha);
    }

    private static Color32 Lighten(Color32 color, int delta)
    {
        int r = Mathf.Clamp(color.r + delta, 0, 255);
        int g = Mathf.Clamp(color.g + delta, 0, 255);
        int b = Mathf.Clamp(color.b + delta, 0, 255);
        return new Color32((byte)r, (byte)g, (byte)b, color.a);
    }
}
