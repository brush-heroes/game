using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoTransition : MonoBehaviour
{
    public float waitTime = 3.5f;
    public string secondSceneName = "Zoom";

    void Start()
    {
        Invoke("ChangeScene", waitTime);
    }

    void ChangeScene()
    {
        SceneManager.LoadScene(secondSceneName);
    }
}
