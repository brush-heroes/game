using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class BacteriaSpawner : MonoBehaviour
{
    public GameObject bacteriaPrefab;

    [SerializeField] ARFaceManager faceManager;
    [SerializeField] float spawnInterval = 2f;
    [SerializeField] float randomOffsetRadius = 0.05f;

    const string MouthAnchorName = "MouthAnchor";

    void Start()
    {
        if (faceManager == null)
            faceManager = FindObjectOfType<ARFaceManager>();

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            TrySpawn();
        }
    }

    void TrySpawn()
    {
        if (faceManager == null || bacteriaPrefab == null)
            return;

        Transform mouthAnchor = GetMouthAnchor();
        if (mouthAnchor == null)
            return;

        Vector3 localOffset = new Vector3(
            Random.Range(-randomOffsetRadius, randomOffsetRadius),
            Random.Range(-randomOffsetRadius, randomOffsetRadius),
            Random.Range(-randomOffsetRadius, randomOffsetRadius)
        );

        GameObject instance = Instantiate(bacteriaPrefab, mouthAnchor);
        instance.transform.localPosition = localOffset;
        instance.transform.localRotation = Quaternion.identity;
    }

    Transform GetMouthAnchor()
    {
        foreach (ARFace face in faceManager.trackables)
        {
            Transform anchor = face.transform.Find(MouthAnchorName);
            if (anchor != null)
                return anchor;
            anchor = FindInChildren(face.transform, MouthAnchorName);
            if (anchor != null)
                return anchor;
            return face.transform;
        }
        return null;
    }

    static Transform FindInChildren(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
            Transform found = FindInChildren(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
}
