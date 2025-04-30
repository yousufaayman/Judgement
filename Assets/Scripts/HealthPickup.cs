using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int healthRestore = 20;
    AudioSource pickupSource;

    private void Awake()
    {
        pickupSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    private void OnTriggerEnter2D( Collider2D collision)
    {
        Damagable damagable = collision.GetComponent<Damagable>();

        if (damagable)
        {
            damagable.Heal(healthRestore);
            AudioSource.PlayClipAtPoint(pickupSource.clip, gameObject.transform.position, pickupSource.volume);
            Destroy(gameObject);
        }
    }
}
