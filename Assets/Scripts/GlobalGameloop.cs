using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalGameloop : MonoBehaviour
{
    public static GlobalGameloop I;

    private static int score = 0;
    private static float timer = 60f;

    public TextMeshProUGUI scoreUGUI;
    public TextMeshProUGUI finalScoreUGUI;
    public GameObject GameOverObj;

    public bool gameOver = false;

    private void Awake() {
        if (I) Destroy(I.gameObject);
        I = this;
    }

    private void Start() {
        scoreUGUI.text = score + "";
    }

    public static void FinishRound(int roundScore)
    {
        timer = 30f;
        score += roundScore;
        I.scoreUGUI.text = score + "";
    }

    private void Update() {
        if (gameOver) {
            if (Input.GetKeyDown(KeyCode.R)) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    public static void TriggerGameOver() {
        // Debug.Log("GAME OVER!");
        I.GameOverObj.SetActive(true);
        I.gameOver = true;
        I.finalScoreUGUI.text = score.ToString();

    }
}
