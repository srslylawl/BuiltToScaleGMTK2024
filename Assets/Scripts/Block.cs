using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Block : MonoBehaviour, IGridOccupant {
	public List<Vector2Int> Positions { get; private set; }

	public Vector2Int CurrentPosition { get; set; }

	private Vector2Int interpolationOrigin { get; set; }

	private List<GameObject> paletteChildren = new();
	private GameObject paletteObject;

	private bool isMoving;
	private float interpolationProgress;

	private Color mainColor;

	public bool TaggedAsStillSpawning;

	private Vector3 GridToWorldPos(Vector2Int gridPos) {
		return new Vector3(gridPos.x, 0, gridPos.y);
	}

	private float EaseExponential(float t) {
		return t >= 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
	}

	private Color highlightColor = new Color(230f / 255f, 255 / 255f, 204 / 255f, 1.0f);

	public void SetHighlight(bool highlight) {
		foreach (var paletteChild in paletteChildren) {
			//can be destroyed already so we check for null
			if (!paletteChild) continue;
			var ren = paletteChild.GetComponent<MeshRenderer>();
			if (!ren) continue;
			ren.material.SetColor("_Color", highlight ? highlightColor : mainColor);
		}
	}
	
	public void OnRegister() {
		if (TaggedAsStillSpawning) return;

		foreach (var vector2Int in Positions) {
			if (GlobalGrid.InBounds(vector2Int)) return;
		}
		
		//if nothing in bounds, object falls down!
		Debug.Log("Object fell down!");
		GlobalGameloop.TriggerGameOver();
	}


	// private Vector2Int Forward = Vector2Int.right;

	// private void RotateToForwardAxis(Vector2Int direction) {
	// 	Forward = direction;
	// 	transform.rotation = Quaternion.Euler(new Vector3(0, Mathf.Atan2(Forward.x, Forward.y) * Mathf.Rad2Deg + 90, 0));
	// }

	private void Update() {
		if (isMoving) {
			interpolationProgress = Mathf.Clamp(interpolationProgress + Time.deltaTime * 5.0f, 0, 1);
			bool finished = Math.Abs(interpolationProgress - 1) < 0.00001f;
			var start = GridToWorldPos(interpolationOrigin);
			var end = GridToWorldPos(CurrentPosition);

			if (finished) {
				transform.position = end;
				interpolationOrigin = CurrentPosition;
				isMoving = false;
			}
			else {
				var t = EaseExponential(interpolationProgress);
				transform.position = Vector3.Lerp(start, end, t);
				return;
			}
		}
	}

	private void StartMoveInterpolation() {
		interpolationProgress = 0;
		isMoving = true;
	}


	public void Init(BlockData blockData, GameObject palettePrefab, int meshVariant) {
		var baseGridPos = new Vector2Int(Mathf.CeilToInt(transform.position.x), Mathf.CeilToInt(transform.position.z));
		CurrentPosition = baseGridPos;


		Positions = new List<Vector2Int>(blockData.GridLayoutData.GridPositions.Count);

		mainColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

		paletteChildren.Capacity = blockData.GridLayoutData.GridPositions.Count;
		foreach (var blockDataGridPosition in blockData.GridLayoutData.GridPositions) {
			Positions.Add(blockDataGridPosition + baseGridPos);
			var go = Instantiate(palettePrefab, transform.position + new Vector3(blockDataGridPosition.x, 0, blockDataGridPosition.y), Quaternion.identity,
				transform);
			go.name = $"Child {blockDataGridPosition.x},{blockDataGridPosition.y}";
			go.GetComponent<MeshRenderer>().material.SetColor("_Color", mainColor);
			paletteChildren.Add(go);
		}

		var mesh = blockData.MeshVariants[meshVariant];
		var paletteParent = new GameObject("RotParent");
		paletteParent.transform.parent = transform;
		paletteParent.transform.localPosition = new Vector3(0.5f, 0, 0.5f);
		paletteObject = Instantiate(mesh, paletteParent.transform, true);
		paletteObject.transform.localPosition = new Vector3(-0.5f, 0, -0.5f);


		TaggedAsStillSpawning = true;
		//TODO spawn mesh
	}

	public void Rotate(List<Vector2Int> newPositions, bool clockWise) {
		Positions = newPositions;
		float deg = clockWise ? 90 : -90;

		paletteObject.transform.parent.Rotate(new Vector3(0, 1.0f, 0), deg);

		for (int i = 0; i < Positions.Count; i++) {
			var offset = Positions[i] - CurrentPosition;
			paletteChildren[i].transform.localPosition = GridToWorldPos(offset);
		}

		transform.position = GridToWorldPos(CurrentPosition);
	}

	private void PerformMove() {
		// transform.position = new Vector3(CurrentPosition.x, 0, CurrentPosition.y);
		StartMoveInterpolation();
	}

	public void Move(Vector2Int direction) {
		for (var i = 0; i < Positions.Count; i++) {
			Positions[i] = Positions[i] + direction;
		}

		interpolationOrigin = CurrentPosition;
		CurrentPosition += direction;

		PerformMove();
	}

	private void OnDestroy() {
		GlobalGrid.UnregisterOccupant(this);
	}

}