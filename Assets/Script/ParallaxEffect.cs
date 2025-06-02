using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    public float parallaxFactor = 0.5f;
    private Transform cam;
    private Vector3 previousCamPos;

    void Start()
    {
        cam = Camera.main.transform;
        previousCamPos = cam.position;
    }

    void Update()
    {
        Vector3 deltaMovement = cam.position - previousCamPos;
        transform.position += deltaMovement * parallaxFactor;
        previousCamPos = cam.position;
    }
}