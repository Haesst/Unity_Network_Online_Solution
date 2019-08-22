using System.Collections.Generic;
using UnityEngine;

public class NetPlayer : MonoBehaviour
{
    public static NetPlayer instance;
    public static int connectionID { get; protected set; }
    public static Vector3 position { get; protected set; }
    [SerializeField] private List<Sprite> sprites = new List<Sprite>();
    static SpriteRenderer g_ship;

    public static Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

    private void Awake()
    {
        instance = this;
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

    public void InstantiateNewPlayer(int connectionID, int spriteID)
    {
        if (connectionID <= 0 | players.ContainsKey(connectionID)) { return; }

        int randomSprite = spriteID <= 0 ? Random.Range(0, 11) : spriteID;

        GameObject go = Instantiate(Resources.Load("Prefabs/Player")) as GameObject;
        go.name = $"Player | {connectionID}";
        go.GetComponentInChildren<SpriteRenderer>().sprite = sprites[randomSprite];

        players.Add(connectionID, go);
        players[connectionID].GetComponentInChildren<Player>().SpriteID = randomSprite;
        BackgroundMove.instance.Player = players[connectionID].transform;
        ClientTCP.PACKAGE_RequestWorldPlayers(randomSprite);
    }

    public void InstantiateNewPlayerAtPosition(int connectionID, float posX, float posY, float rotation, int sprite = 0)
    {
        if (connectionID <= 0 | players.ContainsKey(connectionID)) { return; }

        GameObject go = Instantiate(Resources.Load("Prefabs/Player")) as GameObject;
        go.transform.position = new Vector3(posX, posY, go.transform.position.z);
        go.transform.rotation = Quaternion.Euler(0, 0, rotation);
        go.GetComponent<Player>().ConnectionID = connectionID;
        go.GetComponentInChildren<SpriteRenderer>().sprite = sprites[sprite];
        go.name = $"Player | {connectionID}";

        players.Add(connectionID, go);
    }

}
