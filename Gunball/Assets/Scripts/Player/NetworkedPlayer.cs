using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Gunball.MapObject;
using Gunball.WeaponSystem;
using Cinemachine;

namespace Gunball
{
    public class NetworkedPlayer : NetworkBehaviour
    {
        [SerializeField] GameObject playerCamera;
        [SerializeField] GameObject playerGuide;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                Destroy(playerCamera);
                Destroy(playerGuide);
                GetComponent<Player>().IsPlayer = false;
            }
        }
    }
}