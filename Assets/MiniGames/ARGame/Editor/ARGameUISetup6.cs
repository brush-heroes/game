using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// Setup step 6: imports audio clips and wires ARGameAudioManager in the scene.
/// Run via: Tools ▶ AR Game ▶ Setup Audio
public class ARGameUISetup6
{
    const string SOUNDS_BASE = "Assets/MiniGames/ARGame/Assets/sounds/";
    const string SCENE_PATH  = "Assets/MiniGames/ARGame/Scenes/ARGame.unity";

    [MenuItem("Tools/AR Game/Setup Audio")]
    public static void Execute()
    {
        // 1. Configure audio importer settings for each clip
        ImportAudioClips();

        // 2. Find or create the AudioManager GO
        var audioMgr = SetupAudioManagerGO();
        if (audioMgr == null) { Debug.LogError("[ARGameUISetup6] Failed to create AudioManager."); return; }

        // 3. Wire references
        WireAudioManager(audioMgr);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), SCENE_PATH);
        AssetDatabase.SaveAssets();

        Debug.Log("[ARGameUISetup6] Audio setup complete — scene saved.");
    }

    // ── 1. Audio import settings ─────────────────────────────────────────────

    static void ImportAudioClips()
    {
        // Short SFX — decompress on load for instant playback
        string[] sfxClips =
        {
            SOUNDS_BASE + "lower_right.mp3",
            SOUNDS_BASE + "lower_left.mp3",
            SOUNDS_BASE + "lower_front.mp3",
            SOUNDS_BASE + "upper_left.mp3",
            SOUNDS_BASE + "upper_right.mp3",
            SOUNDS_BASE + "upper_front.mp3",
            SOUNDS_BASE + "bacteria_death.mp3",
            SOUNDS_BASE + "bacteria_born.mp3",
        };
        foreach (string p in sfxClips)
            ConfigureAudioClip(p, AudioClipLoadType.DecompressOnLoad, true);

        // Looping brushing — compressed in memory to save RAM
        ConfigureAudioClip(SOUNDS_BASE + "brushing.mp3",
            AudioClipLoadType.CompressedInMemory, true);

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    static void ConfigureAudioClip(string path, AudioClipLoadType loadType, bool mono)
    {
        var imp = AssetImporter.GetAtPath(path) as AudioImporter;
        if (imp == null) { Debug.LogWarning("[ARGameUISetup6] Audio clip not found: " + path); return; }

        imp.forceToMono = mono;
        var settings = imp.defaultSampleSettings;
        settings.loadType           = loadType;
        settings.compressionFormat  = AudioCompressionFormat.Vorbis;
        settings.quality            = 0.7f;
        imp.defaultSampleSettings   = settings;
        imp.SaveAndReimport();
    }

    // ── 2. AudioManager GO ───────────────────────────────────────────────────

    static ARGameAudioManager SetupAudioManagerGO()
    {
        // Reuse existing if already present
        var existing = Object.FindObjectOfType<ARGameAudioManager>();
        if (existing != null)
        {
            Debug.Log("[ARGameUISetup6] ARGameAudioManager already exists — updating refs.");
            return existing;
        }

        var go = new GameObject("ARGameAudioManager");
        return go.AddComponent<ARGameAudioManager>();
    }

    // ── 3. Wire all references ───────────────────────────────────────────────

    static void WireAudioManager(ARGameAudioManager mgr)
    {
        var sm  = Object.FindObjectOfType<SessionManager>();
        var bc  = Object.FindObjectOfType<VirtualBrushController>();

        var so = new SerializedObject(mgr);

        SetRef(so, "sessionManager",    sm);
        SetRef(so, "brushController",   bc);
        SetRef(so, "bacteriaDeathClip", LoadClip("bacteria_death.mp3"));
        SetRef(so, "bacteriaBornClip",  LoadClip("bacteria_born.mp3"));
        SetRef(so, "brushingClip",      LoadClip("brushing.mp3"));

        // Zone sounds array
        var entries = new (MouthZone zone, string file)[]
        {
            (MouthZone.LowerRight, "lower_right.mp3"),
            (MouthZone.LowerLeft,  "lower_left.mp3"),
            (MouthZone.FrontLower, "lower_front.mp3"),
            (MouthZone.UpperLeft,  "upper_left.mp3"),
            (MouthZone.UpperRight, "upper_right.mp3"),
            (MouthZone.FrontUpper, "upper_front.mp3"),
        };

        var arr = so.FindProperty("zoneSounds");
        arr.arraySize = entries.Length;
        for (int i = 0; i < entries.Length; i++)
        {
            var (zone, file) = entries[i];
            var el = arr.GetArrayElementAtIndex(i);
            el.FindPropertyRelative("zone").enumValueIndex = (int)zone;
            el.FindPropertyRelative("clip").objectReferenceValue = LoadClip(file);
        }

        so.ApplyModifiedProperties();
        Debug.Log("[ARGameUISetup6] ARGameAudioManager wired with " + entries.Length + " zone sounds.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    static AudioClip LoadClip(string fileName)
    {
        string path = SOUNDS_BASE + fileName;
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip == null) Debug.LogWarning("[ARGameUISetup6] Clip not found: " + path);
        return clip;
    }

    static void SetRef(SerializedObject so, string prop, Object value)
    {
        var p = so.FindProperty(prop);
        if (p != null) p.objectReferenceValue = value;
        else Debug.LogWarning("[ARGameUISetup6] Property not found: " + prop);
    }
}
