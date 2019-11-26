using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameMath
{
    /// <summary>
    /// Slowly tapers the currentValue from maximumLength to the toValue using the speed
    /// </summary>
    /// <param name="currentValue">The value we want to taper</param>
    /// <param name="toValue">The end value</param>
    /// <param name="timerValue">The timer value we use to taper</param>
    /// <param name="startPercent">The point in which we should start to taper | example: at 0.3f we'll start to taper (30%)</param>
    /// <param name="maximumLength">The starting value</param>
    /// <param name="speed">The speed we'll taper by</param>
    /// <returns></returns>
    public static float TimerTaper(float currentValue, float toValue, float timerValue, float startPercent, float maximumLength, float speed)
    {
        if (timerValue <= maximumLength * startPercent)
        {
            currentValue = Mathf.Lerp(currentValue, Mathf.Lerp(currentValue, toValue, timerValue / (maximumLength * startPercent)), speed * Time.deltaTime);
        }

        return currentValue;
    }

    /// <summary>
    /// Remapping a value (from2 and to2) -> (from1 and to1)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="from1"></param>
    /// <param name="to1"></param>
    /// <param name="from2"></param>
    /// <param name="to2"></param>
    /// <returns>Returns the remapped value</returns>
    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        // takes the value and it's clamp between c and d and remaps it to it's equal value between a and b
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}

public static class PlayerMethods
{
    public static MLAPI.NetworkedObject GetPlayerObjectByOwnerID(ulong ownerID)
    {
        List<MLAPI.NetworkedObject> objs = MLAPI.Spawning.SpawnManager.SpawnedObjectsList;

        for (int i = 0; i < objs.Count; i++)
        {
            if (objs[i].OwnerClientId != ownerID)
                continue;

            return objs[i];
        }

        return null;
    }

    public static bool CanSeeObject(Bounds bounds)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Player.Networker.Instance.Controller.Camera.Cam);

        if (GeometryUtility.TestPlanesAABB(planes, bounds))
            return true;
        else
            return false;
    }

    static Color teamColor = new Color(0.1764706f, 0.7843137f, 0.145098f, 1);
    static Color enemyColor = new Color(0.8862745f, 0.07058824f, 0.145098f, 1);
    static Color neutralColor = new Color(1, 1, 1, 1);
    public static string GetColoredName(string name, byte team)
    {
        bool sameTeam = team == Player.Networker.Instance.nv_Team.Value;

        string color = "";
        if (team > 0)
            color = ColorUtility.ToHtmlStringRGB(sameTeam ? teamColor : enemyColor);
        else
            color = ColorUtility.ToHtmlStringRGB(neutralColor);

        return string.Format("<color=#{0}>{1}</color>", color, name);
    }

    public static string GetColoredTitleName(string name, byte gameAccess, byte team)
    {
        bool sameTeam = team > 0 && team == Player.Networker.Instance.nv_Team.Value;

        // The tag that goes before the player to signify their status
        string title = "";
        if (gameAccess == 1)
            // Tester access
            title = "<color=#FF4EAD><b>[T] </b></color>";
        else if (gameAccess == 2)
            // Developer access
            title = "<color=#E66830><b>[DEV] </b></color>";

        string color = "";
        if (team > 0)
            color = ColorUtility.ToHtmlStringRGB(sameTeam ? teamColor : enemyColor);
        else
            color = ColorUtility.ToHtmlStringRGB(neutralColor);

        return string.Format(title + "<color=#{0}>{1}</color>", color, name);
    }

    const string teamMaterialName = "Team Color Rend Blue (Instance)";
    const string enemyMaterialName = "Team Color Rend Red (Instance)";
    const string teamLazerName = "Lazer_Blue (Instance)";
    const string enemyLazerName = "Lazer_Red (Instance)";
    static Color teamRendColor = new Color(0.0f, 0.2392157f, 1.0f, 1.0f);
    static Color enemyRendColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
    public static void SetTeamRenderers(MeshRenderer[] renderersInScene, int team)
    {
        // Get all of the game objects in the scene with a material
        //MeshRenderer[] renderersInScene = FindObjectsOfType<MeshRenderer>();

        foreach (MeshRenderer rend in renderersInScene)
        {
            Material[] materialsOnRenderer = rend.materials;

            foreach (Material mat in materialsOnRenderer)
            {
                // If the blue side material
                // and our team is on blue side
                if ((mat.name == teamMaterialName || mat.name == teamLazerName) && team == 2)
                {
                    Color color = new Color(teamRendColor.r, teamRendColor.g, teamRendColor.b, mat.color.a);
                    mat.SetColor("_BaseColor", color);
                }
                // If the blue side material
                // and our team is on red side
                else if ((mat.name == teamMaterialName || mat.name == teamLazerName) && team == 1)
                {
                    Color color = new Color(enemyRendColor.r, enemyRendColor.g, enemyRendColor.b, mat.color.a);
                    mat.SetColor("_BaseColor", color);
                }

                // If the red side material
                // and our team is on red side
                if ((mat.name == enemyMaterialName || mat.name == enemyLazerName) && team == 1)
                {
                    Color color = new Color(teamRendColor.r, teamRendColor.g, teamRendColor.b, mat.color.a);
                    mat.SetColor("_BaseColor", color);
                }
                // If the red side material
                // and our team is on blue side
                else if ((mat.name == enemyMaterialName || mat.name == enemyLazerName) && team == 2)
                {
                    Color color = new Color(enemyRendColor.r, enemyRendColor.g, enemyRendColor.b, mat.color.a);
                    mat.SetColor("_BaseColor", color);
                }
            }
        }
    }
}