using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Gunball.Interface;

public class EndGameUIScript : MonoBehaviour
{
    [SerializeField] ScoringSystem scoringSystem;

    public int ScoreA;
    public int ScoreB;

    public Text WinnersTxt;
    public Text ASideScore;
    public Text BSideScore;

    public void Start()
    {
        ScoreA = scoringSystem.SideAScore;
        ScoreB = scoringSystem.SideBScore;

        ASideScore.text = ScoreA.ToString();
        BSideScore.text = ScoreB.ToString();
    }

    public void updateWinners(){
        if (ScoreA > ScoreB){
            WinnersTxt.text = "Team A Wins!";
        }
        else{
            WinnersTxt.text = "Team B Wins!";
        }
    }


    // When click on the main menu button, load the main menu scene
    public void MainMenu()
    {
        // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuUI");
    }

    // when click on the quit button, quit the game
    public void Quit()
    {
        // Debug.Log("Quit");
        // Application.Quit();
    }


}
