using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Player.HUDElements
{
    public class Statistics : MonoBehaviour
    {
        [Header("Health/Stamina")]
        public EnergyBar HealthBar;
        public EnergyBar StaminaBar;

        public void Initialize(short maxHealth, float maxStamina)
        {
            HealthBar.valueMax = maxHealth;
            StaminaBar.valueMax = (int)(maxStamina * 10);
        }

        public void UpdateScript(short health, float stamina)
        {
            HealthBar.valueCurrent = health;
            StaminaBar.valueCurrent = (int)(stamina * 10);
        }
    }
}