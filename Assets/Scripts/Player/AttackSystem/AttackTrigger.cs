using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    [SerializeField] private StatsSystem statsSystem;

    private void Awake()
    {
        ResolveReferences();
    }

    private void ResolveReferences()
    {
        if (statsSystem == null)
            statsSystem = FindFirstObjectByType<StatsSystem>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out EnemyHealth enemyHealth))
            return;

        int strength = statsSystem != null ? statsSystem.Strength : 1;
        int attackDamage = Mathf.RoundToInt(strength * Random.Range(0.8f, 1.2f));
        enemyHealth.TakeDamage(attackDamage);
    }
}