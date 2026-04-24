using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMode : MonoBehaviour
{
    [Header("Flow")]
    [SerializeField] private string themePanelName = "Main";

    [Header("References")]
    [SerializeField] private FoodDatabase foodDatabase;
    [SerializeField] private TrackedImageFoodManager trackedImageFoodManager;

    private RectTransform runtimeRoot;
    private TMP_FontAsset uiFont;

    private void Awake()
    {
        if (foodDatabase == null)
            foodDatabase = FindObjectOfType<FoodDatabase>();

        if (trackedImageFoodManager == null)
            trackedImageFoodManager = FindObjectOfType<TrackedImageFoodManager>();
    }

    private void OnEnable()
    {
        UIController.ShowUI(themePanelName);
        EnsureThemeSelectionUI();
    }

    private void EnsureThemeSelectionUI()
    {
        if (runtimeRoot != null)
            return;

        RectTransform panelRoot = FindPanelRoot();
        if (panelRoot == null)
        {
            Debug.LogError("Theme selection panel could not be found.");
            return;
        }

        uiFont = TMP_Settings.defaultFontAsset;
        if (uiFont == null)
        {
            TMP_Text existingText = FindObjectOfType<TMP_Text>(true);
            if (existingText != null)
                uiFont = existingText.font;
        }

        FoodThemePalette defaultPalette = FoodThemePalettes.Get(FoodTheme.None);

        runtimeRoot = CreateUIObject("ThemeSelectionRuntime", panelRoot).GetComponent<RectTransform>();
        Stretch(runtimeRoot, new Vector2(24f, 24f), new Vector2(-24f, -24f));

        Image overlay = runtimeRoot.gameObject.AddComponent<Image>();
        overlay.color = WithAlpha(defaultPalette.BackgroundTint, 242);

        RectTransform card = CreateUIObject("ThemeCard", runtimeRoot).GetComponent<RectTransform>();
        card.anchorMin = new Vector2(0.5f, 0.5f);
        card.anchorMax = new Vector2(0.5f, 0.5f);
        card.pivot = new Vector2(0.5f, 0.5f);
        card.sizeDelta = new Vector2(620f, 760f);

        Image cardImage = card.gameObject.AddComponent<Image>();
        cardImage.color = defaultPalette.Surface;

        CreateLabel(
            "CuisineTitle",
            card,
            "Choose a Cuisine",
            34,
            FontStyles.Bold,
            defaultPalette.PrimaryText,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -70f),
            new Vector2(520f, 48f));

        CreateLabel(
            "CuisineSubtitle",
            card,
            "Pick a menu theme to decide which dishes the markers reveal.",
            20,
            FontStyles.Normal,
            defaultPalette.SecondaryText,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -122f),
            new Vector2(520f, 60f));

        CreateThemeButton(card, FoodTheme.American, "American", "Burgers, hot dogs, and fries", -215f);
        CreateThemeButton(card, FoodTheme.Italian, "Italian", "Pizza, bread, and cheese", -105f);
        CreateThemeButton(card, FoodTheme.Mexican, "Mexican", "Tacos, chili, and chips", 5f);
        CreateThemeButton(card, FoodTheme.Japanese, "Japanese", "Sushi, maki, and onigiri", 115f);

        CreateLabel(
            "CuisineHint",
            card,
            "Use the Back button in Scan or Food Info to return here and switch cuisines any time.",
            18,
            FontStyles.Italic,
            defaultPalette.SecondaryText,
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 48f),
            new Vector2(520f, 70f));
    }

    private void CreateThemeButton(
        RectTransform parent,
        FoodTheme theme,
        string title,
        string subtitle,
        float anchoredY)
    {
        FoodThemePalette palette = FoodThemePalettes.Get(theme);

        RectTransform buttonRect = CreateUIObject($"{theme}Button", parent).GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, anchoredY);
        buttonRect.sizeDelta = new Vector2(520f, 86f);

        Image buttonImage = buttonRect.gameObject.AddComponent<Image>();
        buttonImage.color = palette.Surface;

        Button button = buttonRect.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = palette.Surface;
        colors.highlightedColor = Lighten(palette.Surface, -6);
        colors.pressedColor = Lighten(palette.Surface, -14);
        colors.selectedColor = palette.Surface;
        button.colors = colors;
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(() => OnThemeSelected(theme));

        RectTransform accentStrip = CreateUIObject($"{theme}Accent", buttonRect).GetComponent<RectTransform>();
        accentStrip.anchorMin = new Vector2(0f, 0f);
        accentStrip.anchorMax = new Vector2(0f, 1f);
        accentStrip.pivot = new Vector2(0f, 0.5f);
        accentStrip.anchoredPosition = Vector2.zero;
        accentStrip.sizeDelta = new Vector2(18f, 0f);

        Image accentImage = accentStrip.gameObject.AddComponent<Image>();
        accentImage.color = palette.Accent;

        TMP_Text titleLabel = CreateLabel(
            $"{theme}Title",
            buttonRect,
            title,
            24,
            FontStyles.Bold,
            palette.PrimaryText,
            new Vector2(0f, 0.5f),
            new Vector2(1f, 0.5f),
            new Vector2(0f, 14f),
            new Vector2(-40f, 30f),
            new Vector2(36f, 0f));
        titleLabel.alignment = TextAlignmentOptions.MidlineLeft;

        TMP_Text subtitleLabel = CreateLabel(
            $"{theme}Subtitle",
            buttonRect,
            subtitle,
            17,
            FontStyles.Normal,
            palette.SecondaryText,
            new Vector2(0f, 0.5f),
            new Vector2(1f, 0.5f),
            new Vector2(0f, -18f),
            new Vector2(-40f, 24f),
            new Vector2(36f, 0f));
        subtitleLabel.alignment = TextAlignmentOptions.MidlineLeft;

        RectTransform themeBadge = CreateUIObject($"{theme}Badge", buttonRect).GetComponent<RectTransform>();
        themeBadge.anchorMin = new Vector2(1f, 0.5f);
        themeBadge.anchorMax = new Vector2(1f, 0.5f);
        themeBadge.pivot = new Vector2(1f, 0.5f);
        themeBadge.anchoredPosition = new Vector2(-18f, 0f);
        themeBadge.sizeDelta = new Vector2(124f, 42f);

        Image themeBadgeImage = themeBadge.gameObject.AddComponent<Image>();
        themeBadgeImage.color = palette.Accent;

        TMP_Text badgeText = CreateLabel(
            $"{theme}BadgeText",
            themeBadge,
            "Select",
            16,
            FontStyles.Bold,
            Color.white,
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            Vector2.zero);
        badgeText.alignment = TextAlignmentOptions.Center;
    }

    private void OnThemeSelected(FoodTheme theme)
    {
        if (foodDatabase == null || trackedImageFoodManager == null)
        {
            Debug.LogError("Theme selection is missing a FoodDatabase or TrackedImageFoodManager reference.");
            return;
        }

        foodDatabase.SetActiveTheme(theme);
        trackedImageFoodManager.BeginScanning(theme);
    }

    private RectTransform FindPanelRoot()
    {
        GameObject panelObject = GameObject.Find("Main UI");
        return panelObject != null ? panelObject.GetComponent<RectTransform>() : null;
    }

    private TMP_Text CreateLabel(
        string objectName,
        RectTransform parent,
        string text,
        float fontSize,
        FontStyles fontStyle,
        Color color,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        return CreateLabel(objectName, parent, text, fontSize, fontStyle, color, anchorMin, anchorMax, anchoredPosition, sizeDelta, Vector2.zero);
    }

    private TMP_Text CreateLabel(
        string objectName,
        RectTransform parent,
        string text,
        float fontSize,
        FontStyles fontStyle,
        Color color,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Vector2 offsetMin)
    {
        RectTransform rect = CreateUIObject(objectName, parent).GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        rect.offsetMin += offsetMin;

        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = uiFont;
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = true;

        return label;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static void Stretch(RectTransform rectTransform, Vector2 offsetMin, Vector2 offsetMax)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;
    }

    private static Color32 Lighten(Color32 color, int delta)
    {
        int r = Mathf.Clamp(color.r + delta, 0, 255);
        int g = Mathf.Clamp(color.g + delta, 0, 255);
        int b = Mathf.Clamp(color.b + delta, 0, 255);
        return new Color32((byte)r, (byte)g, (byte)b, color.a);
    }

    private static Color32 WithAlpha(Color32 color, byte alpha)
    {
        return new Color32(color.r, color.g, color.b, alpha);
    }
}
