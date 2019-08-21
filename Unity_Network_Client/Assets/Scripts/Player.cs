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

    public Player(int connectionID)
    {
        this.ConnectionID = connectionID;
    }

    public int ConnectionID { get => connectionID; set => connectionID = value; }
    public int SpriteID { get => spriteID; set => spriteID = value; }

    private void Awake()
    {
        instance = this;
    }

    
}
