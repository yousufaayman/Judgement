using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public TMP_Text healthBarText;
    Damagable playerDamagable;

    private void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerDamagable = player.GetComponent<Damagable>();
    }

    void Start()
    {
        healthSlider.value = CalculateSliderPercentage(playerDamagable.Health, playerDamagable.MaxHealth);
        healthBarText.text = "HP" + playerDamagable.Health + " / " +  playerDamagable.MaxHealth;
    }

    private float CalculateSliderPercentage(float currentHealth, float maxHealth)
    {
        return currentHealth / maxHealth;
    }

    private void OnEnable()
    {
        playerDamagable.healthChanged.AddListener(OnPLayerHealthChanged);
    }

    private void OnDisable()
    {
        playerDamagable.healthChanged.RemoveListener(OnPLayerHealthChanged);
    }

    private void OnPLayerHealthChanged(int newHealth, int maxHealth)
    {
        healthSlider.value = CalculateSliderPercentage(newHealth, maxHealth);
        healthBarText.text = "HP   " + newHealth + " / " + maxHealth;
    }

    void Update()
    {
        
    }
}
