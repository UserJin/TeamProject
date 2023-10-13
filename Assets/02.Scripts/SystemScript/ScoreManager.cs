using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager
{
    private int score;

    public int Score
    {
        get
        {
            return score;
        }
        set
        {
            score = value;
        }
    }

    public ScoreManager()
    {
        score = 0;
    }

    public void AddScore(int num)
    {
        score += num;
    }

    public void MultiScore(float num)
    {
        score = (int)(score * num);
    }

    public string CheckRank()
    {
        string s = "F";
        if (score >= 10000) s = "A";
        return s;
    }

    public void CalCombo(int _combo)
    {
        score += _combo * 500;
    }
}
