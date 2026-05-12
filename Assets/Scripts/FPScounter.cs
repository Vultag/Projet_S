using TMPro;
using UnityEngine;

public class FPScounter : MonoBehaviour
{
    private TextMeshProUGUI counter;
    void Start()
    {
        counter = this.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        counter.text = ((int)(1f / Time.unscaledDeltaTime)).ToString();
    }
}
