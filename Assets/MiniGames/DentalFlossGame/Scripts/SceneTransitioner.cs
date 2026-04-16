using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransitioner : MonoBehaviour
{
    [Header("UI Config")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1.0f;

    [Header("Time Config")]
    public string nextSceneName;
    public float waitBeforeFade = 3.0f;

    void Start()
    {
        if (fadeGroup != null)
        {
            // Siempre empezamos en blanco (Alpha 1)
            fadeGroup.alpha = 1;

            if (SceneManager.GetActiveScene().name == "Start")
            {
                // En la escena inicial, se aclara muy rápido (0.4 segundos)
                StartCoroutine(DoFade(1, 0, 0.3f));
            }
            else
            {
                // En las demás escenas, usa la duración que pongas en el Inspector
                StartCoroutine(DoFade(1, 0, fadeDuration));
            }
        }

        // Programar la transición a la siguiente escena si hay un nombre asignado
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
        // Fundido de transparente a blanco antes de cambiar de escena
        yield return StartCoroutine(DoFade(0, 1, fadeDuration));
        SceneManager.LoadScene(nextSceneName);
    }

    // Método de fundido con duración ajustable
    IEnumerator DoFade(float start, float end, float duration)
    {
        float counter = 0;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(start, end, counter / duration);
            yield return null;
        }
        fadeGroup.alpha = end;
    }
}