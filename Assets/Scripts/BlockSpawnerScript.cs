using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlockSpawnerScript : MonoBehaviour {
	public float timeBetweenSpawns = 5f;
	// public int spawnTimeVariance;

	private Timer spawnTimer;

	[SerializeField] public GameObject PalettePrefab;
	[SerializeField] public List<BlockData> blockData = new();

	// Start is called before the first frame update
	void Start() {
		//Call BlockSpawn Methode after time
		// Invoke(nameof(SpawnBlock), 0 + Random.Range(-spawnTimeVariance, spawnTimeVariance));
		spawnTimer = new Timer(timeBetweenSpawns, true);
	}

	private int blockNum = 0;

	private void Update() {
		if (spawnTimer) {
			SpawnBlock();
			spawnTimer = new Timer(timeBetweenSpawns);
		}
	}

	void SpawnBlock() {
		int blockType = Random.Range(0, blockData.Count);

		GameObject blockGameObject = new GameObject($"Block_{blockNum}_('{blockType}')");
		Block b = blockGameObject.AddComponent<Block>();
		var data = blockData[blockType];

		//we need to move it so it leaves the area, so find how wide it is if we assume moving from left to right
		int width = 1;
		foreach (var dataGridPosition in data.GridPositions) {
			width = Math.Max(dataGridPosition.x + 1, width);
		}

		blockGameObject.transform.position = transform.position - new Vector3(width, 0, 0);
		b.Init(data, PalettePrefab);

		StartCoroutine(MoveBlockFromSpawn(b, width));
	}

	IEnumerator MoveBlockFromSpawn(IGridOccupant block, int count) {
		for (int i = 0; i < count; i++) {
			yield return new WaitForSeconds(0.5f);
			GlobalGrid.TryMove(block, Vector2Int.right, true, true);
		}

		((Block)block).TaggedAsStillSpawning = false;
	}
}