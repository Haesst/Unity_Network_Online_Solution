using System.Collections.Generic;
using UnityEngine;

public class NetPlayer : MonoBehaviour
{
    public static NetPlayer instance;
    public static int connectionID { get; protected set; }
    public static Vector3 position { get; protected set; }
    [SerializeField] private Sprite[] sprites;
    static SpriteRenderer g_ship;

    public static Dictionary<int, GameObject> Players = new Dictionary<int, GameObject>();

    private void Awake()
    {
        instance = this;
        //sprites = Resources.LoadAll("Sprites/ships") as Sprite[];
        //Debug.Log(sprites.Length);
        //g_ship = GameObject.Find("Player/Ship").GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        // If there is no connectionID set request a connectionID from the server
        if (connectionID <= 0) { ClientTCP.PACKAGE_RequestConnectionID(); }

    }
    public static void SetConnectionID(int id)
    {
        if (connectionID != 0) { return; }
        connectionID = id;
        //g_ship.sprite = sprites[connectionID];

    }

    public void InstantiateNewPlayer(int connectionID)
    {
        GameObject go = Instantiate(Resources.Load("Prefabs/Player", typeof(GameObject))) as GameObject;
        Players.Add(connectionID, go);
    }

}
