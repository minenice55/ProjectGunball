using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Gunball.MapObject;
using TMPro;

namespace Gunball.Interface
{
    public class ScoringSystem : MonoBehaviour
    {
        public int SideAScore, SideBScore;

        public TMP_Text SideATxt, SideBTxt;

        public static ScoringSystem instance;

        NetworkedScoringSystem netScoring;

        public void Awake()
        {
            SideAScore = 0;
            SideBScore = 0;

            UpdateTxt();
        }

        public void Start()
        {
            instance = this;
            netScoring = GetComponent<NetworkedScoringSystem>();
        }

        public void AddScore(int Score, ITeamObject.Teams _side, bool doCheck = true)
        {
            Debug.Log("Score Added " + Score + " to " + _side);
            ITeamObject.Teams opposingTeam = (_side == ITeamObject.Teams.Alpha) ? ITeamObject.Teams.Bravo : ITeamObject.Teams.Alpha;
            // scoring in opponent's goal should increment *your* score
            if (_side == ITeamObject.Teams.Alpha)
                SideBScore += Score;
            else if (_side == ITeamObject.Teams.Bravo)
                SideAScore += Score;

            UpdateTxt();
            GameCoordinator.instance.CallFixScores(SideAScore, SideBScore, opposingTeam);
            if (doCheck)
                CheckWinner();
        }

        public void SetScores(int alphaScore, int bravoScore, bool doCheck = true)
        {
            SideAScore = alphaScore;
            SideBScore = bravoScore;

            UpdateTxt();
            if (doCheck)
                CheckWinner();
        }

        void UpdateTxt()
        {
            Debug.Log("Team A Score: " + SideAScore + " Team B Score: " + SideBScore);
            SideATxt.text = $"{ITeamObject.Teams.Alpha.ToString()} {SideAScore}";
            SideBTxt.text = $"{SideBScore} {ITeamObject.Teams.Bravo.ToString()}";
        }

        void CheckWinner()
        {
            if (SideAScore >= GameCoordinator.instance.TargetScore)
            {
                // Team A wins
                GameCoordinator.instance.CallEndGame();
            }
            else if (SideBScore >= GameCoordinator.instance.TargetScore)
            {
                // Team B wins
                GameCoordinator.instance.CallEndGame();
            }
        }

        public void ResetScores()
        {
            SideAScore = 0;
            SideBScore = 0;

            UpdateTxt();
        }
    }
}