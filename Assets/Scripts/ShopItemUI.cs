using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI statsText;
    public Image icon;

    /// <summary>
    /// Setup for Building Objects
    /// </summary>
    public void Setup(GameObject buildingPrefab, int price, int moneyPerPerson, int faithPerPerson)
    {
        Building building = buildingPrefab.GetComponent<Building>();
        if (building != null)
        {
            nameText.text = buildingPrefab.name;
            priceText.text = $"Price: {price}";
            statsText.text = $"Money gained: {moneyPerPerson}\nFaith gained: {faithPerPerson}";
        }

        SetIcon(buildingPrefab.GetComponent<SpriteRenderer>().sprite);
    }

    /// <summary>
    /// Setup for Wall Objects
    /// </summary>
    public void SetupWall(GameObject wallPrefab)
    {
        WallObject wallObject = wallPrefab.GetComponent<WallObject>();
        if (wallObject == null) return;

        nameText.text = wallObject.wallName;
        priceText.text = $"Price: {wallObject.price}";
        statsText.text = "";

        // Extract first sprite from the SpriteAtlas
        Sprite firstSprite = GetFirstSpriteFromAtlas(wallObject.wallAtlas, 0);
        if (firstSprite != null)
        {
            SetIcon(firstSprite);
        }
    }

    public void SetupPath(GameObject wallPrefab)
    {
        PathObject pathObject = wallPrefab.GetComponent<PathObject>();
        if (pathObject == null) return;

        nameText.text = pathObject.wallName;
        priceText.text = $"Price: {pathObject.price}";
        statsText.text = "";

        // Extract first sprite from the SpriteAtlas
        Sprite firstSprite = GetFirstSpriteFromAtlas(pathObject.pathAtlas, 5);
        if (firstSprite != null)
        {
            SetIcon(firstSprite);
        }
    }

    /// <summary>
    /// Retrieves the first sprite from a SpriteAtlas
    /// </summary>
    private Sprite GetFirstSpriteFromAtlas(SpriteAtlas atlas, int index)
    {
        if (atlas == null)
        {
            Debug.LogError("SpriteAtlas is missing for wall.");
            return null;
        }

        Sprite[] sprites = new Sprite[atlas.spriteCount];
        atlas.GetSprites(sprites);

        // Sort the sprites by their names based on numbers extracted from the name
        var sortedSprites = sprites
            .OrderBy(sprite => ExtractNumberFromName(sprite.name))
            .ToList();

        // Return the first sprite in the sorted list, or null if there are no sprites
        return sortedSprites.Count > 0 ? sortedSprites[index] : null;
    }

    private int ExtractNumberFromName(string name)
    {
        // Extract the first number found in the name
        Match match = Regex.Match(name, @"\d+");
        return match.Success ? int.Parse(match.Value) : int.MaxValue; // If no number, push to the end
    }

    /// <summary>
    /// Set the icon image and adjust its size
    /// </summary>
    private void SetIcon(Sprite sprite)
    {
        if (sprite == null) return;
        icon.sprite = sprite;

        // Adjust icon size while maintaining aspect ratio
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        float spriteAspect = sprite.rect.width / sprite.rect.height;
        iconRect.sizeDelta = new Vector2(iconRect.sizeDelta.y * spriteAspect, iconRect.sizeDelta.y);
    }
}
