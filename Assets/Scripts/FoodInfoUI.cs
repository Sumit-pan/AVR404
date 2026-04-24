using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class FoodInfoUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button viewDetailsButton;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TMP_Text foodNameText;
    [SerializeField] private TMP_Text foodDescriptionText;
    [SerializeField] private TMP_Text foodMacrosText;
    [SerializeField] private Button closeButton;

    private FoodItem currentFoodItem;
    private FoodTheme currentTheme = FoodTheme.None;
    private FoodThemePalette currentPalette = FoodThemePalettes.Get(FoodTheme.None);
    private RectTransform themeBackgroundRoot;
    private Image themeWashImage;
    private Image themeAccentBarImage;
    private RectTransform themeForegroundRoot;
    private Image themeBadgeImage;
    private TMP_Text themeBadgeText;

    private void Awake()
    {
        AutoAssignReferences();
        EnsureThemeChrome();

        if (viewDetailsButton != null)
            viewDetailsButton.onClick.AddListener(ShowInfoPanel);

        if (closeButton != null)
            closeButton.onClick.AddListener(HideInfoPanel);
    }

    private void Start()
    {
        ApplyTheme(FoodTheme.None);
        SetDetailsButtonVisible(false);
    }

    public void ApplyTheme(FoodTheme theme)
    {
        currentTheme = theme;
        currentPalette = FoodThemePalettes.Get(theme);

        EnsureThemeChrome();
        ApplyThemeVisuals();

        if (currentFoodItem != null)
            UpdateFoodTexts(currentFoodItem);
    }

    public void SetFood(FoodItem item)
    {
        currentFoodItem = item;

        if (item == null)
        {
            ClearFoodTexts();
            SetDetailsButtonVisible(false);
            return;
        }

        if (item.theme != FoodTheme.None)
            ApplyTheme(item.theme);

        UpdateFoodTexts(item);

        if (infoPanel != null)
            infoPanel.SetActive(true);

        SetDetailsButtonVisible(true);
    }

    public void ShowInfoPanel()
    {
        if (currentFoodItem == null)
            return;

        if (infoPanel != null)
            infoPanel.SetActive(true);
    }

    public void HideInfoPanel()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    public void SetDetailsButtonVisible(bool isVisible)
    {
        if (viewDetailsButton != null)
        {
            viewDetailsButton.gameObject.SetActive(isVisible);
            viewDetailsButton.interactable = isVisible;
        }
    }

    private void UpdateFoodTexts(FoodItem item)
    {
        if (foodNameText != null)
        {
            foodNameText.text = item.itemName;
            foodNameText.color = currentPalette.PrimaryText;
        }

        if (foodDescriptionText != null)
        {
            foodDescriptionText.text = FormatDescription(item);
            foodDescriptionText.color = currentPalette.SecondaryText;
        }

        if (foodMacrosText != null)
        {
            foodMacrosText.text = FormatNutrition(item);
            foodMacrosText.color = currentPalette.PrimaryText;
        }
    }

    private void ClearFoodTexts()
    {
        if (foodNameText != null)
            foodNameText.text = "";

        if (foodDescriptionText != null)
            foodDescriptionText.text = "";

        if (foodMacrosText != null)
            foodMacrosText.text = "";
    }

    private static string FormatDescription(FoodItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.description))
            return item.description.Trim();

        return "A featured menu item ready to explore in AR.";
    }

    private string FormatNutrition(FoodItem item)
    {
        string accentColor = ToHex(currentPalette.Accent);
        string bodyColor = ToHex(currentPalette.PrimaryText);

        return
            $"<size=90%><b><color={accentColor}>{currentPalette.DisplayName} nutrition</color></b></size>\n" +
            $"<b><color={bodyColor}>Calories</color></b>  <color={bodyColor}>{item.calories} kcal</color>\n" +
            $"<b><color={bodyColor}>Protein</color></b>  <color={bodyColor}>{item.protein:0.#} g</color>\n" +
            $"<b><color={bodyColor}>Carbs</color></b>  <color={bodyColor}>{item.carbs:0.#} g</color>\n" +
            $"<b><color={bodyColor}>Fat</color></b>  <color={bodyColor}>{item.fat:0.#} g</color>";
    }

    private void AutoAssignReferences()
    {
        if (infoPanel == null)
        {
            Transform panelTransform = transform.Cast<Transform>()
                .FirstOrDefault(child => child.name.Contains("Panel"));

            if (panelTransform != null)
                infoPanel = panelTransform.gameObject;
        }

        Button[] buttons = GetComponentsInChildren<Button>(true);
        if (viewDetailsButton == null && buttons.Length > 0)
            viewDetailsButton = buttons[0];

        if (closeButton == null && buttons.Length > 1)
            closeButton = buttons[1];

        TMP_Text[] nonButtonTexts = GetComponentsInChildren<TMP_Text>(true)
            .Where(text => text.GetComponentInParent<Button>() == null)
            .ToArray();

        if (foodNameText == null && nonButtonTexts.Length > 0)
            foodNameText = nonButtonTexts[0];

        if (foodDescriptionText == null && nonButtonTexts.Length > 1)
            foodDescriptionText = nonButtonTexts[1];

        if (foodMacrosText == null && nonButtonTexts.Length > 2)
            foodMacrosText = nonButtonTexts[2];

        if (infoPanel == null)
        {
            Debug.LogWarning($"{nameof(FoodInfoUI)} on {name} could not find an info panel automatically. Assign one in the inspector.");
        }
    }

    private void EnsureThemeChrome()
    {
        Transform chromeParent = infoPanel != null ? infoPanel.transform : transform;
        if (chromeParent == null)
            return;

        if (themeBackgroundRoot == null)
        {
            themeBackgroundRoot = CreateUIObject("ThemeBackgroundChrome", chromeParent).GetComponent<RectTransform>();
            Stretch(themeBackgroundRoot);
            themeBackgroundRoot.SetAsFirstSibling();

            themeWashImage = themeBackgroundRoot.gameObject.AddComponent<Image>();
            themeWashImage.raycastTarget = false;

            RectTransform accentBar = CreateUIObject("ThemeAccentBar", themeBackgroundRoot).GetComponent<RectTransform>();
            accentBar.anchorMin = new Vector2(0f, 1f);
            accentBar.anchorMax = new Vector2(1f, 1f);
            accentBar.pivot = new Vector2(0.5f, 1f);
            accentBar.anchoredPosition = Vector2.zero;
            accentBar.sizeDelta = new Vector2(0f, 12f);

            themeAccentBarImage = accentBar.gameObject.AddComponent<Image>();
            themeAccentBarImage.raycastTarget = false;
        }

        if (themeForegroundRoot == null)
        {
            themeForegroundRoot = CreateUIObject("ThemeForegroundChrome", chromeParent).GetComponent<RectTransform>();
            Stretch(themeForegroundRoot);
            themeForegroundRoot.SetAsLastSibling();

            RectTransform badgeRect = CreateUIObject("ThemeBadge", themeForegroundRoot).GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(1f, 1f);
            badgeRect.anchorMax = new Vector2(1f, 1f);
            badgeRect.pivot = new Vector2(1f, 1f);
            badgeRect.anchoredPosition = new Vector2(-18f, -18f);
            badgeRect.sizeDelta = new Vector2(210f, 44f);

            themeBadgeImage = badgeRect.gameObject.AddComponent<Image>();
            themeBadgeImage.raycastTarget = false;

            RectTransform badgeLabelRect = CreateUIObject("Label", badgeRect).GetComponent<RectTransform>();
            Stretch(badgeLabelRect);

            TextMeshProUGUI badgeLabel = badgeLabelRect.gameObject.AddComponent<TextMeshProUGUI>();
            badgeLabel.font = TMP_Settings.defaultFontAsset;
            badgeLabel.fontSize = 18f;
            badgeLabel.fontStyle = FontStyles.Bold;
            badgeLabel.alignment = TextAlignmentOptions.Center;
            badgeLabel.raycastTarget = false;
            themeBadgeText = badgeLabel;
        }
    }

    private void ApplyThemeVisuals()
    {
        bool hasTheme = currentTheme != FoodTheme.None;

        if (themeBackgroundRoot != null)
            themeBackgroundRoot.gameObject.SetActive(hasTheme);

        if (themeForegroundRoot != null)
            themeForegroundRoot.gameObject.SetActive(hasTheme);

        if (themeWashImage != null)
            themeWashImage.color = WithAlpha(currentPalette.BackgroundTint, 78);

        if (themeAccentBarImage != null)
            themeAccentBarImage.color = currentPalette.Accent;

        if (themeBadgeImage != null)
            themeBadgeImage.color = currentPalette.Accent;

        if (themeBadgeText != null)
        {
            themeBadgeText.text = hasTheme ? $"{currentPalette.DisplayName} Menu" : string.Empty;
            themeBadgeText.color = Color.white;
        }

        if (infoPanel != null && infoPanel.TryGetComponent(out Image infoPanelImage))
            infoPanelImage.color = hasTheme ? currentPalette.Surface : Color.white;

        ApplyButtonTheme(viewDetailsButton, currentPalette.Accent, Color.white, hasTheme);
        ApplyButtonTheme(closeButton, currentPalette.Accent, Color.white, hasTheme);
    }

    private static void ApplyButtonTheme(Button button, Color32 fillColor, Color textColor, bool useTheme)
    {
        if (button == null)
            return;

        Graphic targetGraphic = button.targetGraphic;
        if (targetGraphic != null)
        {
            if (useTheme)
                targetGraphic.color = fillColor;
        }

        ColorBlock colors = button.colors;
        colors.normalColor = useTheme ? fillColor : colors.normalColor;
        colors.highlightedColor = useTheme ? Lighten(fillColor, 18) : colors.highlightedColor;
        colors.pressedColor = useTheme ? Lighten(fillColor, -20) : colors.pressedColor;
        colors.selectedColor = useTheme ? fillColor : colors.selectedColor;
        button.colors = colors;

        foreach (Graphic graphic in button.GetComponentsInChildren<Graphic>(true))
        {
            if (graphic.gameObject == button.gameObject)
                continue;

            if (graphic is TMP_Text tmpText)
                tmpText.color = textColor;
            else
                graphic.color = textColor;
        }
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private static string ToHex(Color32 color)
    {
        return $"#{color.r:X2}{color.g:X2}{color.b:X2}";
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
