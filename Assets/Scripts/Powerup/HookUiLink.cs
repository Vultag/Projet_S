using UnityEngine;

public class HookUiLink : MonoBehaviour
{
    [HideInInspector]
    public GameObject UiButton;

    //private Hook hook;
    //private void Start()
    //{
    //    hook = GetComponent<Hook>();
    //}
    private void OnEnable()
    {
        UiButton.SetActive(true);
    }
    private void OnDisable()
    {
        UiButton.SetActive(false);
    }

}
