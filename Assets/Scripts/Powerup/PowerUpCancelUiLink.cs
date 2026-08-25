using UnityEngine;

public class PowerUpCancelUiLink : MonoBehaviour
{
    [HideInInspector]
    public GameObject UiButton;

    private void OnEnable()
    {
        UiButton.SetActive(true);
    }
    private void OnDisable()
    {
        UiButton.SetActive(false);
    }

}
