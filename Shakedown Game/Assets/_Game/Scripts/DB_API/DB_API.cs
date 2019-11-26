using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP;
using Newtonsoft.Json;
using System;

public class DB_API
{
    public static User_Auth UserAuth;
    public static User_Info UserInfo;

    public static int MatchID;
    public static string ConnectionData
    {
        get
        {
            string i = MatchID.ToString();
            i += "," + UserInfo.Account.ID;
            return i;
        }
    }

    static string url = "https://game-api.shakedown.gg";
    static string img_url = "https://i.s4db.net/id/";

#region USER DATA
    public delegate void User_Auth_Delegate(User_Auth user_auth);
    public static IEnumerator User_Auth(string email, string password, User_Auth_Delegate callback)
    {
        User_Auth p = new User_Auth();

        HTTPRequest httpRequest = new HTTPRequest(new Uri(url + "/user/auth/"), HTTPMethods.Post, (request, response) =>
        {
            Debug.Log("User Authorization: " + response.DataAsText);
            p = JsonConvert.DeserializeObject<User_Auth>(response.DataAsText);
        });
        httpRequest.AddField("email", email);
        httpRequest.AddField("password", password);
        httpRequest.DisableCache = true;
        httpRequest.Send();
        yield return httpRequest;

        UserAuth = p;
        callback(p);
    }

    public delegate void User_Info_Delegate(User_Info user_info);
    public static IEnumerator User_Info(string sessionID, User_Info_Delegate callback)
    {
        User_Info p = new User_Info();

        HTTPRequest httpRequest = new HTTPRequest(new Uri(url + "/user/info/"), HTTPMethods.Get, (request, response) =>
        {
            Debug.Log("User Info: " + response.DataAsText);
            p = JsonConvert.DeserializeObject<User_Info>(response.DataAsText);
        });
        httpRequest.AddHeader("x-session-key", sessionID);
        httpRequest.DisableCache = true;
        httpRequest.Send();
        yield return httpRequest;

        UserInfo = p;
        callback(p);
    }

    public delegate void User_Avatar_Delegate(Texture2D image);
    /// <summary>
    /// Gets the user's avatar in different sizes depending on the suffix
    /// </summary>
    /// <param name="prefix">s = 90px | b = 160px | t = 160px | m = 320px | l = 640px | h = 1024px</param>
    /// <returns></returns>
    public static IEnumerator User_Avatar(string prefix, User_Avatar_Delegate callback)
    {
        Texture2D img = null;
        string link = img_url + UserInfo.Account.Avatar + prefix + ".png";

        HTTPRequest httpRequest = new HTTPRequest(new Uri(link), HTTPMethods.Get, (request, response) =>
        {
            Debug.Log("Downloaded User Avatar from: " + link);
            img = response.DataAsTexture2D;
        });
        httpRequest.Send();
        yield return httpRequest;

        callback(img);
    }

    public delegate void User_Ping_Delegate(bool success);
    public static IEnumerator User_Ping(string sessionID, User_Ping_Delegate callback)
    {
        GenericSuccess p = new GenericSuccess();

        HTTPRequest httpRequest = new HTTPRequest(new Uri(url + "/user/ping/"), HTTPMethods.Get, (request, response) =>
        {
            Debug.Log("User Ping: " + response.DataAsText);
            p = JsonConvert.DeserializeObject<GenericSuccess>(response.DataAsText);
        });
        httpRequest.AddHeader("x-session-key", sessionID);
        httpRequest.DisableCache = true;
        httpRequest.Send();
        yield return httpRequest;

        callback(p.Success);
    }
#endregion

#region MATCH DATA
    public delegate void Match_List_Delegate(Match_List match_list);
    public static IEnumerator Match_List(string sessionID, Match_List_Delegate callback)
    {
        Match_List p = new Match_List();

        HTTPRequest httpRequest = new HTTPRequest(new Uri(url + "/match/list/"), HTTPMethods.Get, (request, response) =>
        {
            Debug.Log("Match List: " + response.DataAsText);
            p = JsonConvert.DeserializeObject<Match_List>(response.DataAsText);
            var matches = p.Matches; // We do this because i don't know why the fuck it works without it
        });
        httpRequest.AddHeader("x-session-key", sessionID);
        httpRequest.DisableCache = true;
        httpRequest.Send();
        yield return httpRequest;

        callback(p);
    }

    public delegate void Match_Create_Delegate(Match_Create match_create);
    public static IEnumerator Match_Create(string sessionKey, string matchName, string map, int gameMode, int playerLimit, int timeLimit, int scoreLimit, string password, bool isTest, Match_Create_Delegate callback)
    {
        Match_Create p = new Match_Create();

        HTTPRequest httpRequest = new HTTPRequest(new Uri(url + "/match/create/"), HTTPMethods.Post, (request, response) =>
        {
            Debug.Log("Match Created: " + response.DataAsText);
            p = JsonConvert.DeserializeObject<Match_Create>(response.DataAsText);
            MatchID = p.MatchID;
        });
        httpRequest.AddHeader("x-session-key", sessionKey);
        httpRequest.AddField("name", matchName);
        httpRequest.AddField("map", map);
        httpRequest.AddField("gameMode", gameMode.ToString());
        httpRequest.AddField("playerLimit", playerLimit.ToString());
        httpRequest.AddField("timeLimit", timeLimit.ToString());
        httpRequest.AddField("scoreLimit", scoreLimit.ToString());
        httpRequest.AddField("password", password);
        httpRequest.AddField("isTest", (isTest ? 1 : 0).ToString());
        httpRequest.DisableCache = true;
        httpRequest.Send();
        yield return httpRequest;

        callback(p);
    }

    public delegate void Match_Join_Delegate(Match_Join match_join);
    public static IEnumerator Match_Join(string sessionID, int matchID, string password, Match_Join_Delegate callback)
    {
        Match_Join p = new Match_Join();

        HTTPRequest httpRequest = new HTTPRequest(new Uri(url + "/match/join/"), HTTPMethods.Post, (request, response) =>
        {
            Debug.Log("Match Join: " + response.DataAsText);
            p = JsonConvert.DeserializeObject<Match_Join>(response.DataAsText);
        });
        httpRequest.AddHeader("x-session-key", sessionID);
        httpRequest.AddField("id", matchID.ToString());
        httpRequest.AddField("password", password);
        httpRequest.DisableCache = true;
        httpRequest.Send();
        yield return httpRequest;

        callback(p);
    }

    public delegate void Match_Leave_Delegate(bool success);
    public static IEnumerator Match_Leave(string sessionID, int matchID, Match_Leave_Delegate callback)
    {
        GenericSuccess p = new GenericSuccess();

        HTTPRequest httpRequest = new HTTPRequest(new Uri(url + "/match/leave/"), HTTPMethods.Post, (request, response) =>
        {
            Debug.Log("Match Leave: " + response.DataAsText);
            p = JsonConvert.DeserializeObject<GenericSuccess>(response.DataAsText);
        });
        httpRequest.AddHeader("x-session-key", sessionID);
        httpRequest.AddField("id", matchID.ToString());
        httpRequest.DisableCache = true;
        httpRequest.Send();
        yield return httpRequest;

        callback(p.Success);
    }

    public delegate void Match_Ping_Delegate(bool success);
    public static IEnumerator Match_Ping(string sessionID, int matchID, Match_Ping_Delegate callback)
    {
        GenericSuccess p = new GenericSuccess();

        HTTPRequest httpRequest = new HTTPRequest(new Uri(url + "/match/ping/"), HTTPMethods.Post, (request, response) =>
        {
            Debug.Log("Match Ping: " + response.DataAsText);
            p = JsonConvert.DeserializeObject<GenericSuccess>(response.DataAsText);
        });
        httpRequest.AddHeader("x-session-key", sessionID);
        httpRequest.AddField("id", matchID.ToString());
        httpRequest.DisableCache = true;
        httpRequest.Send();
        yield return httpRequest;

        callback(p.Success);
    }
#endregion
}

/// <summary>
/// Used to return any API callback that only has "success"
/// </summary>
public class GenericSuccess
{
    public bool Success { get; set; }
}

#region USER DATA
public class User_Auth
{
    public bool Success { get; set; }
    public string SessionID { get; set; }
}

public class User_Info
{
    public bool Success { get; set; }
    public PlayerAccount Account { get; set; }

    public class PlayerAccount
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int GameAccess { get; set; }
        public string Avatar { get; set; }
    }
}
#endregion

#region MATCH DATA
public class Match_List
{
    public bool Success { get; set; }
    public List<Match> Matches { get; set; }

    public class Match
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Map { get; set; }
        public int GameMode { get; set; }
        public bool HasPassword { get; set; }
        public int PlayerOnline { get; set; }
        public int PlayerLimit { get; set; }
        public List<MatchPlayers> Players { get; set; }

        public class MatchPlayers
        {
            public string Name { get; set; }
            public int TeamID { get; set; }
        }
    }
}

public class Match_Create
{
    public bool Success { get; set; }
    public int MatchID { get; set; }
}

public class Match_Join
{
    public bool Success { get; set; }
    public MatchServer Server { get; set; }

    public class MatchServer
    {
        public string IP { get; set; }
        public int Port { get; set; }
    }
}
#endregion