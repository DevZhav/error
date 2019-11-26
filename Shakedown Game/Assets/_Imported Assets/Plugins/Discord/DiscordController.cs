using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using MLAPI;

public class DiscordController : MonoBehaviour
{
    public long ClientID = 522217682422988851;
    private Discord.Discord discord;
    private Discord.ActivityManager activityManager;

    private void Start()
    {
        // Create a new discord setup
        discord = new Discord.Discord(ClientID, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);
        // Set the activity manager
        activityManager = discord.GetActivityManager();
        // Invoke activity updates
        InvokeRepeating("UpdateActivity", 0.0f, 5.0f);
    }

    private void Update()
    {
        discord.RunCallbacks();
    }

    private void UpdateActivity()
    {
        var activity = new Discord.Activity();
        activity.State = "In " + GetSceneName();
        activity.Assets.LargeImage = GetSceneName(true);
        activity.Assets.LargeText = GetSceneName();

        if (NetworkingManager.Singleton.IsClient)
        {
            activity.Details = string.Format("{0} ({1})", GetSceneName(), Enum.GetName(typeof(Game.Game.GameModes), Game.Game.Instance.Mode));
            activity.State = "In Match";

            if (Player.Networker.Instance != null)
            {
                Player.Networker a = Player.Networker.Instance;
                activity.Assets.SmallImage = a.nv_Team.Value == 2 ? "beta" : "alpha";
                activity.Assets.SmallText = a.nv_Team.Value == 2 ? "Team Beta" : "Team Alpha";
            }

            activity.Party.Size.CurrentSize = NetworkingManager.Singleton.ConnectedClientsList.Count;
            activity.Party.Size.MaxSize = 12;//DB_API.Match.PlayerLimit
        }

        activityManager.UpdateActivity(activity, callback => { Debug.Log("Discord: " + callback); });
    }

    private string GetSceneName(bool asset = false)
    {
        if (asset)
            return SceneManager.GetActiveScene().name.Replace(" ", "_").ToLower();
        else
            return SceneManager.GetActiveScene().name;
    }
}
