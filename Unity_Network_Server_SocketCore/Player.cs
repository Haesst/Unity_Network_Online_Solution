﻿namespace Unity_Network_Server_SocketCore
{
    public class Player
    {
        //TODO: Store all players info here
        private int connectionID;
        private int spriteID;
        private int health;
        private float posX;
        private float posY;
        private float rotation;
        private int bulletHitId;
        private bool isAlive;

        public Player(int connectionID)
        {
            ConnectionID = connectionID;
            Health = 100;
            IsAlive = true;
        }
        public float PosX { get => posX; }
        public float PosY { get => posY; }
        public float Rotation { get => rotation; }
        public int ConnectionID { get => connectionID; set => connectionID = value; }
        public int SpriteID { get => spriteID; set => spriteID = value; }
        public int BulletHitId { get => bulletHitId; set => bulletHitId = value; }
        public int Health { get => health; set => health = value; }
        public bool IsAlive { get => isAlive; set => isAlive = value; }

        public void SetPlayerPosition(float posX, float posY, float rotation)
        {
            this.posX = posX;
            this.posY = posY;
            this.rotation = rotation;
        }
    }
}