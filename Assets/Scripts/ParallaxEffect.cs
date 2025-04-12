using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    public Camera cam;
    public Transform followTarget;

    Vector2 startingPosition;
    float startingZ;
    float distanceFromSubject;

    Vector2 camMoveSinceStart => (Vector2)cam.transform.position - startingPosition;
    float zDistanceFromTarget => transform.position.z - followTarget.transform.position.z;
    float parallaxFactor => Mathf.Abs(zDistanceFromTarget) / clippingPlane;
    float clippingPlane => (cam.transform.position.z + (distanceFromSubject > 0 ? cam.farClipPlane : cam.nearClipPlane));

    // Start is called before the first frame update
    void Start()
    {
        startingPosition = transform.position;  // Fixed typo in variable name
        startingZ = transform.position.z;
        distanceFromSubject = transform.position.z - followTarget.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 newPosition = startingPosition + camMoveSinceStart * parallaxFactor;  // Use = instead of +
        transform.position = new Vector3(newPosition.x, newPosition.y, startingZ);  // Added missing semicolon
    }
}