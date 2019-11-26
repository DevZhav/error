using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player.HUDElements
{
    public class KillFeed : MonoBehaviour
    {
        private List<GameObject> MessageLabels = new List<GameObject>();
        private int MaxMessages = 6;

        [Header("References")]
        public ScrollRect ScrollView;
        public Transform MessageContentArea;
        public GameObject MessagePrefab;

        [Header("Colors")]
        public Color Neutral;
        public Color OurTeam;
        public Color EnemyTeam;

        [Header("Icons")]
        public Sprite SuicideIcon;

        public void Message(string killerName, byte killerTeam, string victimName, byte victimTeam, byte weaponID)
        {
            // Check if we're over our max messages
            // If we are, remove the first message
            if (MessageLabels.Count >= MaxMessages)
                MessageLabels.RemoveAt(0);

            // Create an empty gameobject for us to use as a variable
            KillFeedMessage label = null;

            // Instantiate it as a gameobject first, then get the KillFeedMessage component attached
            label = (Instantiate(MessagePrefab, MessageContentArea) as GameObject).GetComponent<KillFeedMessage>();

            // Set it as the last index in the transform
            // So that it's shown to the bottom of the UI
            label.transform.SetAsLastSibling();

            // Add the gameobject to our list
            MessageLabels.Add(label.gameObject);

            // And now set the info
            string killer = PlayerMethods.GetColoredName(killerName, killerTeam);
            string victim = PlayerMethods.GetColoredName(victimName, victimTeam);
            Sprite icon = null;
            if (killerName != victimName)
            {
                icon = Networker.Instance.Controller.GetWeaponByID(weaponID).ActiveSkin.Icon;
                label.Initialize(killer, victim, icon);
            }
            else
            {
                icon = SuicideIcon;
                label.Initialize("", victim, icon);
            }

            StartCoroutine(ScrollToBottom());
        }

        private IEnumerator ScrollToBottom()
        {
            yield return new WaitForEndOfFrame();
            ScrollView.verticalNormalizedPosition = 0;
        }
    }
}