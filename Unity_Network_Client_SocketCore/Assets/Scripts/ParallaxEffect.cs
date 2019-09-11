using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    public static ParallaxEffect instance;

    private Transform origin;
    private Camera mainCamera;
    private Transform player;
    private Vector3 originPos;

    private float lengthX;
    private float startPosX;
    private float lengthY;
    private float startPosY;
    public float parallaxEffect;

    public Transform Player { get => player; set => player = value; }

    private void Awake()
    {
        instance = this;
        origin = transform;
        mainCamera = Camera.main;

        startPosX = transform.position.x;
        startPosY = transform.position.y;
        lengthX = GetComponent<SpriteRenderer>().bounds.size.x;
        lengthY = GetComponent<SpriteRenderer>().bounds.size.y;

        //parallaxEffect = 0.8f;
    }

    private void FixedUpdate()
    {
        float tempX = (mainCamera.transform.position.x * (1 - parallaxEffect));
        float tempY = (mainCamera.transform.position.y * (1 - parallaxEffect));

        float distX = (mainCamera.transform.position.x * parallaxEffect);
        float distY = (mainCamera.transform.position.y * parallaxEffect);

        if (tempX > startPosX + lengthX)
        {
            startPosX += lengthX * 3f;
        }

        transform.position = new Vector3(startPosX + distX, startPosY + distY, transform.position.z);
        /*
        if (tempX > startPosX + lengthX) startPosX += lengthX;
        else if (tempX < startPosX - lengthX) startPosX -= lengthX;

        if (tempY > startPosY + lengthY) startPosY += lengthY;
        else if (tempY < startPosY - lengthY) startPosY -= lengthY;*/
    }

    private void UpdateCameraPosition()
    {
        mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, mainCamera.transform.position.z);
    }

}
