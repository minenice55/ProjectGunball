using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Gunball
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] GameObject lobbyWait;
        [SerializeField] GameObject[] playerListLabels;
        [SerializeField] TMP_InputField ipInput;
        [SerializeField] Toggle hostToggle;
        [SerializeField] AudioSource bgm;
        bool isHost = true;
        bool isLocalReady = false;
        private void Update() {
            if (GameCoordinator.instance == null) return;
            int i = 0;
            foreach (var entry in playerListLabels)
            {
                if (i < GameCoordinator.instance.JoinedPlayers)
                {
                    entry.SetActive(true);
                }
                else
                {
                    entry.SetActive(false);
                }
                i++;
            }
        }

        public void QuitGame () {
            Debug.Log("Game Ended");
            Application.Quit();
        }

        public void SetLocalReady()
        {
            isLocalReady = !isLocalReady;
            GameCoordinator.instance.PlayerReadyConfirm(isLocalReady);
            lobbyWait.SetActive(isLocalReady);
        }

        public void SetIsHost()
        {
            isHost = hostToggle.isOn;
        }

        public void PlayGame()
        {
            GameCoordinator.instance.StartJoinLobby(isHost);
        }

        public void StopMusic()
        {
            bgm.Stop();
        }

        public void SetConnectOrListenIp()
        {
            string address = ipInput.text;
            address.Trim();
            NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = address;
            NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.ServerListenAddress = address;
            Debug.Log($"Set Coonnect/Listen IP to {NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address}");
        }
    }
}