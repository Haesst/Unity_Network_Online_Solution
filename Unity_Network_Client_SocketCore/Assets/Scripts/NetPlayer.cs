using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetPlayer : MonoBehaviour
{
    public static NetPlayer instance;
    public static Guid Id { get; protected set; }
    public static Vector3 position { get; protected set; }
    [SerializeField] private static List<Sprite> sprites = new List<Sprite>();

    public static Dictionary<Guid, GameObject> players = new Dictionary<Guid, GameObject>();
    public static Dictionary<int, GameObject> projectiles = new Dictionary<int, GameObject>();
    public static int onlinePlayerCount;
    public static GameObject bulletPrefab;
    public static GameObject playerPrefab;
    public static GameObject crossair;

    public static Text healthText;

    private Transform closestEnemy;
    private GameObject enemyPointer;
    private RectTransform enemyPointerArrow;
    private Text enemyPointerText;
    private Image enemyPointerEnemySprite;

    private void Awake()
    {
        instance = this;
        #region Setup Prefabs
        bulletPrefab = Resources.Load("Prefabs/Bullet") as GameObject;
        playerPrefab = Resources.Load("Prefabs/Player") as GameObject;
        healthText = GameObject.Find("Health").GetComponentInChildren<Text>();
        crossair = Instantiate(Resources.Load("Prefabs/Crossair") as GameObject);
        #endregion
        #region Setup Player Radar
        enemyPointer = GameObject.Find("Canvas/EnemyPointer");
        enemyPointerArrow = GameObject.Find("Canvas/EnemyPointer/Arrow").GetComponent<RectTransform>();
        enemyPointerText = GameObject.Find("Canvas/EnemyPointer/Text").GetComponent<Text>();
        enemyPointerEnemySprite = GameObject.Find("Canvas/EnemyPointer/EnemySprite").GetComponent<Image>();
        enemyPointer.SetActive(false);
        #endregion
    }

    private void FixedUpdate()
    {
        if (!NetworkManager.isConnected) { return; }
        // If there is no connectionID set request a connectionID from the server
        //if (connectionID <= 0) { ClientTCP.PACKAGE_RequestConnectionID(); }
        if (Id != null && players.Count < onlinePlayerCount) { ClientTCP.PACKAGE_RequestWorldPlayers(); }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        crossair.transform.position = mousePos;

        PlayerRadar();
    }

    private void PlayerRadar()
    {
        if (players.Count > 1)
        {
            GameObject player = players[Id];
            if (player.GetComponent<Player>().Id == Id)
            {
                closestEnemy = GetClosestEnemy();
                float enemyDistance = Vector3.Distance(player.transform.position, closestEnemy.position);
                if (enemyDistance > 5f)
                {
                    enemyPointer.SetActive(true);

                    if ((int)enemyDistance > 100)
                        enemyPointerText.text = "+100m";
                    else
                        enemyPointerText.text = (int)enemyDistance + "m";

                    float angle = Mathf.Atan2(player.transform.position.y - closestEnemy.position.y, player.transform.position.x - closestEnemy.position.x) * Mathf.Rad2Deg;
                    enemyPointerArrow.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 45f));
                    enemyPointerEnemySprite.sprite = closestEnemy.GetComponentInChildren<SpriteRenderer>().sprite;
                }
                else
                    enemyPointer.SetActive(false);
            }
        }
    }
    private Transform GetClosestEnemy()
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 playerPosition = transform.position;

        foreach (var enemy in players)
        {
            if (enemy.Value.GetComponent<Player>().Id == Id) { continue; }

            Vector3 directionToEnemy = enemy.Value.transform.position - playerPosition;
            float dSqrToTarget = directionToEnemy.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = enemy.Value.transform;
            }
        }

        return bestTarget;
    }

    public static void SetConnectionID(Guid id)
    {
        if (Id != null) { return; }
        Id = id;
    }

    public static int GetPositiveHashCode()
    {
        System.Object obj = new System.Object();
        int num = obj.GetHashCode();
        num = (num < 0) ? -num : num;
        return num;
    }

    public static void InstantiateNewPlayer(Guid id, int spriteID = -1, string name = null, float posX = 0, float posY = 0, float rotation = 0)
    {
        if (id != null | players.ContainsKey(Id)) { return; }

        if (spriteID < 0)
        {
            spriteID = UnityEngine.Random.Range(0, sprites.Count);
        }

        if (name == null)
        {
            name = NetworkManager.instance.selectName.transform.GetChild(0).GetChild(2).GetComponent<Text>().text;
        }

        GameObject go = Instantiate(playerPrefab);
        go.name = $"Player: {name} | {id}";
        go.transform.position = new Vector3(posX, posY, go.transform.position.z);
        go.transform.rotation = Quaternion.Euler(0, 0, rotation);
        go.GetComponentInChildren<SpriteRenderer>().sprite = sprites[spriteID];

        Player player = go.GetComponent<Player>();
        player.Id = id;
        player.SpriteID = spriteID;
        player.Name = name;

        players.Add(id, go);

        if (id == NetPlayer.Id)
        {
            PlayerInput.instance.id = id;
            ClientTCP.PACKAGE_SendPlayerData(id);
        }
    }

    public void InstantiateNewPlayerAtPosition(Guid id, float posX, float posY, float rotation, string name, int sprite = 0)
    {
        if (id != null | players.ContainsKey(id)) { return; }

        GameObject go = Instantiate(playerPrefab);
        go.name = $"Player: {name} | {id}";
        go.transform.position = new Vector3(posX, posY, go.transform.position.z);
        go.transform.rotation = Quaternion.Euler(0, 0, rotation);
        go.GetComponentInChildren<SpriteRenderer>().sprite = sprites[sprite];

        Player player = go.GetComponent<Player>();
        player.Id = id;
        player.Name = name;

        players.Add(id, go);
    }
    public static void InstantiateNewProjectile(Guid id)
    {
        int bulletID = GetPositiveHashCode();
        Transform parent = players[id].transform; // who shot the projectile

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

    public static void InstantiateNewProjectile(Guid id, int bulletID)
    {
        Transform parent = players[id].transform;

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
