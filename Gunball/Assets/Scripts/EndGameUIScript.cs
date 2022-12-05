using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Gunball.Interface;

using TMPro;

namespace Gunball
{
    public class EndGameUIScript : MonoBehaviour
    {
        public int ScoreA;
        public int ScoreB;

        public TMP_Text WinnersTxt;
        public TMP_Text ASideScore;
        public TMP_Text BSideScore;

        public void Start()
        {
            ScoreA = ScoringSystem.instance.SideAScore;
            ScoreB = ScoringSystem.instance.SideBScore;

            ASideScore.text = $"{ScoreA}p";
            BSideScore.text = $"{ScoreB}p";

            UpdateWinners();
        }

        public void UpdateWinners()
        {
            ScoreA = ScoringSystem.instance.SideAScore;
            ScoreB = ScoringSystem.instance.SideBScore;

            ASideScore.text = $"{ScoreA}p";
            BSideScore.text = $"{ScoreB}p";

            if (ScoreA > ScoreB)
            {
                WinnersTxt.text = "Team Alpha Wins!";
            }
            else
            {
                WinnersTxt.text = "Team Bravo Wins!";
            }
        }

        // When click on the lobby button, return to the lobby and reset game state
        public void ReturnToLobby()
        {
            ScoringSystem.instance.ResetScores();
        }

        // when click on the quit button, quit the game
        public void Quit()
        {
            Debug.Log("Quit");
            Application.Quit();
        }
    }
}