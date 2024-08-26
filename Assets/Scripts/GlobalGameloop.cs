using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalGameloop : MonoBehaviour {
	public static GlobalGameloop I;

	private int score = 0;

	public TextMeshProUGUI scoreUGUI;
	public TextMeshProUGUI finalScoreUGUI;
	public GameObject GameOverObj;

	public bool gameOver = false;

	private void Awake() {
		if (I && I != this) Destroy(I.gameObject);
		I = this;
	}

	private void Start() {
		scoreUGUI.text = score + "";
	}

	public static void FinishRound(int roundScore) {
		I.score += roundScore;
		I.scoreUGUI.text = I.score + "";
	}

	private void Update() {
		if (gameOver || Input.GetKeyDown(KeyCode.RightShift)) {
			if (Input.GetKeyDown(KeyCode.R)) {
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			}
		}
	}

	public static void TriggerGameOver() {
		I.GameOverObj.SetActive(true);
		I.gameOver = true;
		I.finalScoreUGUI.text = I.score.ToString();
	}
}