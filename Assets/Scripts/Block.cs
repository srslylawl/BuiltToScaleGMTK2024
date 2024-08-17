using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Block : MonoBehaviour, IGridOccupant {


	public List<Vector2Int> Positions { get; private set; }
	private List<Vector2Int> PreRotationPositions;

	public Vector2Int CurrentPosition { get; set; }
	private Vector2Int interpolationOrigin { get; set; }

	private List<GameObject> paletteChildren = new();

	private bool isMoving;
	private float interpolationProgress;

	private Vector3 GridToWorldPos(Vector2Int gridPos) {
		return new Vector3(gridPos.x, 0, gridPos.y);
	}
	
	private float EaseExponential(float t) {
		return t >= 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
	}

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


	public void Init(BlockData blockData, GameObject palettePrefab) {
		var baseGridPos = new Vector2Int(Mathf.CeilToInt(transform.position.x), Mathf.CeilToInt(transform.position.z));
		CurrentPosition = baseGridPos;
		
		
		Positions = new List<Vector2Int>(blockData.GridPositions.Count);
		
		var randomColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

		paletteChildren.Capacity = blockData.GridPositions.Count;
		foreach (var blockDataGridPosition in blockData.GridPositions) {
			Positions.Add(blockDataGridPosition + baseGridPos);
			var go = Instantiate(palettePrefab, transform.position + new Vector3(blockDataGridPosition.x, 0, blockDataGridPosition.y), Quaternion.identity,
				transform);
			go.name = $"Child {blockDataGridPosition.x},{blockDataGridPosition.y}";
			go.GetComponent<MeshRenderer>().material.SetColor("_Color", randomColor);
			paletteChildren.Add(go);
		}
		
		//TODO spawn mesh
	}
	
	public void Rotate(List<Vector2Int> newPositions) {
		PreRotationPositions = new List<Vector2Int>(Positions);
		Positions = newPositions;

		for (int i = 0; i < Positions.Count; i++) {
			var offset = Positions[i] - CurrentPosition;
			paletteChildren[i].transform.localPosition = GridToWorldPos(offset);
		}

		transform.position = GridToWorldPos(CurrentPosition);
		// var base = 
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

	public void MoveTo(Vector2Int gridPosition) {
		var originOffset = gridPosition - CurrentPosition;
		for (var i = 0; i < Positions.Count; i++) {
			Positions[i] = Positions[i] + originOffset;
		}
		interpolationOrigin = CurrentPosition;
		CurrentPosition = gridPosition;
		PerformMove();
	}
}