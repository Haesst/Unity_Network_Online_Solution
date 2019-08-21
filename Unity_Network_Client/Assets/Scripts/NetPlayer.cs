using System.Collections.Generic;
using UnityEngine;

public class NetPlayer : MonoBehaviour
{
    public static NetPlayer instance;
    public static int connectionID { get; protected set; }
    public static Vector3 position { get; protected set; }
    [SerializeField] private List<Sprite> sprites = new List<Sprite>();
    static SpriteRenderer g_ship;

    public static Dictionary<int, GameObject> Players = new Dictionary<int, GameObject>();

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

    public void InstantiateNewPlayer(int connectionID)
    {
        if (connectionID <= 0 || Players.ContainsKey(connectionID)) { return; }

        GameObject go = Instantiate(Resources.Load("Prefabs/Player", typeof(GameObject))) as GameObject;
        go.GetComponentInChildren<SpriteRenderer>().sprite = sprites[connectionID - 1];
        Players.Add(connectionID, go);
    }

    public void InstantiateNewPlayer(int connectionID, float posX, float posY, float rotation)
    {
        if (connectionID <= 0 || Players.ContainsKey(connectionID)) { return; }
        Debug.Log($"InstantiateNewPlayer::Assigned connectionID: {connectionID}");

        GameObject go = Instantiate(Resources.Load("Prefabs/Player", typeof(GameObject))) as GameObject;
        go.transform.position = new Vector3(posX, posY, go.transform.position.z);
        go.transform.rotation = Quaternion.Euler(0, 0, rotation);
        go.GetComponent<Player>().ConnectionID = connectionID;
        go.GetComponentInChildren<SpriteRenderer>().sprite = sprites[connectionID - 1];
        go.name = $"Player | {connectionID}";
        Players.Add(connectionID, go);
    }

}
