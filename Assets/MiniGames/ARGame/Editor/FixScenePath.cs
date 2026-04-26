using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// Copies the scene saved at Assets/ARGame.unity (wrong path from MCP tool) to
/// Assets/MiniGames/ARGame/Scenes/ARGame.unity (the path Build Settings expects).
public class FixScenePath
{
    [MenuItem("Tools/AR Game/Fix Scene Path")]
    public static void Execute()
    {
        const string wrongPath   = "Assets/ARGame.unity";
        const string correctPath = "Assets/MiniGames/ARGame/Scenes/ARGame.unity";

        // Open the scene that has all the new UI (the MCP tool saved it here)
        var scene = EditorSceneManager.OpenScene(wrongPath, OpenSceneMode.Single);
        UnityEngine.Debug.Log("[FixScenePath] Opened: " + scene.path);

        // Save it to the Build Settings path (overwrites the old file)
        bool saved = EditorSceneManager.SaveScene(scene, correctPath, saveAsCopy: false);
        UnityEngine.Debug.Log("[FixScenePath] Saved to " + correctPath + " : " + saved);

        // Remove the stray file at the root of Assets
        AssetDatabase.DeleteAsset(wrongPath);
        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("[FixScenePath] Deleted stray: " + wrongPath);
        UnityEngine.Debug.Log("[FixScenePath] Done — Build Settings scene is now up to date.");
    }
}
