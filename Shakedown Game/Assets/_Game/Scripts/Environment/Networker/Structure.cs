using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enviro.Structures;
using MLAPI;
using MLAPI.Messaging;

namespace Enviro
{
    public class Structure : NetworkedBehaviour
    {
        public static Structure Instance;
        public StructureObject[] Structures;

        private void Start()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);

            Structures = FindObjectsOfType<StructureObject>();
        }

        [ClientRPC]
        public void UpdateStructure(byte id, short health)
        {
            foreach (StructureObject s in Structures)
            {
                if (s.ID != id)
                    continue;

                s.UpdateStructure(health);
                return;
            }
        }

        /*
        public override void DoDamage(RpcArgs args)
        {
        }

        public override void UpdateStructure(RpcArgs args)
        {
            MainThreadManager.Run(() =>
            {
                byte id = args.GetNext<byte>();
                short health = args.GetNext<short>();

                foreach (StructureObject s in Structures)
                {
                    if (s.ID == id)
                    {
                        s.UpdateStructure(health);
                        return;
                    }
                }
            });
        }
        */
    }
}