﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unity_Network_Server
{
    public class Player
    {
        //TODO: Store all players info here
        int health;
        private float posX;
        private float posY;
        private float rotation;

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
