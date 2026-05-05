using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class FixGuidedAmplitude
{
    [MenuItem("Tools/AR Game/Fix Guided Amplitude")]
    public static void Execute()
    {
        var go = GameObject.Find("VirtualToothbrush");
        if (go == null) { Debug.LogError("[FixGuidedAmplitude] VirtualToothbrush not found."); return; }

        var gmc = go.GetComponent<GuidedModeController>();
        if (gmc == null) { Debug.LogError("[FixGuidedAmplitude] GuidedModeController not found."); return; }

        var so = new SerializedObject(gmc);
        so.Update();

        var hAmp = so.FindProperty("horizontalAmplitude");
        var vAmp = so.FindProperty("verticalAmplitude");

        if (hAmp != null) hAmp.floatValue = 0.009f;
        if (vAmp != null) vAmp.floatValue = 0.004f;

        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),
            "Assets/MiniGames/ARGame/Scenes/ARGame.unity");

        Debug.Log("[FixGuidedAmplitude] Amplitudes set → horizontal=0.009, vertical=0.004. Scene saved.");
    }
}
