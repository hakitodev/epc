using UnityEngine;
using UnityEngine.UI;

public class WaterTrigger : MonoBehaviour
{
    [SerializeField] private Image water;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Water")
        {
            water.fillAmount = 1f;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Water")
        {
            water.fillAmount = 0f;
        }
    }
}
