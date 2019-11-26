using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Player))]
public class PlayerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        Player p = (Player)target;

        if (Application.isPlaying)
        {
            if (GUILayout.Button("Damage Player"))
            {
                p.DoDamage(10);
            }
            if (GUILayout.Button("Heal Player"))
            {
                p.DoDamage(-10);
            }
            if (GUILayout.Button("Kill Player"))
            {
                p.DoDamage(255);
            }

            GUILayout.Space(20);
        }
        DrawDefaultInspector();
    }
}
