using System;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player instance;

    private Guid id;
    private string playerName;
    private int spriteID;
    private int health;
    private float posX;
    private float posY;
    private float rotation;
    private int kills;

    public Player(Guid id)
    {
        Id = id;
    }

    public Guid Id { get => id; set => id = value; }
    public int SpriteID { get => spriteID; set => spriteID = value; }
    public int Health { get => health; set => health = value; }
    public string Name { get => playerName; set => playerName = value; }
    public int Kills { get => kills; set => kills = value; }

    private void Awake()
    {
        instance = this;
        //Health = 100;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player") { return; }  // if we collide with a player do nothing just return
        if (collision.tag == "Bullet")
        {
            if (Id == NetPlayer.Id)
            {
                // This send the playerID and bulletID when the player got hit for comparing on serverside, if its valid the player will lose health
                Guid bulletID = collision.GetComponent<Projectile>().id;
                ClientTCP.PACKAGE_SendPlayerGotHit(Id, bulletID);
            }
        }

    }
}
