using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float maxDistance = 2000f;

    private GameObject shooter;
    private Vector3 origin;

    private bool running = false;
    private bool isLocal = false;

    private string bulletID;

    private System.Guid guid = System.Guid.NewGuid();

    public string BulletID { get => bulletID; }

    public void SetSource(GameObject gameObject, Vector3 origin)
    {
        this.gameObject.name = guid.ToString();
        shooter = gameObject;
        this.origin = origin;

        if(shooter.GetComponent<Player>()?.ConnectionID == NetworkPlayerManager.ConnectionID)
        {
            isLocal = true;
        }

        running = true;
    }

    private void Update()
    {
        if (running)
        { 
            transform.position += transform.up * speed * Time.deltaTime;
            if (Vector3.Distance(transform.position, origin) >= maxDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.parent.name == shooter.name || collision.gameObject == gameObject)
        {
            return;
        }

        if (collision.tag == "Player" && isLocal)
        {
            HandleClientData.SendBulletHit(bulletID);
        }

        Destroy(gameObject);
    }
}