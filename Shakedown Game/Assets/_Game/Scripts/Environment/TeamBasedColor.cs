using UnityEngine;

namespace Enviro
{
    public class TeamBasedColor : MonoBehaviour
    {
        [Header("Instance")]
        public static TeamBasedColor Instance = null;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
        
        public void UpdateColors(int team)
        {
            Debug.Log("Set Team Color Renderers");
            PlayerMethods.SetTeamRenderers(FindObjectsOfType<MeshRenderer>(), team);
        }
    }
}