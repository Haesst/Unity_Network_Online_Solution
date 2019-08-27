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
    GameObject bullet;

    private void Awake()
    {
        rb = transform.GetComponent<Rigidbody2D>();
        projectileSpeed = 10f;
        maxTravelTime = 100f;
    }
    public Projectile(Transform parent, int bulletID)
    {
        this.bulletID = bulletID;
        Debug.Log(this.bulletID);
        startPosition = parent.position + parent.up;
        bullet = Instantiate(NetPlayer.bulletPrefab);
        bullet.name = $"Bullet | {bulletID}";
        bullet.transform.rotation = parent.rotation;
        bullet.transform.position = startPosition;
        NetPlayer.projectiles.Add(bulletID, bullet);
    }

    private void FixedUpdate()
    {
        timer++;
        MoveProjectile();

        if (timer >= maxTravelTime)
        {
            DestroyBullet(bulletID);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //TODO: Explosion effect
        int playerID = collision.gameObject.GetComponent<Player>().ConnectionID;
        ClientTCP.PACKAGE_SendProjectileHit(bulletID, playerID);
        DestroyBullet(bulletID);
    }

    private void DestroyBullet(int bulletID)
    {
        // bulletID is always 0 why?
        NetPlayer.projectiles.Remove(bulletID);
        Destroy(gameObject);
        Debug.Log($"Projectiles dictonary: {NetPlayer.projectiles.Count}");
    }

    private void MoveProjectile()
    {
        transform.position = transform.position + ((transform.up * Time.deltaTime) * projectileSpeed);
    }
}
