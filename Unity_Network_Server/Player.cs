﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unity_Network_Server
{
    public class Player
    {
        //ADDME!
        //TODO: Store all players info here
        private string id;
        int health;
        private float posX;
        private float posY;
        private float rotation;

        private string lastBulletHit;

        public string ID { get => id; }
        public string LastBulletHit { get => lastBulletHit; set => lastBulletHit = value; }

        public Player(Guid guid)
        {
            id = guid.ToString();
        }
        public float PosX { get => posX; }
        public float PosY { get => posY; }
        public float Rotation { get => rotation; }

        public void SetPlayerPosition(float posX, float posY, float rotation)
        {
            this.posX = posX;
            this.posY = posY;
            this.rotation = rotation;
        }
    }

}
