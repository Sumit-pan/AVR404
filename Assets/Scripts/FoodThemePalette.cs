using UnityEngine;

public readonly struct FoodThemePalette
{
    public readonly string DisplayName;
    public readonly Color32 Accent;
    public readonly Color32 Surface;
    public readonly Color32 BackgroundTint;
    public readonly Color32 PrimaryText;
    public readonly Color32 SecondaryText;

    public FoodThemePalette(
        string displayName,
        Color32 accent,
        Color32 surface,
        Color32 backgroundTint,
        Color32 primaryText,
        Color32 secondaryText)
    {
        DisplayName = displayName;
        Accent = accent;
        Surface = surface;
        BackgroundTint = backgroundTint;
        PrimaryText = primaryText;
        SecondaryText = secondaryText;
    }
}

public static class FoodThemePalettes
{
    private static readonly FoodThemePalette DefaultPalette = new FoodThemePalette(
        "Cuisine",
        new Color32(184, 92, 56, 255),
        new Color32(255, 247, 240, 250),
        new Color32(248, 241, 231, 210),
        new Color32(53, 42, 33, 255),
        new Color32(117, 99, 85, 255));

    public static FoodThemePalette Get(FoodTheme theme)
    {
        switch (theme)
        {
            case FoodTheme.American:
                return new FoodThemePalette(
                    "American",
                    new Color32(179, 47, 47, 255),
                    new Color32(255, 248, 243, 250),
                    new Color32(252, 236, 232, 220),
                    new Color32(58, 36, 31, 255),
                    new Color32(133, 94, 86, 255));
            case FoodTheme.Italian:
                return new FoodThemePalette(
                    "Italian",
                    new Color32(40, 126, 78, 255),
                    new Color32(249, 253, 245, 250),
                    new Color32(235, 246, 236, 220),
                    new Color32(34, 56, 39, 255),
                    new Color32(83, 118, 89, 255));
            case FoodTheme.Mexican:
                return new FoodThemePalette(
                    "Mexican",
                    new Color32(204, 104, 32, 255),
                    new Color32(255, 249, 241, 250),
                    new Color32(250, 233, 214, 220),
                    new Color32(77, 48, 23, 255),
                    new Color32(142, 100, 62, 255));
            case FoodTheme.Japanese:
                return new FoodThemePalette(
                    "Japanese",
                    new Color32(58, 68, 96, 255),
                    new Color32(245, 247, 252, 250),
                    new Color32(227, 233, 245, 220),
                    new Color32(34, 42, 67, 255),
                    new Color32(88, 101, 132, 255));
            default:
                return DefaultPalette;
        }
    }
}
