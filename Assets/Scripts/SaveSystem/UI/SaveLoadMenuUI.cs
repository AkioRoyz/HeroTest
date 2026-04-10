using System;
using TMPro;
using UnityEngine;

public class SaveLoadMenuUI : MonoBehaviour
{
    public event Action OnWindowClosed;

    private enum WindowMode
    {
        Save,
        Load
    }

    [Header("Root")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text titleText;

    [Header("Regular Slots")]
    [SerializeField] private SaveSlotButtonUI[] regularSlotButtons;

    [Header("Auto Slot")]
    [SerializeField] private SaveSlotButtonUI autoSlotButton;
    [SerializeField] private bool showAutoSlotInLoadMode = true;
    [SerializeField] private bool showAutoSlotInSaveMode = false;

    [Header("Labels")]
    [SerializeField] private string saveTitle = "Сохранение";
    [SerializeField] private string loadTitle = "Загрузка";
    [SerializeField] private string emptySlotText = "Пусто";
    [SerializeField] private string corruptedSlotText = "Повреждено";
    [SerializeField] private string autoSlotTitle = "Автосохранение";
    [SerializeField] private string manualSlotTitlePrefix = "Слот";

    private WindowMode currentMode = WindowMode.Load;

    public bool IsOpen => root != null && root.activeSelf;

    private void Awake()
    {
        if (root != null)
            root.SetActive(false);
    }

    private void OnEnable()
    {
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.OnSaveSlotsChanged += Refresh;
    }

    private void OnDisable()
    {
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.OnSaveSlotsChanged -= Refresh;
    }

    public void OpenSaveMode()
    {
        currentMode = WindowMode.Save;
        OpenInternal();
    }

    public void OpenLoadMode()
    {
        currentMode = WindowMode.Load;
        OpenInternal();
    }

    public void Close()
    {
        if (root == null)
            return;

        bool wasOpen = root.activeSelf;
        root.SetActive(false);

        if (wasOpen)
            OnWindowClosed?.Invoke();
    }

    public void Refresh()
    {
        if (root == null || !root.activeSelf)
            return;

        if (titleText != null)
            titleText.text = currentMode == WindowMode.Save ? saveTitle : loadTitle;

        SaveSystem saveSystem = SaveSystem.Instance;
        if (saveSystem == null)
        {
            Debug.LogWarning("[SaveLoadMenuUI] SaveSystem.Instance is missing.", this);
            return;
        }

        for (int i = 0; i < regularSlotButtons.Length; i++)
        {
            SaveSlotButtonUI slotButton = regularSlotButtons[i];
            if (slotButton == null)
                continue;

            int slotIndex = i + 1;
            bool shouldShow = slotIndex <= saveSystem.RegularSlotCount;
            slotButton.gameObject.SetActive(shouldShow);

            if (!shouldShow)
                continue;

            SaveSlotInfo info = saveSystem.GetRegularSlotInfo(slotIndex);
            ConfigureRegularSlotButton(slotButton, slotIndex, info);
        }

        ConfigureAutoSlotButton();
    }

    private void OpenInternal()
    {
        if (root != null)
            root.SetActive(true);

        Refresh();
    }

    private void ConfigureRegularSlotButton(SaveSlotButtonUI slotButton, int slotIndex, SaveSlotInfo info)
    {
        string title = $"{manualSlotTitlePrefix} {slotIndex}";
        string details = BuildDetailsText(info);
        bool showDelete = info.Exists;
        bool mainInteractable = currentMode == WindowMode.Save || info.Exists;

        slotButton.Configure(
            title,
            details,
            mainInteractable,
            showDelete,
            () => HandleRegularSlotMainClick(slotIndex),
            () => HandleRegularSlotDeleteClick(slotIndex));
    }

    private void ConfigureAutoSlotButton()
    {
        if (autoSlotButton == null)
            return;

        bool shouldShow = currentMode == WindowMode.Load ? showAutoSlotInLoadMode : showAutoSlotInSaveMode;
        autoSlotButton.gameObject.SetActive(shouldShow);

        if (!shouldShow)
            return;

        SaveSystem saveSystem = SaveSystem.Instance;
        SaveSlotInfo info = saveSystem != null ? saveSystem.GetAutoSlotInfo() : null;

        string details = BuildDetailsText(info);
        bool mainInteractable = currentMode == WindowMode.Save || (info != null && info.Exists);
        bool showDelete = currentMode == WindowMode.Load && info != null && info.Exists;

        autoSlotButton.Configure(
            autoSlotTitle,
            details,
            mainInteractable,
            showDelete,
            HandleAutoSlotMainClick,
            HandleAutoSlotDeleteClick);
    }

    private string BuildDetailsText(SaveSlotInfo info)
    {
        if (info == null || !info.Exists)
            return emptySlotText;

        if (info.IsCorrupted)
            return corruptedSlotText;

        return $"Локация: {info.SceneName}\nУровень: {info.PlayerLevel}\nДата: {info.SavedAtText}";
    }

    private void HandleRegularSlotMainClick(int slotIndex)
    {
        SaveSystem saveSystem = SaveSystem.Instance;
        if (saveSystem == null)
            return;

        if (currentMode == WindowMode.Save)
        {
            if (saveSystem.SaveToRegularSlot(slotIndex))
                Close();

            return;
        }

        saveSystem.LoadFromRegularSlot(slotIndex);
        Close();
    }

    private void HandleRegularSlotDeleteClick(int slotIndex)
    {
        SaveSystem saveSystem = SaveSystem.Instance;
        if (saveSystem == null)
            return;

        saveSystem.DeleteRegularSlot(slotIndex);
        Refresh();
    }

    private void HandleAutoSlotMainClick()
    {
        SaveSystem saveSystem = SaveSystem.Instance;
        if (saveSystem == null)
            return;

        if (currentMode == WindowMode.Save)
        {
            if (saveSystem.SaveToAutoSlot())
                Close();

            return;
        }

        saveSystem.LoadFromAutoSlot();
        Close();
    }

    private void HandleAutoSlotDeleteClick()
    {
        SaveSystem saveSystem = SaveSystem.Instance;
        if (saveSystem == null)
            return;

        saveSystem.DeleteAutoSlot();
        Refresh();
    }
}