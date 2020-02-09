using System;

namespace Unity_Network_Server_SocketCore
{
    public class Player
    {
        //TODO: Store all players info here
        private Guid id;
        private int spriteID;
        private int health;
        private int maxHealth = 10;
        private float posX;
        private float posY;
        private float rotation;
        private Guid bulletHitId;
        private Guid bulletOwnerId;
        private bool isAlive;
        private string name;
        private int kills;


        public Player(Guid id)
        {
            Id = id;
            ResetPlayerData();
        }
        public float PosX { get => posX; }
        public float PosY { get => posY; }
        public float Rotation { get => rotation; }
        public Guid Id { get => id; protected set => id = value; }
        public int SpriteID { get => spriteID; set => spriteID = value; }
        public Guid BulletHitId { get => bulletHitId; set => bulletHitId = value; }
        public int Health { get => health; set => health = value; }
        public string Name { get => name; set => name = value; }
        public int Kills { get => kills; set => kills = value; }
        public Guid BulletOwnerId { get => bulletOwnerId; set => bulletOwnerId = value; }

        public void SetPlayerPosition(float posX, float posY, float rotation)
        {
            this.posX = posX;
            this.posY = posY;
            this.rotation = rotation;
        }

        public void ResetPlayerData()
        {
            health = maxHealth;
            posX = 0;
            posY = 0;
            rotation = 0;
            kills = 0;
        }
    }
}