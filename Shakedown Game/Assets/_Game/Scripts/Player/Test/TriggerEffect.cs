using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEffect : MonoBehaviour
{
    [Header("Knockback")]
    public bool IsKnockback;
    public Vector3 KnockbackDirection;
    public float KnockbackSpeed;
    public float KnockbackLength;
    public bool KnockbackWallBounce;
    public float KnockbackSlamLength;

    [Header("Pushback")]
    public bool IsPushback;
    public Vector3 PushbackDirection;
    public float PushbackSpeed;
    public float PushbackLength;

    [Header("Flinch")]
    public bool IsFlinch;
    public float FlinchLength;

    [Header("Stun")]
    public bool IsStun;
    public float StunLength;

    private void OnTriggerEnter(Collider col)
    {
        if (col.GetComponent<Player.Controller>())
        {
            var serializer = col.GetComponent<Player.Controller>();

            if (IsKnockback)
                serializer.Motor.DoKnockback(transform.TransformDirection(KnockbackDirection), KnockbackSpeed, KnockbackLength, KnockbackWallBounce, KnockbackSlamLength);
            else if (IsPushback)
                serializer.Motor.DoPushback(transform.TransformDirection(PushbackDirection), PushbackSpeed, PushbackLength);
            else if (IsFlinch)
                serializer.Motor.DoFlinch(FlinchLength);
            else if (IsStun)
                serializer.Motor.DoStun(StunLength);
        }
    }
}
