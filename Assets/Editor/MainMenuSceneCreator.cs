using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class MainMenuSceneCreator
{
    const string ScenePath = "Assets/Scenes/MainMenu.unity";
    const string GameScenePath = "Assets/Scenes/GameScene.unity";
    const string MainMenuPrefabPath = "Assets/Prefabs/UI/MainMenu.prefab";

    [MenuItem("Tools/Create Main Menu Scene")]
    public static void CreateScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var light = GameObject.Find("Directional Light");
        if (light != null)
            Object.DestroyImmediate(light);

        var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        var canvasObject = new GameObject("UI Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1024, 768);

        var menuPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MainMenuPrefabPath);
        if (menuPrefab != null)
            PrefabUtility.InstantiatePrefab(menuPrefab, canvasObject.transform);

        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true),
        };

        AssetDatabase.SaveAssets();
        Debug.Log("Main menu scene created at " + ScenePath);
    }
}
