using UnityEngine;

public sealed class PlayerRegistration : MonoBehaviour
{
    [SerializeField] private bool registerOnEnable = true;

    private void OnEnable()
    {
        if (registerOnEnable)
            PlayerRegistry.Register(gameObject);
    }

    private void OnDisable()
    {
        PlayerRegistry.Unregister(gameObject);
    }

    private void OnDestroy()
    {
        PlayerRegistry.Unregister(gameObject);
    }
}