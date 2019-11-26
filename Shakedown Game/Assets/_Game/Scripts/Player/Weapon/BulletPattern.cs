using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Weapons
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "Bullet Pattern", menuName = "Shakedown/Bullet Pattern", order = 1)]
    public class BulletPattern : ScriptableObject
    {
        public int Bullets;
        public Vector2[] Pattern;
    }
}