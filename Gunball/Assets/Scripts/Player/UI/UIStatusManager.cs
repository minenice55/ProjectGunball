using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Gunball.MapObject
{
    public class UIStatusManager : MonoBehaviour
    {
        [SerializeField] Slider healthBar;
        [SerializeField] TMP_Text healthText;
        [SerializeField] TMP_Text respawnText;

        readonly string[] respawningTexts = new string[] {"Respawning", "Respawning.", "Respawning..", "Respawning..."};
        readonly string[] firstSpawnTexts = new string[] {"Ready", "Ready.", "Ready..", "Ready..."};
        
        public void SetHealth(float health, float maxHealth)
        {
            respawnText.gameObject.SetActive(false);
            healthText.gameObject.SetActive(true);
            healthBar.value = health / maxHealth;
            healthText.text = $"{(int)health}/{(int)maxHealth}";
        }

        public void SetRespawning(float respawnProgress, float maxTime, bool aiming, bool isGameStart)
        {
            respawnText.gameObject.SetActive(true);
            healthText.gameObject.SetActive(false);
            healthBar.value = Mathf.Max((0.5f - (respawnProgress / maxTime)) * 2f, 0f);
            if (aiming)
            {
                int idx = (int)(Time.time * 3f % respawningTexts.Length);
                respawnText.text = respawningTexts[idx];
            }
            else
                respawnText.text = $"Wait {respawnProgress:0.00}...";
        }

        public void SetTeam(ITeamObject.Teams team)
        {
            Color teamColor = GameCoordinator.instance.GetTeamColor(team);
            healthBar.fillRect.GetComponent<Image>().color = teamColor;
            healthText.color = Color.Lerp(teamColor, Color.white, 0.6f);
        }
    }
}