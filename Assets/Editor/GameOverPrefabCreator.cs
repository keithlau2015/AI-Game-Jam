using Platformer.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class GameOverPrefabCreator
{
    const string Folder = "Assets/Prefabs/UI";

    [MenuItem("Tools/Create Game Over UI Prefabs")]
    public static void CreatePrefabs()
    {
        if (!AssetDatabase.IsValidFolder(Folder))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

        CreatePanel(
            Folder + "/WinPanel.prefab",
            "WinPanel",
            new Color(0.06f, 0.14f, 0.10f, 0.95f),
            new Color(0.35f, 0.85f, 0.55f, 1f),
            "Victory",
            "Your team reached a winning milestone.",
            new Color(0.18f, 0.55f, 0.38f, 1f));

        CreatePanel(
            Folder + "/LosePanel.prefab",
            "LosePanel",
            new Color(0.16f, 0.06f, 0.08f, 0.95f),
            new Color(0.95f, 0.45f, 0.45f, 1f),
            "Defeat",
            "A critical value dropped too low.",
            new Color(0.72f, 0.22f, 0.22f, 1f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Game over UI prefabs created in " + Folder);
    }

    static void CreatePanel(
        string prefabPath,
        string rootName,
        Color backdropColor,
        Color titleColor,
        string title,
        string message,
        Color buttonColor)
    {
        var root = new GameObject(rootName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(GameOverPanelView));
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        var backdrop = root.GetComponent<Image>();
        backdrop.color = backdropColor;

        var card = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        card.transform.SetParent(root.transform, false);
        var cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.2f, 0.22f);
        cardRect.anchorMax = new Vector2(0.8f, 0.78f);
        cardRect.offsetMin = Vector2.zero;
        cardRect.offsetMax = Vector2.zero;
        card.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.16f, 0.98f);

        var titleText = CreateText(card.transform, "Title", title, 52, titleColor, new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.92f));
        var messageText = CreateText(card.transform, "Message", message, 24, new Color(0.82f, 0.87f, 0.95f, 1f), new Vector2(0.1f, 0.52f), new Vector2(0.9f, 0.7f));
        var statsText = CreateText(card.transform, "Stats", "Karma 50   Morale 50   Reputation 50", 20, new Color(0.7f, 0.76f, 0.88f, 1f), new Vector2(0.1f, 0.34f), new Vector2(0.9f, 0.5f));

        var buttonObject = new GameObject("RestartButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(card.transform, false);
        var buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.28f, 0.1f);
        buttonRect.anchorMax = new Vector2(0.72f, 0.24f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        buttonObject.GetComponent<Image>().color = buttonColor;

        var buttonLabel = CreateText(buttonObject.transform, "Label", "Play Again", 26, Color.white, Vector2.zero, Vector2.one);

        var view = root.GetComponent<GameOverPanelView>();
        view.titleText = titleText;
        view.messageText = messageText;
        view.statsText = statsText;
        view.restartButton = buttonObject.GetComponent<Button>();

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
    }

    static TMP_Text CreateText(Transform parent, string name, string content, float fontSize, Color color, Vector2 anchorMin, Vector2 anchorMax)
    {
        var textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        var rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        return text;
    }
}
