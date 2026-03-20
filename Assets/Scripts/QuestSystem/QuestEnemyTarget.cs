using UnityEngine;

public class QuestEnemyTarget : MonoBehaviour
{
    [Header("Quest Enemy")]
    [SerializeField] private EnemyType enemyType = EnemyType.Any;

    public EnemyType EnemyType => enemyType;

    // Этот метод нужно будет вызывать из системы смерти врага,
    // когда ты позже сделаешь полноценную систему противников.
    public void NotifyKilled()
    {
        if (QuestSystem.Instance == null)
            return;

        QuestSystem.Instance.RegisterEnemyKilled(enemyType);
    }
}