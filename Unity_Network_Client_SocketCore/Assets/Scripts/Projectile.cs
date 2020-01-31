﻿using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Rigidbody2D rb;
    CapsuleCollider2D capsuleCollider;
    float projectileSpeed;
    float maxTravelTime;
    int timer;

    public Transform parent;
    public int bulletID;

    private void Awake()
    {
        rb = transform.GetComponent<Rigidbody2D>();
        projectileSpeed = 10f;
        maxTravelTime = 100f;
    }
    private void FixedUpdate()
    {
        timer++;
        transform.position = transform.position + ((transform.up * Time.deltaTime) * projectileSpeed);
        if (timer >= maxTravelTime) { DestroyBullet(); }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Bullet") { return; }  // do nothing if we collide whit a other bullet for now, should we be able to destroy other bullets?
        if (collision.tag == "Player")
        {
            // This gets the playerID of the player that got hit, and the bulletID of the bullet that hit the player.
            // Then send it to the server for comparing, and also destroy the local bullet gameObject
            //TODO: Explosion effect
            Guid playerID = collision.GetComponent<Player>().Id;
            ClientTCP.PACKAGE_SendProjectileHit(bulletID, playerID);
            DestroyBullet();
        }

    }

    private void DestroyBullet()
    {
        NetPlayer.projectiles.Remove(bulletID);
        Destroy(gameObject);
    }
}
