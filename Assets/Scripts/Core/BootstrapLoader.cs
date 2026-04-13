using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "MainMenu";
    private bool hasLoaded;

    private void Start()
    {
        if (hasLoaded)
            return;

        hasLoaded = true;

        if (string.IsNullOrWhiteSpace(firstSceneName))
        {
            Debug.LogError("[BootstrapLoader] First scene name is empty.", this);
            return;
        }

        if (SceneManager.GetActiveScene().name == firstSceneName)
            return;

        SceneManager.LoadScene(firstSceneName, LoadSceneMode.Single);
    }
}