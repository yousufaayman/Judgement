using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyTimeWarpZone : MonoBehaviour
{
    [Header("Time Warp Settings")]
    [Tooltip("Multiplier for enemy walk speed (1 = normal speed)")]
    public float walkSpeedMultiplier = 0.5f;
    
    [Tooltip("Multiplier for enemy attack speed (1 = normal speed)")]
    public float attackSpeedMultiplier = 0.5f;

    private void Start()
    {
        // Ensure collider is set to trigger
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Try SlothEnemy first
            SlothEnemy enemy = other.GetComponent<SlothEnemy>();
            if (enemy != null)
            {
                enemy.ApplyTimeWarp(walkSpeedMultiplier, attackSpeedMultiplier);
                return;
            }

            // Try WrathBoss if not a SlothEnemy
            WrathBoss boss = other.GetComponent<WrathBoss>();
            if (boss != null)
            {
                boss.ApplyTimeWarp(walkSpeedMultiplier, attackSpeedMultiplier);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Try SlothEnemy first
            SlothEnemy enemy = other.GetComponent<SlothEnemy>();
            if (enemy != null)
            {
                enemy.ResetTimeWarp();
                return;
            }

            // Try WrathBoss if not a SlothEnemy
            WrathBoss boss = other.GetComponent<WrathBoss>();
            if (boss != null)
            {
                boss.ResetTimeWarp();
            }
        }
    }
} 