using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP;
using Newtonsoft.Json;
using System;

public class DB_API
{
    static string url = "https://game-api.shakedown.gg";
    public static int MatchID;

    public delegate void Server_Create_Delegate(GenericSuccess success);
    public static IEnumerator Server_Create(string uniqueServerID, int maxPlayers, int maxMatches, string ip, int port, Server_Create_Delegate callback)
    {
        GenericSuccess p = new GenericSuccess();

        HTTPRequest httpRequest = new HTTPRequest(new Uri(url + "/server/create/"), HTTPMethods.Post, (request, response) =>
        {
            Debug.Log("Created Server: " + response.DataAsText);
            p = JsonConvert.DeserializeObject<GenericSuccess>(response.DataAsText);
        });
        httpRequest.AddHeader("x-server-secret-key", "sh4k3d0wnIsL!f3!");
        httpRequest.AddField("uniqueServerId", uniqueServerID);
        httpRequest.AddField("maxPlayers", maxPlayers.ToString());
        httpRequest.AddField("maxMatches", maxMatches.ToString());
        httpRequest.AddField("ip", ip);
        httpRequest.AddField("port", port.ToString());
        httpRequest.DisableCache = true;
        httpRequest.Send();
        yield return httpRequest;

        callback(p);
    }

    public delegate void Server_Match_User_Authorized_Delegate(Server_Match_User_Authorized server_match_user_auth);
    public static IEnumerator Server_Match_User_Authorized(int matchID, int userID, Server_Match_User_Authorized_Delegate callback)
    {
        Server_Match_User_Authorized p = new Server_Match_User_Authorized();

        HTTPRequest httpRequest = new HTTPRequest(new Uri(url + string.Format("/server/match/{0}/user/{1}/authorized/", matchID, userID)), HTTPMethods.Get, (request, response) =>
        {
            Debug.Log("Server Match User Authorized: " + response.DataAsText);
            p = JsonConvert.DeserializeObject<Server_Match_User_Authorized>(response.DataAsText);
        });
        httpRequest.DisableCache = true;
        httpRequest.Send();
        yield return httpRequest;

        callback(p);
    }

    public delegate void Match_Create_Delegate(Match_Create match_create);
    public static IEnumerator Match_Create(string matchName, string map, int gameMode, int playerLimit, int timeLimit, int scoreLimit, string password, bool isTest, Match_Create_Delegate callback)
    {
        Match_Create p = new Match_Create();

        HTTPRequest httpRequest = new HTTPRequest(new Uri(url + "/match/create/"), HTTPMethods.Post, (request, response) =>
        {
            Debug.Log("Match Created: " + response.DataAsText);
            p = JsonConvert.DeserializeObject<Match_Create>(response.DataAsText);
            MatchID = p.MatchID;
        });
        httpRequest.AddField("name", matchName);
        httpRequest.AddField("map", map);
        httpRequest.AddField("gameMode", gameMode.ToString());
        httpRequest.AddField("playerLimit", playerLimit.ToString());
        httpRequest.AddField("timeLimit", timeLimit.ToString());
        httpRequest.AddField("scoreLimit", scoreLimit.ToString());
        httpRequest.AddField("password", password);
        httpRequest.AddField("isTest", (isTest ? 1 : 0).ToString());
        httpRequest.DisableCache = true;
        yield return httpRequest;

        callback(p);
    }
}

/// <summary>
/// Used to return any API callback that only has "success"
/// </summary>
public class GenericSuccess
{
    public bool Success { get; set; }
}

public class Server_Match_User_Authorized
{
    public bool Success { get; set; }
    public AuthorizedAccount Account { get; set; }
    public AuthorizedMatch Match { get; set; }

    public class AuthorizedAccount
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int GameAccess { get; set; }
    }

    public class AuthorizedMatch
    {
        public int TeamID { get; set; }
    }
}

public class Match_Create
{
    public bool Success { get; set; }
    public int MatchID { get; set; }
}