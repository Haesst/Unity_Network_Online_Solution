using UnityEngine;

public class Projectile : MonoBehaviour
{
    Rigidbody2D rb;
    Vector3 startPosition;
    Quaternion projectileRotation;
    float projectileSpeed;
    float maxTravelTime;
    GameObject go;
    int timer;

    private void Awake()
    {
        rb = transform.GetComponent<Rigidbody2D>();
        projectileSpeed = 10f;
        maxTravelTime = 100f;
    }
    public Projectile(Transform parent)
    {
        startPosition = parent.position + parent.up;
        go = Instantiate(Resources.Load("Prefabs/Bullet")) as GameObject;
        go.transform.rotation = parent.rotation;
        go.transform.position = startPosition;
        
    }

    private void FixedUpdate()
    {
        transform.position = transform.position + ((transform.up * Time.deltaTime) * projectileSpeed);
        timer++;
        if (timer >= maxTravelTime)
        {
            Destroy(gameObject);
        }
    }
}
