using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;

    private int connectionID;
    private int spriteID;
    int health;
    private float posX;
    private float posY;
    private float rotation;
    private GameObject bullet;

    public Player(int connectionID)
    {
        this.ConnectionID = connectionID;
    }

    public int ConnectionID { get => connectionID; set => connectionID = value; }
    public int SpriteID { get => spriteID; set => spriteID = value; }
    public GameObject Bullet { get => bullet; set => bullet = value; }

    private void Awake()
    {
        instance = this;
        Bullet = Instantiate(Resources.Load("Prefabs/Bullet")) as GameObject;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log(collision.gameObject.name);
    }

}
