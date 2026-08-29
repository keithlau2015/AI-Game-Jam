using Platformer.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class MainMenuPrefabCreator
{
    const string Folder = "Assets/Prefabs/UI";
    const string PrefabPath = Folder + "/MainMenu.prefab";

    [MenuItem("Tools/Create Main Menu Prefab")]
    public static void CreatePrefab()
    {
        if (!AssetDatabase.IsValidFolder(Folder))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

        var root = new GameObject("MainMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(MainMenuPanelView));
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        root.GetComponent<Image>().color = new Color(0.05f, 0.07f, 0.12f, 0.94f);

        var card = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        card.transform.SetParent(root.transform, false);
        var cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.18f, 0.2f);
        cardRect.anchorMax = new Vector2(0.82f, 0.8f);
        cardRect.offsetMin = Vector2.zero;
        cardRect.offsetMax = Vector2.zero;
        card.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.16f, 0.98f);

        var titleText = CreateText(card.transform, "Title", "Worker Shift", 52, new Color(0.35f, 0.85f, 0.55f, 1f), new Vector2(0.08f, 0.68f), new Vector2(0.92f, 0.9f));
        var subtitleText = CreateText(card.transform, "Subtitle", "Drag workers to matching stations before time runs out.", 24, new Color(0.82f, 0.87f, 0.95f, 1f), new Vector2(0.1f, 0.48f), new Vector2(0.9f, 0.66f));

        var buttonObject = new GameObject("StartButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(card.transform, false);
        var buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.28f, 0.12f);
        buttonRect.anchorMax = new Vector2(0.72f, 0.28f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        buttonObject.GetComponent<Image>().color = new Color(0.22f, 0.45f, 0.78f, 1f);
        CreateText(buttonObject.transform, "Label", "Start Game", 28, Color.white, Vector2.zero, Vector2.one);

        var view = root.GetComponent<MainMenuPanelView>();
        view.titleText = titleText;
        view.subtitleText = subtitleText;
        view.startButton = buttonObject.GetComponent<Button>();

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Main menu prefab created at " + PrefabPath);
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
