using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetPlayer : MonoBehaviour
{
    public static NetPlayer instance;
    public static int connectionID { get; protected set; }
    public static Vector3 position { get; protected set; }
    [SerializeField] private List<Sprite> sprites = new List<Sprite>();

    public static Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();
    public static Dictionary<int, GameObject> projectiles = new Dictionary<int, GameObject>();
    public static int onlinePlayerCount;
    public static GameObject bulletPrefab;
    public static GameObject playerPrefab;
    public static GameObject crossair;

    public static Text healthText;

    private void Awake()
    {
        instance = this;
        bulletPrefab = Resources.Load("Prefabs/Bullet") as GameObject;
        playerPrefab = Resources.Load("Prefabs/Player") as GameObject;
        healthText = GameObject.Find("Health").GetComponentInChildren<Text>();
        crossair = Instantiate(Resources.Load("Prefabs/Crossair") as GameObject);
    }

    private void FixedUpdate()
    {
        if (!NetworkManager.instance.isConnected) { return; }
        // If there is no connectionID set request a connectionID from the server
        if (connectionID <= 0) { ClientTCP.PACKAGE_RequestConnectionID(); }
        if (connectionID != 0 && players.Count < onlinePlayerCount) { ClientTCP.PACKAGE_RequestWorldPlayers(players[connectionID].GetComponent<Player>().SpriteID); }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        crossair.transform.position = mousePos;
    }

    public static void SetConnectionID(int id)
    {
        if (connectionID != 0) { return; }
        connectionID = id;
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
        go.GetComponent<Player>().ConnectionID = connectionID;

        players.Add(connectionID, go);
        players[connectionID].GetComponentInChildren<Player>().SpriteID = randomSprite;
        //TODO: Set the player name to the player class
        ClientTCP.PACKAGE_SendPlayerData(connectionID);
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
    public static void InstantiateNewProjectile(int connectionID)
    {
        int bulletID = GetPositiveHashCode();
        Transform parent = players[connectionID].transform; // who shot the projectile



        GameObject bullet = Instantiate(bulletPrefab);
        bullet.name = $"Bullet | {bulletID}";
        bullet.transform.rotation = parent.rotation;
        bullet.transform.position = parent.position + parent.up;

        Projectile projectileData = bullet.GetComponent<Projectile>();
        projectileData.bulletID = bulletID;
        projectileData.parent = parent;

        if (projectiles.ContainsKey(bulletID))
        {
            bulletID = GetPositiveHashCode();
        }

        projectiles.Add(bulletID, bullet);
        ClientTCP.PACKAGE_SendProjectile(bulletID);
    }

    public static void InstantiateNewProjectile(int connectionID, int bulletID)
    {
        Transform parent = players[connectionID].transform;

        GameObject bullet = Instantiate(bulletPrefab);
        bullet.name = $"Bullet | {bulletID}";
        bullet.transform.rotation = parent.rotation;
        bullet.transform.position = parent.position + parent.up;

        Projectile projectileData = bullet.GetComponent<Projectile>();
        projectileData.bulletID = bulletID;
        projectileData.parent = parent;

        projectiles.Add(bulletID, bullet);
    }

    public static void Destroy(GameObject gameObject)
    {
        Destroy(gameObject);
    }

}
