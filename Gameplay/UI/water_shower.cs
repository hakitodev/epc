using UnityEngine;
using UnityEngine.UI;

public class water_shower : MonoBehaviour
{
    [SerializeField] private bool w_one, w_two;
    [SerializeField] private Transform pos_down;
    [SerializeField] private Image water;
    [SerializeField] private float speed;

    public void FixedUpdate() { // rend_water() {
        if (w_one && w_two) {
            if (pos_down.position.y <= 24f)
                water.fillAmount = 1f;
            else water.fillAmount = 0f;
            // else water.fillAmount = (24.99542f - pos_down.position.y) * speed;
        }
    }
}
