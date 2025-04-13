using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealthText : MonoBehaviour
{
    public Vector3 moveSpeed = new Vector3(0, 75, 0);
    public float timeToFade = 1f;
    
    private float timeElapsed;
    private Color startColor;

    TextMeshProUGUI textMeshPro;
    RectTransform textTransform;

    private void Awake()
    {
        textTransform = GetComponent<RectTransform>();
        textMeshPro =  GetComponent<TextMeshProUGUI>();
        startColor = textMeshPro.color;
    }

    private void Update()
    {
        textTransform.position += moveSpeed * Time.deltaTime;

        timeElapsed += Time.deltaTime;

        float newAlpha = startColor.a * (1 - (timeElapsed / timeToFade));

        if (timeElapsed < timeToFade)
        {
            textMeshPro.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
        }
        else
        {
            Destroy(gameObject);
        }

    }
}
