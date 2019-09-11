using UnityEngine;

public class Player : MonoBehaviour
{

    private string connectionID;
    int health;
    private float posX;
    private float posY;
    private float rotation;
    private float speed;

    public float Speed { get => speed; set => speed = value; }
    public string ConnectionID { get => connectionID; set => connectionID = value; }

    public Player(string connectionID)
    {
        this.ConnectionID = connectionID;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.tag == "Bullet")
        {
            Debug.Log($"Got hit by: {collision.collider.name}");
            Destroy(collision.collider.gameObject);
        }
        else
        {
            Debug.Log(collision.collider.tag);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Bullet")
        {
            HandleClientData.SendPlayerHit(collision.GetComponent<Bullet>().BulletID);
        }
    }
}
