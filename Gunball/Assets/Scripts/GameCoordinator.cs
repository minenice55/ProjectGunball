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
        [SerializeField] public RespawnPoint[] respawnPoints;
        [SerializeField] public GameObject rammerPrefab;
        [SerializeField] public Color[] teamColours;
        public int assignedRespawnPoints = 0;
        public static GameCoordinator instance;
        public List<WeaponEntry> weapons;
        public Dictionary<string, GameObject> weaponObjects;
        public NetworkCoordinator _netCoordinator;
        void Awake()
        {
            instance = this;
        }

        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void SetNetCoordinator(NetworkCoordinator netCoordinator)
        {
            _netCoordinator = netCoordinator;
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
                weaponObjects.Add(name, weapon);
            }
            return weaponObjects[name];
        }

        public GameObject CreatePlayerWeapon(string name)
        {
            GameObject weapon = GameObject.Instantiate(GetWeaponFromName(name));
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

        public RespawnPoint GetRespawnPointForTeam(ITeamObject.Teams team, int lastAssigned)
        {
            int i = 0;
            foreach (RespawnPoint point in respawnPoints)
            {
                if (i < lastAssigned)
                {
                    i++;
                    continue;
                }
                if (!point.IsForRammer) continue;
                if (point.ObjectTeam == team)
                {
                    return point;
                }
            }
            return null;
        }

        public void AssignRespawnRammer(GameObject player)
        {
            if (_netCoordinator == null)
            {
                int pointNum = assignedRespawnPoints % respawnPoints.Length;
                RespawnPoint point = respawnPoints[pointNum];
                if (pointNum % 2 == 0)
                    point = GetRespawnPointForTeam(ITeamObject.Teams.Alpha, assignedRespawnPoints);
                else
                    point = GetRespawnPointForTeam(ITeamObject.Teams.Bravo, assignedRespawnPoints);

                Vector3 pos = point.Position;
                Quaternion facing = point.Facing;

                GameObject rammer = Instantiate(rammerPrefab, pos, facing);
                rammer.GetComponent<RespawnRammer>().SetOwner(player);
                rammer.GetComponent<RespawnRammer>().SetTeam(point.ObjectTeam);

                assignedRespawnPoints++;
            }
            else
            {
                _netCoordinator.AssignRespawnPointServerRpc();
            }
        }

        public Color GetTeamColor(ITeamObject.Teams team)
        {
            if (teamColours.Length > (int)team)
            {
                return teamColours[(int)team];
            }
            else
            {
                return Color.white;
            }
        }
    }
}