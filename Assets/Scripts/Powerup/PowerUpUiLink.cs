using UnityEngine;

public class PowerUpUiLink : MonoBehaviour
{
    [HideInInspector]
    public GameObject UIobject;

    private void OnEnable()
    {
        UIobject.SetActive(true);
    }
    private void OnDisable()
    {
        UIobject.SetActive(false);
    }

}
