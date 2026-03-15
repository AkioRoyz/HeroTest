using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    // ===== UI ЭЛЕМЕНТЫ =====

    [Header("UI References")]

    [SerializeField] private GameObject dialoguePanel;

    // Текст имени NPC
    [SerializeField] private TMP_Text npcNameText;

    // Основной текст диалога
    [SerializeField] private TMP_Text dialogueText;

    // Портрет персонажа
    [SerializeField] private Image portraitImage;

    // Контейнер, где будут появляться кнопки ответов
    [SerializeField] private Transform choicesContainer;

    // Префаб кнопки ответа
    [SerializeField] private GameObject choiceButtonPrefab;

    //Подключение к квестам
    [SerializeField] private DialogueEventSystem eventSystem;


    // ===== ТЕКУЩЕЕ СОСТОЯНИЕ ДИАЛОГА =====

    // Текущий узел диалога
    private DialogueNode currentNode;

    // Текущий локализованный текст (чтобы можно было отписаться)
    private UnityEngine.Localization.LocalizedString currentDialogueLocalized;

    private UnityEngine.Localization.LocalizedString currentNpcName;

    private void Start()
    {
        dialoguePanel.SetActive(false);
    }

    // =========================================================
    // ЗАПУСК ДИАЛОГА
    // =========================================================

    public void StartDialogue(Dialogue dialogue, UnityEngine.Localization.LocalizedString npcName)
    {
        dialoguePanel.SetActive(true);

        currentNpcName = npcName;

        currentNpcName.StringChanged += UpdateNpcName;
        currentNpcName.RefreshString();

        // Берём первый узел диалога
        currentNode = dialogue.startNode;

        // Показываем его
        ShowNode(currentNode);
    }

    void UpdateNpcName(string value)
    {
        npcNameText.text = value;
    }

    // =========================================================
    // ПОКАЗ УЗЛА ДИАЛОГА
    // =========================================================

    void ShowNode(DialogueNode node)
    {
        // Обновляем портрет
        portraitImage.sprite = node.portrait;

        // Получаем локализованный текст
        // Отписываемся от предыдущего текста
        if (currentDialogueLocalized != null)
        {
            currentDialogueLocalized.StringChanged -= UpdateDialogueText;
        }

        // Сохраняем новый локализованный текст
        currentDialogueLocalized = node.dialogueText;

        // Подписываемся на обновление
        currentDialogueLocalized.StringChanged += UpdateDialogueText;

        // Принудительно обновляем текст
        currentDialogueLocalized.RefreshString();

        // Если нет вариантов ответа — заканчиваем диалог
        if (node.choices == null || node.choices.Count == 0)
        {
            EndDialogue();
            return;
        }

        // Создаём варианты ответов
        CreateChoices(node.choices);
    }



    // =========================================================
    // ОБНОВЛЕНИЕ ТЕКСТА (Localization)
    // =========================================================

    void UpdateDialogueText(string value)
    {
        dialogueText.text = value;
    }



    // =========================================================
    // СОЗДАНИЕ КНОПОК ОТВЕТОВ
    // =========================================================

    void CreateChoices(List<DialogueChoice> choices)
    {
        // Удаляем старые кнопки
        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }

        // Создаём новые
        foreach (DialogueChoice choice in choices)
        {
            // Создаём кнопку
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer);

            // Получаем текст кнопки
            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();

            // Получаем компонент Button
            Button button = buttonObj.GetComponent<Button>();


            // Подписываемся на локализацию текста
            choice.choiceText.StringChanged += (value) =>
            {
                if (buttonText != null)
                    buttonText.text = value;
            };

            choice.choiceText.RefreshString();


            // Добавляем действие при нажатии
            button.onClick.AddListener(() =>
            {
                OnChoiceSelected(choice);
            });
        }
    }



    // =========================================================
    // КОГДА ИГРОК ВЫБРАЛ ОТВЕТ
    // =========================================================

    void OnChoiceSelected(DialogueChoice choice)
    {
        // Запускаем событие диалога
        if (choice.eventType != DialogueEventType.None)
        {
            eventSystem.TriggerEvent(choice.eventType, choice.eventID);
        }

        // Если есть следующий узел — переходим к нему
        if (choice.nextNode != null)
        {
            currentNode = choice.nextNode;
            ShowNode(currentNode);
        }
        else
        {
            EndDialogue();
        }
    }



    // =========================================================
    // КОНЕЦ ДИАЛОГА
    // =========================================================

    void EndDialogue()
    {
        // Отписываемся от локализации
        if (currentDialogueLocalized != null)
        {
            currentDialogueLocalized.StringChanged -= UpdateDialogueText;
            currentDialogueLocalized = null;
        }

        // Очищаем кнопки
        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }

        // Очищаем текст
        dialogueText.text = "";
        npcNameText.text = "";

        dialoguePanel.SetActive(false);

        if (currentNpcName != null)
        {
            currentNpcName.StringChanged -= UpdateNpcName;
            currentNpcName = null;
        }

        Debug.Log("Dialogue Ended");
    }
}