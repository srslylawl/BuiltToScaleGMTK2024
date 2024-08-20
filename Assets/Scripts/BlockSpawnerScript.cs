using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlockSpawnerScript : MonoBehaviour {
	public float timeBetweenSpawns = 5f;
	// public int spawnTimeVariance;

	private Timer spawnTimer;

	private float lastSpawnTimer;

	private float nextSpawnTimer;

	[SerializeField] public GameObject PalettePrefab;
	[SerializeField] public List<BlockData> blockData = new();

	private bool readyToSpawn = true;
	private List<BlockData> currentRNGBucket = new();

	// Start is called before the first frame update
	void Start() {
		//Call BlockSpawn Methode after time
		// Invoke(nameof(SpawnBlock), 0 + Random.Range(-spawnTimeVariance, spawnTimeVariance));
		spawnTimer = new Timer(timeBetweenSpawns, true);
		nextSpawnTimer = timeBetweenSpawns;
	}

	private int blockNum = 0;
	private int spawns = 0;

	private void Update() {

		if (Input.GetKey(KeyCode.LeftShift)) {
			nextSpawnTimer = 1f;
		}
		
		if (spawnTimer && readyToSpawn) {
			SpawnBlock();
			spawns++;
			var spawnTime = nextSpawnTimer;
			lastSpawnTimer = spawnTime;
			spawnTimer = new Timer(spawnTime);
			nextSpawnTimer = Mathf.Max(timeBetweenSpawns - 0.05f * spawns, 1f);
			Debug.Log("Nextspawntimer: " +nextSpawnTimer);
		}
	}

	private void FillRngBucket() {
		foreach (var data in blockData) {
			currentRNGBucket.Add(data);
		}
	}

	void SpawnBlock() {
		if (currentRNGBucket.Count <= 1) {
			FillRngBucket();
		}
		int blockType = Random.Range(0, currentRNGBucket.Count);

		GameObject blockGameObject = new GameObject($"Block_{blockNum}_('{blockType}')");
		Block b = blockGameObject.AddComponent<Block>();
		var data = currentRNGBucket[blockType];

		currentRNGBucket.RemoveAt(blockType);

		//we need to move it so it leaves the area, so find how wide it is if we assume moving from left to right
		int width = 1;
		foreach (var dataGridPosition in data.GridLayoutData.GridPositions) {
			width = Math.Max(Math.Abs(dataGridPosition.x) + 1, width);
		}
		
		
		int meshVariant = Random.Range(0, data.MeshVariants.Count);

		blockGameObject.transform.position = transform.position - new Vector3(width, 0, 0);
		b.Init(data, PalettePrefab, meshVariant);

		StartCoroutine(MoveBlockFromSpawn(b, width));
	}

	IEnumerator MoveBlockFromSpawn(IGridOccupant block, int count) {
		readyToSpawn = false;
		float spawnTimer = 0;
		int remaining = count;
		while (true){
			if(!GlobalGameloop.I.gameOver) spawnTimer += Time.deltaTime;
			
			
			if (spawnTimer >= Math.Min(nextSpawnTimer / count, 1.0f)) {
				GlobalGrid.TryMove(block, Vector2Int.right, true, true);
				spawnTimer = 0;
				remaining--;

				if (remaining == 0) break;
			}

			yield return null;
		}

		((Block)block).TaggedAsStillSpawning = false;
		readyToSpawn = true;
	}
}