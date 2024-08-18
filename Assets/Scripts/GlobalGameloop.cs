using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GlobalGameloop : MonoBehaviour
{
    public static GlobalGameloop I;

    private static int score = 0;
    private static float timer = 60f;

    public TextMeshProUGUI scoreUGUI;

    private void Awake()
    {
        if (I) throw new Exception("Duplicate GameLoop Instance");

        I = this;
    }

    private void Update()
    {
        scoreUGUI.text = score + "";

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            Debug.Log("Lose!! Your Score: " + score);
        }
    }

    public static void FinishRound(int roundScore)
    {
        //roundScore += (int)timer * 5;
        timer = 30f;
        score += roundScore;
    }
}
