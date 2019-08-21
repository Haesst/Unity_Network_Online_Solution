using System;
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
        int connectionID;
        int spriteID;
        int health;
        private float posX;
        private float posY;
        private float rotation;

        public Player(int connectionID)
        {
            this.ConnectionID = connectionID;
        }
        public float PosX { get => posX; }
        public float PosY { get => posY; }
        public float Rotation { get => rotation; }
        public int ConnectionID { get => connectionID; set => connectionID = value; }
        public int SpriteID { get => spriteID; set => spriteID = value; }

        public void SetPlayerPosition(float posX, float posY, float rotation)
        {
            this.posX = posX;
            this.posY = posY;
            this.rotation = rotation;
        }
    }

}
