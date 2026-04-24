using System.Collections.Generic;
using UnityEngine;

public class FoodDatabase : MonoBehaviour
{
    [SerializeField] private List<FoodItem> foodItems = new List<FoodItem>();
    [SerializeField] private FoodTheme activeTheme = FoodTheme.None;

    private void Awake()
    {
        ApplyMissingThemeDefaults();
    }

    public FoodItem GetFoodByMarkerName(string markerName)
    {
        foreach (FoodItem item in foodItems)
        {
            if (item == null)
                continue;

            if (activeTheme == FoodTheme.None || item.theme != activeTheme)
                continue;

            if (item.markerName.Trim().ToLower() == markerName.Trim().ToLower())
                return item;
        }

        return null;
    }

    public void SetActiveTheme(FoodTheme theme)
    {
        activeTheme = theme;
    }

    public FoodTheme GetActiveTheme()
    {
        return activeTheme;
    }

    public bool HasFoodForTheme(FoodTheme theme)
    {
        foreach (FoodItem item in foodItems)
        {
            if (item != null && item.theme == theme)
                return true;
        }

        return false;
    }

    private void ApplyMissingThemeDefaults()
    {
        foreach (FoodItem item in foodItems)
        {
            if (item == null || item.theme != FoodTheme.None)
                continue;

            item.theme = InferTheme(item);
        }
    }

    private static FoodTheme InferTheme(FoodItem item)
    {
        string searchableText = $"{item.markerName} {item.itemName}".ToLowerInvariant();

        if (searchableText.Contains("burger") || searchableText.Contains("hot dog") || searchableText.Contains("fries"))
            return FoodTheme.American;

        if (searchableText.Contains("pizza") || searchableText.Contains("bread") || searchableText.Contains("cheese"))
            return FoodTheme.Italian;

        if (searchableText.Contains("taco") || searchableText.Contains("chili") || searchableText.Contains("chips"))
            return FoodTheme.Mexican;

        if (searchableText.Contains("sushi") || searchableText.Contains("maki") || searchableText.Contains("onigiri"))
            return FoodTheme.Japanese;

        return FoodTheme.None;
    }
}
