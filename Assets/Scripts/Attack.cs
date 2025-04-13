using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public int attackDamage = 10;
    public Vector2 knockBack = Vector2.zero;

    private void OnTriggerEnter2D(Collider2D collision)
    {
       Damagable damagable = collision.GetComponent<Damagable>();
        if (damagable != null) {

            bool gotHit = damagable.Hit(attackDamage, knockBack);

        }
    }
}
