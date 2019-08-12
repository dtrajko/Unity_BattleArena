using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkPlayerAudioController : NetworkBehaviour
{
    public void PlayWeaponSound(GameObject caller, int index)
    {
        CmdPlayWeaponSound(caller, index);
    }

    [Command]
    void CmdPlayWeaponSound(GameObject caller, int index)
    {
        if (!isServer) return;

        RpcPlayWeaponSound(caller, index);
    }

    [ClientRpc]
    void RpcPlayWeaponSound(GameObject caller, int index) {
        caller.GetComponent<Player>().PlayWeaponSound(index);
    }
}
