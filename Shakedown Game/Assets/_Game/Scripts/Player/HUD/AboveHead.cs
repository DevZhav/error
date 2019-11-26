using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player.HUDElements
{
    public class AboveHead : MonoBehaviour
    {
        [Header("References")]
        private Networker Networker;  // The controller specific to this player
        private Transform PlayerCamera; // The player camerea transform that is specific to this player
        bool isUnderUI;                 // If this isn't true, then we still need to setup the UI

        [Header("UI References")]
        public TextMeshProUGUI NameTag;
        public EnergyBar HealthBar;
        private EnergyBarToolkit.FilledRendererUGUI FilledHealthBar;

        [Header("Color")]
        public Color TeamColor;
        public Color EnemyColor;
        Color color;

        [Header("Camera Raycasts")]
        public LayerMask CheckLayers;

        private void Start()
        {
            Invoke("Initialize", 1.0f);
        }

        bool runCode = false;
        private void Initialize()
        {
            Networker = transform.parent.GetComponent<Networker>();
            PlayerCamera = Networker.Instance.Controller.Camera.Cam.transform;
            FilledHealthBar = HealthBar.GetComponent<EnergyBarToolkit.FilledRendererUGUI>();

            NameTag.SetText(Networker.nv_Name.Value);
            transform.SetParent(Networker.Instance.Controller.HUD.NameTagParent, false);

            // If the player this object belongs to is on our team
            // Then set the color to our team's color, else set it to the enemy's color
            color = Networker.nv_Team.Value == Networker.Instance.nv_Team.Value ? TeamColor : EnemyColor;

            // Set the color of all the UI now
            NameTag.color = color;
            FilledHealthBar.spriteBarColor = color;
            FilledHealthBar.spritesBackground[0].color = new Color(color.r, color.g, color.b, 0.3f);

            runCode = true;
        }

        private void Update()
        {
            if (!runCode)
                return;

            if (Networker == null || Networker == Networker.Instance)
                Destroy(gameObject);

            Debug.Log("Can See Player: " + Networker.CanSeePlayer);

            // If they're not on our team
            // And we can see them
            if (Networker.nv_Team.Value != Networker.Instance.nv_Team.Value && Networker.CanSeePlayer)
            {
                // We cast a ray from the center of our screen
                // And if it hits a hitbox, then we know to display some more UI info
                RaycastHit hit;
                if (Physics.Raycast(PlayerCamera.position, PlayerCamera.forward, out hit, Mathf.Infinity, CheckLayers))
                {
                    if (hit.transform.gameObject.GetComponent<Hitbox>())
                    {
                        UpdateHealthBar();

                        // Enable the objects
                        HealthBar.gameObject.SetActive(true);
                        NameTag.gameObject.SetActive(true);
                    }
                    else
                    {
                        // Disable our objects by default
                        // So we can enable them later (or not if the conditions aren't met)
                        HealthBar.gameObject.SetActive(false);
                        NameTag.gameObject.SetActive(false);
                    }
                }
                else
                {
                    // Disable our objects by default
                    // So we can enable them later (or not if the conditions aren't met)
                    HealthBar.gameObject.SetActive(false);
                    NameTag.gameObject.SetActive(false);
                }
            }
            // If they're on our team
            // And we can see them
            else if (Networker.nv_Team.Value == Networker.Instance.nv_Team.Value && Networker.CanSeePlayer)
            {
                UpdateHealthBar();

                // Enable the objects
                HealthBar.gameObject.SetActive(Networker.nv_Health.Value < Networker.Controller.MaxHealth ? true : false);
                NameTag.gameObject.SetActive(true);
            }
            else
            {
                // Disable our objects by default
                // So we can enable them later (or not if the conditions aren't met)
                HealthBar.gameObject.SetActive(false);
                NameTag.gameObject.SetActive(false);
            }
        }

        private void UpdateHealthBar()
        {
            // Set the healthbar values
            HealthBar.SetValueCurrent(Networker.nv_Health.Value);
            HealthBar.SetValueMax(Networker.Controller.MaxHealth);

            // Set the position of our nametag in the canvas
            transform.position = Networker.Instance.Controller.Camera.Cam.WorldToScreenPoint(Networker.Controller.NameTagPosition.position);
        }
    }
}