using System;
using UnityEngine;

namespace DataModel
{
    [Serializable]
    public class ConstraintsModel
    {
        [SerializeField]
        public string tile;
    
        [SerializeField]
        public int minCount;
        
        [SerializeField]
        public bool asixX;
        
        [SerializeField]
        public bool asixY;
    }
}
