using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    private Camera mainCamera;

    private float lengthX;
    private float startPosX;
    private float lengthY;
    private float startPosY;
    public float parallaxEffect;

    private void Awake()
    {
        mainCamera = Camera.main;
        startPosX = transform.position.x;
        startPosY = transform.position.y;
        lengthX = GetComponent<SpriteRenderer>().bounds.size.x;
        lengthY = GetComponent<SpriteRenderer>().bounds.size.y;
    }

    private void FixedUpdate()
    {
        float tempX = (mainCamera.transform.position.x * (1 - parallaxEffect));
        float tempY = (mainCamera.transform.position.y * (1 - parallaxEffect));

        float distX = (mainCamera.transform.position.x * parallaxEffect);
        float distY = (mainCamera.transform.position.y * parallaxEffect);

        if (tempX > startPosX + lengthX) { startPosX += lengthX * 3f; }
        if (tempX < startPosX - lengthX) { startPosX -= lengthX * 3f; }
        if (tempY > startPosY + lengthY) { startPosY += lengthY * 3f; }
        if (tempY < startPosY - lengthY) { startPosY -= lengthY * 3f; }

        transform.position = new Vector3(startPosX + distX, startPosY + distY, transform.position.z);
    }

}
