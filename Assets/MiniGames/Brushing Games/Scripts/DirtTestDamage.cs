using UnityEngine;

public class DirtTestDamage : MonoBehaviour
{
    public DirtHealth targetDirt;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (targetDirt != null)
            {
                targetDirt.TakeDamage(1);
            }
        }
    }
}