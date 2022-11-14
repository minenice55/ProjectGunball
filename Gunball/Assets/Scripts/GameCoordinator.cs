using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Gunball.WeaponSystem;
using Gunball.MapObject;

namespace Gunball
{
    using static WeaponDictionary;
    public class GameCoordinator : MonoBehaviour
    {
        public static GameCoordinator instance;
        public List<WeaponEntry> weapons;
        public Dictionary<string, GameObject> weaponObjects;
        void Awake()
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

        public GameObject CreateGlobalWeapon(string name)
        {
            if (weaponObjects == null)
            {
                weaponObjects = new Dictionary<string, GameObject>();
            }
            if (!weaponObjects.ContainsKey(name))
            {
                GameObject weapon = Instantiate(GetWeaponFromName(name));
                weapon.GetComponent<WeaponBase>().IsGlobalWeapon = true;
                weaponObjects.Add(name, weapon);
            }
            return weaponObjects[name];
        }

        public GameObject CreatePlayerWeapon(string name)
        {
            GameObject weapon = GameObject.Instantiate(GetWeaponFromName(name));
            weapon.GetComponent<WeaponBase>().IsGlobalWeapon = false;
            return weapon;
        }

        public GameObject GetWeaponFromName(string name)
        {
            foreach (WeaponEntry entry in weapons)
            {
                if (entry.name == name)
                {
                    return entry.weaponManager;
                }
            }
            throw new System.Exception("Weapon " + name + " not found");
        }
    }
}