using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "SampleScene";

    private bool hasLoaded;

    private void Start()
    {
        if (hasLoaded)
            return;

        hasLoaded = true;

        if (string.IsNullOrWhiteSpace(firstSceneName))
        {
            Debug.LogError("[BootLoader] First scene name is empty.", this);
            return;
        }

        SceneManager.LoadScene(firstSceneName, LoadSceneMode.Single);
    }
}