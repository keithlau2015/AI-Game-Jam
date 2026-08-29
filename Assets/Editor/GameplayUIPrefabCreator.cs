using Platformer.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.Editor
{
    public static class GameplayUIPrefabCreator
    {
        const string TestScenePath = "Assets/Scenes/UI test scene.unity";
        const string PrefabPath = "Assets/UI/prefab/GameplayHUD_p.prefab";
        const string CharCardPath = "Assets/UI/prefab/CharCard_p.prefab";

        [MenuItem("Platformer/Create Gameplay HUD Prefab")]
        public static void CreateGameplayHudPrefab()
        {
            if (!System.IO.File.Exists(TestScenePath))
            {
                Debug.LogError("Missing UI test scene at " + TestScenePath);
                return;
            }

            var activeScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.OpenScene(TestScenePath, OpenSceneMode.Single);

            var canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                Debug.LogError("Canvas not found in UI test scene.");
                return;
            }

            var rect = canvas.GetComponent<RectTransform>();
            if (rect != null)
                rect.localScale = Vector3.one;

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }

            var hudRoot = new GameObject("GameplayHUD", typeof(RectTransform));
            var hudRect = hudRoot.GetComponent<RectTransform>();
            hudRect.SetParent(canvas.transform, false);
            hudRect.anchorMin = Vector2.zero;
            hudRect.anchorMax = Vector2.one;
            hudRect.offsetMin = Vector2.zero;
            hudRect.offsetMax = Vector2.zero;
            hudRect.localScale = Vector3.one;

            MoveChild(canvas.transform, hudRoot.transform, "Bg");
            MoveChild(canvas.transform, hudRoot.transform, "FamilyStatic_list");
            MoveChild(canvas.transform, hudRoot.transform, "ChatCarts");
            MoveChild(canvas.transform, hudRoot.transform, "date");

            var bg = hudRoot.transform.Find("Bg");
            if (bg != null)
                bg.gameObject.SetActive(false);

            var hudView = hudRoot.GetComponent<GameplayHUDView>();
            if (hudView == null)
                hudView = hudRoot.AddComponent<GameplayHUDView>();

            hudView.EnsureBindings();
            hudView.charCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharCardPath);

            var familyList = hudRoot.transform.Find("FamilyStatic_list");
            if (familyList != null)
            {
                for (var i = 0; i < familyList.childCount; i++)
                {
                    var child = familyList.GetChild(i);
                    if (child.GetComponent<FamilyStatBarPanelView>() == null)
                        child.gameObject.AddComponent<FamilyStatBarPanelView>();
                }
            }

            var roster = hudRoot.transform.Find("ChatCarts");
            if (roster != null)
            {
                for (var i = roster.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(roster.GetChild(i).gameObject);
            }

            var strayDate = hudRoot.transform.Find("Date");
            if (strayDate != null)
                Object.DestroyImmediate(strayDate.gameObject);

            hudView.EnsureBindings();

            EnsureFolder("Assets/UI");
            EnsureFolder("Assets/UI/prefab");

            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null)
                AssetDatabase.DeleteAsset(PrefabPath);

            var prefab = PrefabUtility.SaveAsPrefabAsset(hudRoot, PrefabPath);
            Object.DestroyImmediate(hudRoot);

            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!string.IsNullOrEmpty(activeScene.path))
                EditorSceneManager.OpenScene(activeScene.path, OpenSceneMode.Single);

            Debug.Log("Gameplay HUD prefab created at " + PrefabPath);
        }

        static void MoveChild(Transform source, Transform destination, string childName)
        {
            var child = source.Find(childName);
            if (child == null)
                return;

            child.SetParent(destination, false);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
            var folderName = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
