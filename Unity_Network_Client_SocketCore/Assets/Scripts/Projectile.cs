using UnityEngine;

public class Projectile : MonoBehaviour
{
    Rigidbody2D rb;
    CapsuleCollider2D capsuleCollider;
    Vector3 startPosition;
    Quaternion projectileRotation;
    float projectileSpeed;
    float maxTravelTime;
    int timer;
    int bulletID;

    private void Awake()
    {
        rb = transform.GetComponent<Rigidbody2D>();
        projectileSpeed = 10f;
        maxTravelTime = 100f;
    }
    public Projectile(Transform parent, int bulletID)
    {
        this.bulletID = bulletID;
        startPosition = parent.position + parent.up;
        GameObject bullet = Instantiate(NetPlayer.bulletPrefab);
        bullet.name = $"Bullet | {bulletID}";
        bullet.transform.rotation = parent.rotation;
        bullet.transform.position = startPosition;
        NetPlayer.projectiles.Add(bulletID, gameObject);
    }

    private void FixedUpdate()
    {
        timer++;
        MoveProjectile();

        if (timer >= maxTravelTime)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //TODO: Explosion effect
        int playerID = collision.gameObject.GetComponent<Player>().ConnectionID;
        ClientTCP.PACKAGE_SendProjectileHit(bulletID, playerID);
        Destroy(gameObject);
    }

    private void MoveProjectile()
    {
        transform.position = transform.position + ((transform.up * Time.deltaTime) * projectileSpeed);
    }
}
