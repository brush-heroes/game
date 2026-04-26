using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// Setup step 7: adds GuidedBrush to FacePrefab/Face/MouthZones so the guided-mode
/// toothbrush follows AR face tracking automatically.
/// Run via: Tools ▶ AR Game ▶ Setup Guided Brush
public class ARGameUISetup7
{
    const string PREFAB_PATH      = "Assets/MiniGames/ARGame/Prefabs/FacePrefab.prefab";
    const string SPRITESHEET_PATH = "Assets/MiniGames/ARGame/Assets/toothbrush/brushing.png";

    [MenuItem("Tools/AR Game/Setup Guided Brush")]
    public static void Execute()
    {
        AddGuidedBrushToPrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ARGameUISetup7] Guided brush setup complete.");
    }

    // ── Prefab editing ────────────────────────────────────────────────────────

    static void AddGuidedBrushToPrefab()
    {
        var prefabRoot = PrefabUtility.LoadPrefabContents(PREFAB_PATH);
        if (prefabRoot == null) { Debug.LogError("[Setup7] FacePrefab not found at: " + PREFAB_PATH); return; }

        try
        {
            var mouthZones = prefabRoot.transform.Find("Face/MouthZones");
            if (mouthZones == null)
            {
                Debug.LogError("[Setup7] Could not find Face/MouthZones inside FacePrefab.");
                return;
            }

            // Remove any pre-existing GuidedBrush so re-running is safe
            var existing = mouthZones.Find("GuidedBrush");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // ── Create GuidedBrush GO ────────────────────────────────────────
            var go = new GameObject("GuidedBrush");
            go.transform.SetParent(mouthZones, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            // Match the visual scale of VirtualToothbrush/BrushVisual
            go.transform.localScale = new Vector3(0.016f, 0.016f, 0.016f);
            go.SetActive(false); // hidden until GuidedModeController.Activate() is called

            // ── Components ───────────────────────────────────────────────────
            var sr   = go.AddComponent<SpriteRenderer>();
            var anim = go.AddComponent<SpriteAnimator>();
            anim.Fps = 8f;
            var vis  = go.AddComponent<GuidedBrushVisual>();
            go.AddComponent<Billboard>(); // always faces the camera

            // ── Sprites ──────────────────────────────────────────────────────
            var frames = LoadBrushingFrames();
            if (frames.Length > 0)
            {
                sr.sprite = frames[0]; // default sprite shown on first activation frame

                var so = new SerializedObject(vis);
                so.Update();

                var arr = so.FindProperty("brushingFrames");
                arr.arraySize = frames.Length;
                for (int i = 0; i < frames.Length; i++)
                    arr.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];

                so.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning("[Setup7] No brushing sprites found at: " + SPRITESHEET_PATH);
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PREFAB_PATH);
            Debug.Log("[Setup7] GuidedBrush added to FacePrefab/Face/MouthZones with " + frames.Length + " animation frames.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static Sprite[] LoadBrushingFrames()
    {
        var all = AssetDatabase.LoadAllAssetsAtPath(SPRITESHEET_PATH);
        var list = new List<Sprite>();
        foreach (var a in all)
            if (a is Sprite s) list.Add(s);
        list.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        return list.ToArray();
    }
}
