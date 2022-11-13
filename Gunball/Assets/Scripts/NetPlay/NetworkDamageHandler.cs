using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Gunball.MapObject;
using Gunball.WeaponSystem;
using Cinemachine;

namespace Gunball
{
    public class NetworkDamage : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            //Debug.Log("NetworkDamage.OnNetworkSpawn");
        }
    }
}
