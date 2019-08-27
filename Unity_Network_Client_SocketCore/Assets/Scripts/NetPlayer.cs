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
    public static Dictionary<int, GameObject> projectiles = new Dictionary<int, GameObject>();
    public static int onlinePlayerCount;
    public static GameObject bulletPrefab;
    public static GameObject playerPrefab;

    private void Awake()
    {
        instance = this;
        bulletPrefab = Resources.Load("Prefabs/Bullet") as GameObject;
        playerPrefab = Resources.Load("Prefabs/Player") as GameObject;
        //Debug.Log(sprites.Length);
        //g_ship = GameObject.Find("Player/Ship").GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        // If there is no connectionID set request a connectionID from the server
        if (connectionID <= 0) { ClientTCP.PACKAGE_RequestConnectionID(); }
        if (connectionID != 0 && players.Count < onlinePlayerCount) { ClientTCP.PACKAGE_RequestWorldPlayers(players[connectionID].GetComponent<Player>().SpriteID); }

    }
    public static void SetConnectionID(int id)
    {
        if (connectionID != 0) { return; }
        connectionID = id;
        //g_ship.sprite = sprites[connectionID];

    }

    public static int GetPositiveHashCode()
    {
        System.Object obj = new System.Object();
        int num = obj.GetHashCode();
        num = (num < 0) ? -num : num;
        return num;
    }

    public void InstantiateNewPlayer(int connectionID, int spriteID)
    {
        if (connectionID <= 0 | players.ContainsKey(connectionID)) { return; }

        int randomSprite = spriteID <= 0 ? Random.Range(0, 12) : spriteID;

        GameObject go = Instantiate(playerPrefab);
        go.name = $"Player | {connectionID}";
        go.GetComponentInChildren<SpriteRenderer>().sprite = sprites[randomSprite];

        players.Add(connectionID, go);
        players[connectionID].GetComponentInChildren<Player>().SpriteID = randomSprite;
        ClientTCP.PACKAGE_RequestWorldPlayers(randomSprite);
    }

    public void InstantiateNewPlayerAtPosition(int connectionID, float posX, float posY, float rotation, int sprite = 0)
    {
        if (connectionID <= 0 | players.ContainsKey(connectionID)) { return; }

        GameObject go = Instantiate(playerPrefab);
        go.transform.position = new Vector3(posX, posY, go.transform.position.z);
        go.transform.rotation = Quaternion.Euler(0, 0, rotation);
        go.GetComponent<Player>().ConnectionID = connectionID;
        go.GetComponentInChildren<SpriteRenderer>().sprite = sprites[sprite];
        go.name = $"Player | {connectionID}";

        players.Add(connectionID, go);
    }

    public static void Destroy(GameObject gameObject)
    {
        Destroy(gameObject);
    }

}
