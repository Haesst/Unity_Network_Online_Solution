﻿using System;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player instance;

    private Guid id;
    private string name;
    private int spriteID;
    private int health;
    private float posX;
    private float posY;
    private float rotation;

    

    public Player(Guid id)
    {
        Id = id;
    }

    public Guid Id { get => id; set => id = value; }
    public int SpriteID { get => spriteID; set => spriteID = value; }
    public int Health { get => health; set => health = value; }
    public string Name { get => name; set => name = value; }

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
                int bulletID = collision.GetComponent<Projectile>().bulletID;
                ClientTCP.PACKAGE_SendPlayerGotHit(Id, bulletID);
            }
        }

    }
}
