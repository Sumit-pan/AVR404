using UnityEngine;

[System.Serializable]
public enum FoodTheme
{
    None = 0,
    American = 1,
    Italian = 2,
    Mexican = 3,
    Japanese = 4
}

[System.Serializable]
public class FoodItem
{
    public FoodTheme theme;
    public string markerName;
    public string itemName;

    [TextArea(3, 6)]
    public string description;

    [Header("Macros")]
    [Min(0)] public int calories;
    [Min(0f)] public float protein;
    [Min(0f)] public float carbs;
    [Min(0f)] public float fat;

    public GameObject foodPrefab;

    public string GetMacrosText()
    {
        return $"Calories: {calories}\nProtein: {protein:0.#}g\nCarbs: {carbs:0.#}g\nFat: {fat:0.#}g";
    }

    public string GetThemeDisplayName()
    {
        return theme == FoodTheme.None ? "Unassigned" : theme.ToString();
    }
}
