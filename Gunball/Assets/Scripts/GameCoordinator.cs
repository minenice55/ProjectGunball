using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Gunball.WeaponSystem;

namespace Gunball
{
    using static WeaponDictionary;
    public class GameCoordinator : MonoBehaviour
    {
        public static GameCoordinator instance;
        public List<WeaponEntry> weapons;
        // Start is called before the first frame update
        void Start()
        {
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        void OnGUI() {
            GUILayout.BeginArea(new Rect(10, 10, 200, 200));
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) {
                if (GUILayout.Button("Host")) {
                    NetworkManager.Singleton.StartHost();
                }
                if (GUILayout.Button("Client")) {
                    NetworkManager.Singleton.StartClient();
                }
            }
            GUILayout.EndArea();
        }
    }
}