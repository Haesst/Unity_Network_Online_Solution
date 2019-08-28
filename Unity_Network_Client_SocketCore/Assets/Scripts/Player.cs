using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;

    private int connectionID;
    private int spriteID;
    private int health;
    private float posX;
    private float posY;
    private float rotation;

    public Player(int connectionID)
    {
        ConnectionID = connectionID;
    }

    public int ConnectionID { get => connectionID; set => connectionID = value; }
    public int SpriteID { get => spriteID; set => spriteID = value; }
    public int Health { get => health; set => health = value; }

    private void Awake()
    {
        instance = this;
        Health = 100;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player") { return; }  // if we collide with a player do nothing just return
        if (collision.tag == "Bullet")
        {
            if (ConnectionID == NetPlayer.connectionID)
            {
                // This send the playerID and bulletID when the player got hit for comparing on serverside, if its valid the player will lose health
                int bulletID = collision.GetComponent<Projectile>().bulletID;
                ClientTCP.PACKAGE_SendPlayerGotHit(connectionID, bulletID);
            }
        }
        
    }

}
