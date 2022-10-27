using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Side
{
    A_Side,
    B_Side
}

public class ScoringSystem : MonoBehaviour
{
    public int SideAScore, SideBScore;

    public Text SideATxt, SideBTxt;

    public void Awake()
    {
        SideAScore = 0;
        SideBScore = 0;

        UpdateTxt();
    }

    public void SetScore(int Score,Side _side)
    {
        if (_side == Side.A_Side)
            SideAScore += Score;
        else
            SideBScore += Score;

        UpdateTxt();
    }

    void UpdateTxt()
    {
        SideATxt.text = SideAScore.ToString();
        SideBTxt.text = SideBScore.ToString();

        print("Side A Score : " + SideAScore.ToString());
        print("Side B Score : " + SideBScore.ToString());

    }
}
