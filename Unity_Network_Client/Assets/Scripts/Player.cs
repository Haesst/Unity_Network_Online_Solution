using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;

    public int connectionID;
    int health;
    private float posX;
    private float posY;
    private float rotation;

    public Player(int connectionID)
    {
        this.connectionID = connectionID;
    }

    private void Awake()
    {
        instance = this;
    }

    
}
