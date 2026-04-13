using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class SceneContextBootstrap : MonoBehaviour
{
    public enum SceneInputMode
    {
        None,
        Gameplay,
        BootExplore,
        MenuOnly,
        Cutscene
    }

    [SerializeField] private SceneInputMode modeOnStart = SceneInputMode.None;
    [SerializeField] private bool resetTimeScaleToOne = true;
    [SerializeField] private GameObject firstSelectedObject;
    [SerializeField] private bool selectOnNextFrame = true;

    private IEnumerator Start()
    {
        if (resetTimeScaleToOne)
            Time.timeScale = 1f;

        ApplyInputMode();

        if (firstSelectedObject != null)
        {
            if (selectOnNextFrame)
                yield return null;

            Select(firstSelectedObject);
        }
    }

    private void ApplyInputMode()
    {
        GameInput input = GameInput.Instance;
        if (input == null)
            return;

        switch (modeOnStart)
        {
            case SceneInputMode.Gameplay:
                input.SetGameplayMode();
                break;

            case SceneInputMode.BootExplore:
                input.SetBootExploreMode();
                break;

            case SceneInputMode.MenuOnly:
                input.SetMenuMode();
                break;

            case SceneInputMode.Cutscene:
                input.SetCutsceneMode();
                break;
        }
    }

    private static void Select(GameObject target)
    {
        if (target == null || EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target);
    }
}