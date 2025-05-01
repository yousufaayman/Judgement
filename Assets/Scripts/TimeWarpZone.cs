using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TimeWarpZone : MonoBehaviour
{
    [Header("Time Warp Settings")]
    [Tooltip("Multiplier for walk speed (1 = normal speed)")]
    public float walkSpeedMultiplier = 0.5f;
    
    [Tooltip("Multiplier for run speed (1 = normal speed)")]
    public float runSpeedMultiplier = 0.5f;
    
    [Tooltip("Multiplier for jump impulse (1 = normal jump)")]
    public float jumpMultiplier = 0.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ApplyTimeWarp(walkSpeedMultiplier, runSpeedMultiplier, jumpMultiplier);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ResetTimeWarp();
            }
        }
    }
} 