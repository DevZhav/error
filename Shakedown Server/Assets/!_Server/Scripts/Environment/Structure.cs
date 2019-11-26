using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : NetworkedBehaviour
{
    public static Structure Instance;
    public StructureObject[] Structures;

    public void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        Structures = FindObjectsOfType<StructureObject>();
    }

    public void UpdateAllStructures()
    {
        foreach (StructureObject s in Structures)
        {
            //networkObject.SendRpc(RPC_UPDATE_STRUCTURE, Receivers.Others, s.ID, s.Health);
            InvokeClientRpcOnEveryone("UpdateStructure", s.ID, s.Health, channel: "Reliable");
        }
    }

    [ServerRPC(RequireOwnership = false)]
    public void DoDamage(byte id, short damage)
    {
        foreach (StructureObject s in Structures)
        {
            if (s.ID != id)
                continue;

            s.DoDamage(damage);
            return;
        }
    }

    /*
    public override void DoDamage(RpcArgs args)
    {
        MainThreadManager.Run(() =>
        {
            byte id = args.GetNext<byte>();
            short dmg = args.GetNext<short>();

            foreach (StructureObject s in Structures)
            {
                if (s.ID == id)
                {
                    s.DoDamage(dmg);
                    return;
                }
            }
        });
    }

    public override void UpdateStructure(RpcArgs args)
    {
        // Runs on client
    }
    */
}