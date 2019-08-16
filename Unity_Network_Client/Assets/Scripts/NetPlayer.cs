using UnityEngine;

public class NetPlayer : MonoBehaviour
{

    public static int connectionID { get; protected set; }
    public static Vector3 position { get; protected set; }
    [SerializeField] private Sprite[] sprites;
    static SpriteRenderer g_ship;

    private void Awake()
    {
        //sprites = Resources.LoadAll("Sprites/ships") as Sprite[];
        //Debug.Log(sprites.Length);
        g_ship = GameObject.Find("Player/Ship").GetComponent<SpriteRenderer>();
    }
    public static void SetConnectionID(int id)
    {
        if (connectionID != 0) { return; }
        connectionID = id;
        //g_ship.sprite = sprites[connectionID];

    }
}
