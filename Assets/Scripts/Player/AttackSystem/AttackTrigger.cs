using UnityEngine;

public class AttackTrigger : MonoBehaviour
{

    [SerializeField] StatsSystem statsSystem;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out EnemyHealth enemyHealth))
        {
            int attackDamage = Mathf.RoundToInt(statsSystem.Strength * Random.Range(0.8f, 1.2f));
            enemyHealth.TakeDamage(attackDamage);
        }
    }
}
