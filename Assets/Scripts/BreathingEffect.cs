using UnityEngine;

public class BreathingEffect : MonoBehaviour
{
    public float speed = 1.0f;
    public float minScale = 0.9f;
    public float maxScale = 1.1f;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        float breathValue = Mathf.Sin(Time.time * speed) * 0.5f + 0.5f;

        float currentScale = Mathf.Lerp(minScale, maxScale, breathValue);

        transform.localScale = originalScale * currentScale;
    }
}