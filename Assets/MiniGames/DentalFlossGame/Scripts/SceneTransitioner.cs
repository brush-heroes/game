using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransitioner : MonoBehaviour
{
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1.0f;

    public string nextSceneName;
    public float waitBeforeFade = 3.0f;

    void Start()
    {
        if (fadeGroup != null)
        {
            if (SceneManager.GetActiveScene().name == "Start")
            {
                fadeGroup.alpha = 0;
            }
            else
            {
                fadeGroup.alpha = 1;
                StartCoroutine(DoFade(1, 0));
            }
        }

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Invoke("StartTransition", waitBeforeFade);
        }
    }

    public void StartTransition()
    {
        StartCoroutine(TransitionRoutine());
    }

    IEnumerator TransitionRoutine()
    {
        yield return StartCoroutine(DoFade(0, 1));
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator DoFade(float start, float end)
    {
        float counter = 0;
        while (counter < fadeDuration)
        {
            counter += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(start, end, counter / fadeDuration);
            yield return null;
        }
    }
}
