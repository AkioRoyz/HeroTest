public enum DialogueConditionType
{
    None,

    // Проверка минимального уровня игрока
    PlayerLevelAtLeast,

    // Проверка наличия предмета
    HasItem,

    // Проверка отсутствия предмета
    DoesNotHaveItem,

    // Условие "этот шаг доступен только один раз"
    PlayOnce,

    // Заготовка под квестовую систему
    QuestState
}