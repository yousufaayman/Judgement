using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KillZone : MonoBehaviour
{
    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Damagable damagable = collision.GetComponent<Damagable>();
        if (damagable != null)
        {
            damagable.Health = 0;
        }
    }
}
