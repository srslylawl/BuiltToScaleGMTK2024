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

    public TextMeshProUGUI timerUGUI;
    public TextMeshProUGUI scoreUGUI;

    private void Awake()
    {
        if (I) throw new Exception("Duplicate GameLoop Instance");

        I = this;
    }

    private void Update()
    {
        timerUGUI.text = timer.ToString("F2");
        scoreUGUI.text = "Score: " + score;

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            Debug.Log("Lose!! Your Score: " + score);
        }
    }

    public static void ResetTimer()
    {
        timer = 60f;
    }

    public void IncreaseScore(int s)
    {
        score += s;
    }
}
