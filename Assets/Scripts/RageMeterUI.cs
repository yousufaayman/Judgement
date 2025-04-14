using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RageMeterUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image rageFillBar;
    [SerializeField] private Image rageBorder;
    [SerializeField] private TextMeshProUGUI rageText;

    [Header("Visual Settings")]
    [SerializeField] private Color lowRageColor = new Color(0.2f, 0.8f, 0.2f); 
    [SerializeField] private Color mediumRageColor = new Color(0.9f, 0.9f, 0.2f);
    [SerializeField] private Color highRageColor = new Color(0.9f, 0.2f, 0.2f); 
    [SerializeField] private bool showPercentageText = true;
    [SerializeField] private float pulseRate = 2f;
    [SerializeField] private float pulseAmount = 0.2f;

    [Header("Animation Settings")]
    [SerializeField] private float smoothTime = 0.2f;

    private float currentDisplayedRage = 0f;
    private float rageVelocity = 0f;
    private Vector3 originalScale;
    private bool isHighRage = false;

    private void Awake()
    {
        originalScale = transform.localScale;

        if (rageFillBar != null)
            rageFillBar.fillAmount = 0f;

        if (rageText != null)
            rageText.text = showPercentageText ? "0%" : "";
    }

    private void Start()
    {
        if (GlobalRageManager.Instance != null)
        {
            GlobalRageManager.Instance.onRageChanged.AddListener(OnRageChanged);

            OnRageChanged(GlobalRageManager.Instance.NormalizedRage);
        }
        else
        {
            Debug.LogWarning("GlobalRageManager not found. RageMeterUI will not function.");
        }
    }

    private void OnDestroy()
    {
        if (GlobalRageManager.Instance != null)
        {
            GlobalRageManager.Instance.onRageChanged.RemoveListener(OnRageChanged);
        }
    }

    private void Update()
    {
        if (rageFillBar != null)
        {
            RectTransform fillRect = rageFillBar.rectTransform;

            RectTransform parentRect = transform as RectTransform;
            float maxWidth = parentRect.rect.width;

            float targetWidth = maxWidth * currentDisplayedRage;
            float currentWidth = Mathf.SmoothDamp(
                fillRect.sizeDelta.x,
                targetWidth,
                ref rageVelocity,
                smoothTime
            );

            fillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentWidth);
        }

        if (isHighRage)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseRate * Mathf.PI * 2) * pulseAmount;
            transform.localScale = originalScale * pulse;
        }
    }

    public void OnRageChanged(float normalizedRage)
    {
        currentDisplayedRage = normalizedRage;

        if (showPercentageText && rageText != null)
        {
            rageText.text = Mathf.RoundToInt(normalizedRage * 100) + "%";
        }

        if (rageFillBar != null)
        {
            if (normalizedRage < 0.33f)
            {
                rageFillBar.color = lowRageColor;
                isHighRage = false;
                transform.localScale = originalScale;       
            }
            else if (normalizedRage < 0.66f)
            {
                rageFillBar.color = mediumRageColor;
                isHighRage = false;
                transform.localScale = originalScale;       
            }
            else
            {
                rageFillBar.color = highRageColor;
                isHighRage = true;   
            }
        }
    }
}