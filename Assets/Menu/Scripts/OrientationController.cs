using UnityEngine;

/// Add this to any scene that needs a specific screen orientation.
/// On Start it sets the orientation; on Destroy it restores portrait.
public class OrientationController : MonoBehaviour
{
    [SerializeField] bool landscape = true;

    void Start()
    {
        Screen.orientation = landscape
            ? ScreenOrientation.LandscapeLeft
            : ScreenOrientation.Portrait;
    }

    void OnDestroy()
    {
        if (landscape)
            Screen.orientation = ScreenOrientation.Portrait;
    }
}
