using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DB_Object : MonoBehaviour
{
    public static DB_Object Instance;

    private void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void Start_User_Ping_API()
    {
        InvokeRepeating("user_ping_api", 0.0f, 4.0f);
    }
    void user_ping_api()
    {
        StartCoroutine(DB_API.User_Ping(DB_API.UserAuth.SessionID, callback => { }));
    }

    public void Start_Match_Ping_API()
    {
        InvokeRepeating("match_ping_api", 0.0f, 4.0f);
    }
    public void Stop_Match_Ping_API()
    {
        CancelInvoke("match_ping_api");
    }
    void match_ping_api()
    {
        StartCoroutine(DB_API.Match_Ping(DB_API.UserAuth.SessionID, DB_API.MatchID, callback => { }));
    }
}
