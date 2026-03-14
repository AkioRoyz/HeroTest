using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    // ===== UI ЭЛЕМЕНТЫ =====

    [Header("UI References")]

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


    // ===== ТЕКУЩЕЕ СОСТОЯНИЕ ДИАЛОГА =====

    // Текущий узел диалога
    private DialogueNode currentNode;



    // =========================================================
    // ЗАПУСК ДИАЛОГА
    // =========================================================

    public void StartDialogue(Dialogue dialogue)
    {
        // Берём первый узел диалога
        currentNode = dialogue.startNode;

        // Показываем его
        ShowNode(currentNode);
    }



    // =========================================================
    // ПОКАЗ УЗЛА ДИАЛОГА
    // =========================================================

    void ShowNode(DialogueNode node)
    {
        // Обновляем имя NPC
        npcNameText.text = node.speakerName;

        // Обновляем портрет
        portraitImage.sprite = node.portrait;

        // Получаем локализованный текст
        node.dialogueText.StringChanged += UpdateDialogueText;

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
                buttonText.text = value;
            };


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
        // Пока просто выводим сообщение
        Debug.Log("Dialogue Ended");
    }
}