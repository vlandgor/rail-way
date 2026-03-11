using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.Rail
{
    public class RailLink
    {
        public int Id;
        public Vector3 WorldPosition;
        public bool IsJunction; 
        public bool IsDeadEnd;
        public List<RailNode> Nodes = new();
    }
}