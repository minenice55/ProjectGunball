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
                Destroy(GetComponentInChildren<Camera>().gameObject);
                Destroy(GetComponentInChildren<AudioListener>().gameObject);
                Destroy(GetComponentInChildren<CinemachineVirtualCameraBase>().gameObject);
                Destroy(GetComponentInChildren<CinemachineBrain>().gameObject);
                Destroy(GetComponentInChildren<GuideManager>().gameObject);
                GetComponent<Player>().IsPlayer = false;
            }
        }
    }
}