using UnityEngine;

namespace Player
{
    public class Hitbox : MonoBehaviour
    {
        public enum HitboxType
        {
            Head = 0,
            Body = 1,
            Arms = 2,
            Legs = 3,

            FrontMelee = 4,
            BackMelee = 5,

            Shield = 6
        }
        public HitboxType Type;

        public Player.Controller Controller;
    }
}