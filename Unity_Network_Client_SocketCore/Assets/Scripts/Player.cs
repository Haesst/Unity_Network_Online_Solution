using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player instance;

    private int connectionID;
    private string name;
    private int spriteID;
    private int health;
    private float posX;
    private float posY;
    private float rotation;

    private Transform closestEnemy;
    private GameObject enemyPointer;
    private RectTransform enemyPointerArrow;
    private Text enemyPointerText;
    private Image enemyPointerEnemySprite;

    public Player(int connectionID)
    {
        ConnectionID = connectionID;
    }

    public int ConnectionID { get => connectionID; set => connectionID = value; }
    public int SpriteID { get => spriteID; set => spriteID = value; }
    public int Health { get => health; set => health = value; }
    public string Name { get => name; set => name = value; }

    private void Awake()
    {
        instance = this;
        Health = 100;
        enemyPointer = GameObject.Find("Canvas/EnemyPointer");
        enemyPointerArrow = GameObject.Find("Canvas/EnemyPointer/Arrow").GetComponent<RectTransform>();
        enemyPointerText = GameObject.Find("Canvas/EnemyPointer/Text").GetComponent<Text>();
        enemyPointerEnemySprite = GameObject.Find("Canvas/EnemyPointer/EnemySprite").GetComponent<Image>();
        enemyPointer.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player") { return; }  // if we collide with a player do nothing just return
        if (collision.tag == "Bullet")
        {
            if (ConnectionID == NetPlayer.connectionID)
            {
                // This send the playerID and bulletID when the player got hit for comparing on serverside, if its valid the player will lose health
                int bulletID = collision.GetComponent<Projectile>().bulletID;
                ClientTCP.PACKAGE_SendPlayerGotHit(connectionID, bulletID);
            }
        }

    }

    private void FixedUpdate()
    {
        if (NetPlayer.players.Count > 1)
        {
            if (connectionID == NetPlayer.connectionID)
            {
                closestEnemy = GetClosestEnemy();
                float enemyDistance = Vector3.Distance(transform.position, closestEnemy.position);
                if (enemyDistance > 5f)
                {
                    enemyPointer.SetActive(true);

                    if ((int)enemyDistance > 100)
                        enemyPointerText.text = "+100m";
                    else
                        enemyPointerText.text = (int)enemyDistance + "m";

                    float angle = Mathf.Atan2(transform.position.y - closestEnemy.position.y, transform.position.x - closestEnemy.position.x) * Mathf.Rad2Deg;
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

        foreach (var enemy in NetPlayer.players)
        {
            if (enemy.Value.GetComponent<Player>().ConnectionID == connectionID) { continue; }

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
}
