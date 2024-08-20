using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public class ScaleManager : MonoBehaviour {
	public GameObject Floor;
	public GameObject Scale;
	public GameObject Canvas;
	public GameObject ScoreNum;
	public GameObject PerfectPlay;
	public float CircleAnimTime = 0.5f;

	public Material groundMat;
	public List<GameObject> scoresList = new();
	private bool win = false;
	public GridLayoutData floorLayoutActive;
	private GridLayoutData scaleLayoutActive;
	private List<GameObject> tileList = new();
	private List<GameObject> oldTileList = new();
	public List<GridLayoutData> scaleLayouts = new();
	private HashSet<Transform> blocks = new HashSet<Transform>();
	private Dictionary<IGridOccupant, int> blocksScoreCount = new();

	private Vector2Int thisPosOffset;


	private ComputeBuffer tilePosBuffer;


	private Texture2D scalePositionsTexture;


	void PassTileDataToGPU() {
		int numScalePositions = scaleLayoutActive.GridPositions.Count;

		// Create a 1D texture with width equal to the number of positions
		scalePositionsTexture = new Texture2D(numScalePositions, 1, TextureFormat.RGFloat, false);
		scalePositionsTexture.wrapMode = TextureWrapMode.Clamp;
		scalePositionsTexture.filterMode = FilterMode.Point;

		int x = 0;
		foreach (var gridPosition in scaleLayoutActive.GridPositions) {
			var pos = new Vector2(gridPosition.x, gridPosition.y + transform.position.z);
			Color pixelColor = new Color(pos.x, pos.y, 0.0f, 0.0f); // Store Vector2 in the RG channels
			scalePositionsTexture.SetPixel(x, 0, pixelColor);
			x++;
		}
		// Apply changes to the texture
		scalePositionsTexture.Apply();

		// if (tilePosBuffer != null) tilePosBuffer.Dispose();
		// int count = scaleLayoutActive.GridPositions.Count;
		// var data = new List<Vector2>(count);
		// foreach (var gridPosition in scaleLayoutActive.GridPositions) {
			// data.Add(new Vector2(gridPosition.x, gridPosition.y + transform.position.z));
		// }

		// tilePosBuffer = new ComputeBuffer(count, sizeof(float) * 2);
		// tilePosBuffer.SetData(data);
		// Shader.SetGlobalBuffer("scalePositions", tilePosBuffer);
		Shader.SetGlobalInt("numScalePositions", numScalePositions);
		Shader.SetGlobalTexture("_scalePositionsTexture", scalePositionsTexture);
	}

	private void OnDisable() {
		if (tilePosBuffer != null) {
			tilePosBuffer.Dispose();
		}
	}


	// Start is called before the first frame update
	void Start() {
		thisPosOffset = new Vector2Int((int)this.transform.position.x, (int)this.transform.position.z);

		SetupTiles(true);

		Invoke(nameof(CheckForWin), 0.2f);
	}

	void CheckForWin() {
		win = true;

		foreach (Vector2Int v2 in scaleLayoutActive.GridPositions) {
			if (!GlobalGrid.GridOccupants.TryGetValue(v2 + thisPosOffset, out var occupant) || occupant is PlayerControls) {
				win = false;
				break;
			}
		}

		if (win) {
			int roundScore = 0;

			scoresList.Clear();


			var occupants = new HashSet<IGridOccupant>();
			foreach (Vector2Int v2 in scaleLayoutActive.GridPositions) {
				Vector2Int currentCell = v2 + thisPosOffset;
				if (!GlobalGrid.GridOccupants.TryGetValue(currentCell, out var occupant)) {
					Debug.LogException(new Exception("no occupant on scale cell that should be occupied: " + currentCell));
				}

				occupants.Add(occupant);
				
				// every grid cell is filled, points are granted for every block on the cell, and removed for every block of an occupant thats not on the cells
				if (blocksScoreCount.ContainsKey(occupant)) blocksScoreCount[occupant]++;
				else blocksScoreCount.Add(occupant, 1);

				Vector3 screenPos = Camera.main.WorldToScreenPoint(new Vector3(v2.x + thisPosOffset.x, 0, v2.y + thisPosOffset.y));
				GameObject sn = Instantiate(ScoreNum, screenPos, Quaternion.identity, Canvas.transform);
				int score = blocksScoreCount[occupant] * 10;
				sn.GetComponent<TextMeshProUGUI>().text = "+" + score;
				sn.SetActive(false);
				scoresList.Add(sn);
				
				// Debug.Log($"Field {currentCell} added Score: {score}");
			}

			foreach (int value in blocksScoreCount.Values) {
				int add = (value * (value + 1) / 2) * 10;
				roundScore += add;
				// Debug.Log($"value added to roundscore {roundScore}: added Score: {add}");
			}

			int scoreTemp = roundScore;
			
			foreach (var gridOccupant in occupants) {
				//now we check which tile is NOT on the scale and deduct points
				foreach (var gridOccupantPosition in gridOccupant.Positions) {
					var scaleLayoutPos = gridOccupantPosition - thisPosOffset;
					if (!scaleLayoutActive.GridPositions.Contains(scaleLayoutPos)) {
						roundScore -= 20;
						Vector3 screenPos = Camera.main.WorldToScreenPoint(new Vector3(gridOccupantPosition.x, 0, gridOccupantPosition.y));
						GameObject sn = Instantiate(ScoreNum, screenPos, Quaternion.identity, Canvas.transform);
						sn.GetComponent<TextMeshProUGUI>().text = "-20";
						sn.SetActive(false);
						scoresList.Add(sn);
						// Debug.Log($"Tile not on scale: {scaleLayoutPos}: removed Score: {-20}");
					}
				}
			}

			//if perfect we add another 200
			if (roundScore == scoreTemp) {
				roundScore += 200;
				Vector3 screenPos = new Vector3(Screen.width / 2, Screen.height / 2, 0);
				GameObject sn = Instantiate(ScoreNum, screenPos, Quaternion.identity, Canvas.transform);
				sn.GetComponent<TextMeshProUGUI>().text = "PERFECT! +200";
				sn.SetActive(false);
				scoresList.Add(sn);
			}

			int invokeCounter = 1;
			StartCoroutine(SpawnScores(scoresList));

			//Do Score Stuff
			GlobalGameloop.FinishRound(roundScore);

			//Destroy Blocks for now. crane them later
			foreach (var occupant in occupants) {
				Destroy(((MonoBehaviour)occupant).gameObject);
			}

			//Reset Scale
			ResetScales();
		}

		Invoke(nameof(CheckForWin), 0.2f);
	}

	IEnumerator SpawnScores(List<GameObject> scores) {
		foreach (GameObject s in scores) {
			s.SetActive(true);
			yield return new WaitForSeconds(0.05f);
		}
	}

	void ResetScales() {
		win = false;
		SetupTiles();
	}

	IEnumerator AnimateFirstScaleCircle() {
		float endTime = Time.time + 1.0f*CircleAnimTime;
		while (Time.time < endTime) {
			var remaining = endTime - Time.time;
			remaining /= CircleAnimTime;

			var lerp = 0.5f + Mathf.Sin(-0.5f*Mathf.PI + remaining * Mathf.PI) * 0.5f;
			groundMat.SetFloat("_CurrentScaleCircleSize", lerp);

			yield return null;
		}
	}

	IEnumerator AnimateScaleCircles() {
		float endTime = Time.time + 1.0f*CircleAnimTime;
		while (Time.time < endTime) {
			var remaining = endTime - Time.time;
			remaining /= CircleAnimTime;

			var lerp = 0.5f + Mathf.Sin(-0.5f*Mathf.PI + remaining * Mathf.PI * 2.0f) * 0.5f;
			groundMat.SetFloat("_CurrentScaleCircleSize", lerp);

			yield return null;
		}
	}

	private void SetupTiles(bool init = false) {
		floorLayoutActive.GridPositions.Clear();
		for (int x = 0; x < 7; x++) {
			for (int z = 0; z < 7; z++) {
				floorLayoutActive.GridPositions.Add(new Vector2Int(x, z));
			}
		}

		scaleLayoutActive = scaleLayouts[UnityEngine.Random.Range(0, scaleLayouts.Count)];
		PassTileDataToGPU();

		foreach (Vector2Int v2 in scaleLayoutActive.GridPositions) {
			// GameObject obj = Instantiate(Scale, new Vector3(v2.x, 0f, v2.y) + this.transform.position, Quaternion.identity, this.transform);
			// tileList.Add(obj);
			floorLayoutActive.GridPositions.Remove(v2);
		}

		if (init) {
			StartCoroutine(AnimateFirstScaleCircle());
		}
		else {
			StartCoroutine(AnimateScaleCircles());

		}
	}
}