using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Gunball.WeaponSystem;
using Gunball.MapObject;
using Cinemachine;

namespace Gunball
{
    using static WeaponDictionary;
    public class GameCoordinator : MonoBehaviour
    {
        [SerializeField] int maxPlayers = 2;
        [SerializeField] public CinemachineVirtualCamera vsWaitingCam;
        [SerializeField] GameObject waitingCamRoot;
        [SerializeField] GameObject mainMenuGO;
        [SerializeField] MainMenu mainMenu;
        [SerializeField] GameObject scoringSystem;
        [SerializeField] GameObject titleCamera;
        [SerializeField] public RespawnPoint[] respawnPoints;
        [SerializeField] public GameObject rammerPrefab;
        [SerializeField] public GameObject ballSpawn;
        [SerializeField] public Color[] teamColours;
        public int assignedRespawnPoints = 0;
        public static GameCoordinator instance;
        public List<WeaponEntry> weapons;
        public Dictionary<string, GameObject> weaponObjects;
        public NetworkCoordinator _netCoordinator;
        public bool gameStarted = false;
        int joinedPlayers;
        Dictionary<ulong, bool> readyPlayers;

        public int JoinedPlayers { get => joinedPlayers; set => joinedPlayers = value; }
        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            scoringSystem.SetActive(false);
            titleCamera.SetActive(true);
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        }

        // Update is called once per frame
        void Update()
        {
            if (waitingCamRoot != null)
            {
                waitingCamRoot.transform.Rotate(Vector3.up, 22.5f * Time.deltaTime);
            }
        }

        public void SetNetCoordinator(NetworkCoordinator netCoordinator)
        {
            _netCoordinator = netCoordinator;
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (gameStarted) response.Approved = false;
            if (NetworkManager.Singleton.ConnectedClients.Count >= maxPlayers)
            {
                response.Approved = false;
            }
            else
            {
                response.Approved = true;
                response.CreatePlayerObject = true;
            }
        }

        public void PlayerJoinConfirm()
        {
            titleCamera.SetActive(false);
            if (_netCoordinator == null)
            {
                ConfirmJoined();
            }
            else
            {
                _netCoordinator.ConfirmJoinedServerRpc();
            }
        }

        public void ConfirmJoined()
        {
            joinedPlayers++;
            // if (joinedPlayers >= maxPlayers && !gameStarted)
            // {
            //     StartGame();
            // }
        }

        public void PlayerReadyConfirm(bool ready)
        {
            if (_netCoordinator == null)
            {
                if (joinedPlayers >= maxPlayers && !gameStarted)
                {
                    StartGame();
                }
            }
            else
            {
                _netCoordinator.ConfirmReadyServerRpc(ready);
            }
        }

        public void AddRequiredReady(ulong clientId)
        {
            if (readyPlayers == null)
            {
                readyPlayers = new Dictionary<ulong, bool>();
            }
            readyPlayers.Add(clientId, false);
        }

        public void ConfirmReady(bool ready, ulong clientId)
        {
            if (readyPlayers == null)
            {
                readyPlayers = new Dictionary<ulong, bool>();
            }
            if (readyPlayers.ContainsKey(clientId))
            {
                readyPlayers[clientId] = ready;
            }
            else
            {
                readyPlayers.Add(clientId, ready);
            }
            bool allReady = true;
            foreach (bool readyPlayer in readyPlayers.Values)
            {
                if (!readyPlayer)
                {
                    allReady = false;
                    break;
                }
            }
            if (allReady && !gameStarted)
            {
                StartGame();
            }
        }

        public void StartJoinLobby(bool isHost)
        {
            if (isHost)
            {
                NetworkManager.Singleton.StartHost();
            }
            else
            {
                NetworkManager.Singleton.StartClient();
            }
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

        public void StartGame()
        {
            gameStarted = true;
            if (_netCoordinator == null)
            {
                SetPlayLayout();
                ballSpawn.GetComponent<GunBall>().DoDeath();
                foreach (Player player in FindObjectsOfType<Player>())
                {
                    player.StartGame(Time.time + 5f);
                }
            }
            else
            {
                _netCoordinator.StartGameServerRpc();
            }
        }

        public void SetPlayLayout()
        {
            mainMenu.StopMusic();
            mainMenuGO.SetActive(false);
            scoringSystem.SetActive(true);
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