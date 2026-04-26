using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SaveScene
{
    public static void Execute()
    {
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        UnityEngine.Debug.Log("[SaveScene] Scene saved.");
    }
}
