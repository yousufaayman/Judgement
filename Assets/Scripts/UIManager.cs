using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    public GameObject damageTextPrefab;
    public GameObject healthTextPrefab;
    public Canvas gameCanvas;

    private void Awake()
    {
        gameCanvas = FindObjectOfType<Canvas>();

    }

    private void OnEnable()
    {
        CharachterEvents.charachterDamaged+= CharachterTookDamage;
        CharachterEvents.charachterHealed+=CharachterHealed;
    }

    private void OnDisable()
    {
        CharachterEvents.charachterDamaged-=CharachterTookDamage;
        CharachterEvents.charachterHealed-=CharachterHealed;
    }

    public void CharachterTookDamage(GameObject charachter, int damageRecieved)
    {
        Vector3 spawnPosition = Camera.main.WorldToScreenPoint(charachter.transform.position);

        TMP_Text tmpText= Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity, gameCanvas.transform).GetComponent<TMP_Text>();

        tmpText.text = damageRecieved.ToString();
    }

    public void CharachterHealed(GameObject charachter, int healthRestored) {

        Vector3 spawnPosition = Camera.main.WorldToScreenPoint(charachter.transform.position);

        TMP_Text tmpText = Instantiate(healthTextPrefab, spawnPosition, Quaternion.identity, gameCanvas.transform).GetComponent<TMP_Text>();

        tmpText.text = healthRestored.ToString();
    }
}
