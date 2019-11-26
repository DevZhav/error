using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureObject : MonoBehaviour
{
    public byte ID;
    public short MaxHealth = 100;
    public float RespawnTime = 5.0f;

    [Header("")]
    public short Health;

    private void Start()
    {
        Health = MaxHealth;
        Debug.Log("Initialized structure object");
    }

    public void DoDamage(short damage)
    {
        if (Health <= 0)
            return;

        Debug.Log("Player did " + damage + " damage to the structure");
        Health -= damage;
        //Structure.Instance.networkObject.SendRpc("UpdateStructure", BeardedManStudios.Forge.Networking.Receivers.Others, ID, Health);
        Structure.Instance.InvokeClientRpcOnEveryone("UpdateStructure", ID, Health, channel: "Reliable");

        if (Health <= 0)
        {
            StartCoroutine(Respawn());
        }
    }

    private IEnumerator Respawn()
    {
        GetComponent<Collider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        Debug.Log("Structure object has been destroyed");

        yield return new WaitForSeconds(RespawnTime);

        Health = MaxHealth;
        GetComponent<Collider>().enabled = true;
        GetComponent<MeshRenderer>().enabled = true;

        // Respawn over the network
        //Structure.Instance.networkObject.SendRpc("UpdateStructure", BeardedManStudios.Forge.Networking.Receivers.Others, ID, Health);
        Structure.Instance.InvokeClientRpcOnEveryone("UpdateStructure", ID, Health, channel: "Reliable");
        Debug.Log("Structure object has respawned");
    }
}
