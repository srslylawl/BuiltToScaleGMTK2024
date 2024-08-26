using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour, IGridOccupant {
	public float movementSpeed = 5f;
	public float rotationSpeed = 180f;
	private bool rotatingRight = true;

	private bool rememberSpace = false;

	// public Transform playerDestinationPoint;
	private Transform block = null;
	private Transform holdingBlock;
	public LayerMask layermask;

	private int[,] playerBlockLayout = new int[7, 7];

	
	public Vector2Int CurrentPosition { get; set; } //where we actually are on the grid


	private Vector2Int interpolationOrigin; //where we are interpolating from
	private float interpolationProgress;
	private bool isMoving;


	public List<Vector2Int> Positions => new() { CurrentPosition };

	private Vector2Int Forward = Vector2Int.left;
	private Vector2Int Right = Vector2Int.up;


	public void Move(Vector2Int direction) {
		interpolationOrigin = CurrentPosition;
		CurrentPosition += direction;
		StartMoveInterpolation();
	}
	
	
	public void OnRegister() {
		if (!GlobalGrid.InBounds(CurrentPosition)) {
			Debug.Log("Player is off grid! Game over!");
			GlobalGameloop.TriggerGameOver();
		}
	}

	public void Rotate(List<Vector2Int> newPositions, bool clockWise) {
		RotateCharacter(clockWise);
	}

	Vector3 GridToWorldPos(Vector2Int gridPos) {
		return new Vector3(gridPos.x + 0.5f, 0.5f, gridPos.y + 0.5f);
	}

	private void Start() {
		CurrentPosition = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.z));
		interpolationOrigin = CurrentPosition;

		if (!GlobalGrid.TryMove(this, Vector2Int.zero)) {
			Debug.LogException(new Exception("Unable to register player position on spawn - overlaps?"));
		}
	}

	private void StartMoveInterpolation() {
		interpolationProgress = 0;
		isMoving = true;
	}

	private float EaseExponential(float t) {
		return t >= 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
	}

	private Block highlightedBlock;

	private void HandleHighlighting() {
		if (GlobalGrid.GridOccupants.TryGetValue(CurrentPosition + Forward, out var toHighlight) &&
			toHighlight is Block { TaggedAsStillSpawning: false } blockToHighlight) {
			if (!highlightedBlock) {
				blockToHighlight.SetHighlight(true);
				highlightedBlock = blockToHighlight;
				return;
			}

			if (highlightedBlock == blockToHighlight) return;

			highlightedBlock.SetHighlight(false);
			highlightedBlock = blockToHighlight;
			blockToHighlight.SetHighlight(true);
		}

		if (highlightedBlock) {
			highlightedBlock.SetHighlight(false);
			highlightedBlock = null;
		}
	}

	private void Update() {
		//Read Inputs in Update

		// bool isRotating = Vector3.Distance(this.transform.eulerAngles, playerDestinationPoint.eulerAngles) <= 0.1;

		HandleHighlighting();

		//Movement interpolation
		if (isMoving) {
			interpolationProgress = Mathf.Clamp(interpolationProgress + Time.deltaTime * movementSpeed, 0, 1);
			bool finished = Math.Abs(interpolationProgress - 1) < 0.00001f;
			var start = GridToWorldPos(interpolationOrigin);
			var end = GridToWorldPos(CurrentPosition);
			
			Shader.SetGlobalVector("_PlayerPos", new Vector4(transform.position.x - 0.5f, transform.position.z - 0.5f, 0, 0));

			if (finished) {
				transform.position = end;
				interpolationOrigin = CurrentPosition;
				isMoving = false;
			}
			else {
				var t = EaseExponential(interpolationProgress);
				transform.position = Vector3.Lerp(start, end, t);
				// return;
			}
		}

		if (GlobalGameloop.I.gameOver) return;

		// bool hasRotationInput = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
		Vector2 playerInputAxis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		bool hasHorizontalInput = playerInputAxis.x != 0;
		bool hasVerticalInput = playerInputAxis.y != 0;

		bool isGrabbing = highlightedBlock && Input.GetKey(KeyCode.Space);

		bool cclockWiseInput = Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Q);

		bool clockwiseInput = Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.E);

		bool rotationalInput = cclockWiseInput || clockwiseInput;

		bool clockwiseRotation = rotationalInput && clockwiseInput;

		if (rotationalInput && highlightedBlock) {
			if (GlobalGrid.TryRotate(highlightedBlock, CurrentPosition, clockwiseRotation)) {
				RotateCharacter(clockwiseRotation);
				return;
			}
		}

		if (hasHorizontalInput && Input.GetButtonDown("Horizontal")) {
			// bool clockWise = Math.Abs(playerInputAxis.x - 1) < 0.00001f;
			var direction = Math.Sign(playerInputAxis.x) * Vector2Int.right;
			if (!isGrabbing) {
				// RotateCharacter(clockWise);
				// Forward = 
				RotateToForwardAxis(direction);
				if (GlobalGrid.TryMove(this, direction, false)) {
					return;
				}
			}
			else {
				//is grabbing
				List<IGridOccupant> occupants = new() { this, highlightedBlock };
				if (GlobalGrid.TryMove(occupants, direction, true)) {
					return;
				}
				// if (GlobalGrid.TryRotate(highlightedBlock, CurrentPosition, clockWise)) {
				// 	RotateCharacter(clockWise);
				// 	return;
				// }
			}

			// var direction = Math.Sign(playerInputAxis.x) * Vector2Int.right;
			// if (GlobalGrid.TryMove(this, direction, false)) {
			// 	return;
			// }

			// if(hasRotationInput && GlobalGrid.GridOccupants.TryGetValue(CurrentPosition + direction, out var occupant) && occupant != this) {
			// 	if (GlobalGrid.TryRotate(occupant, CurrentPosition, true)) {
			// 		Debug.Log($"Player rotates: {occupant}.");
			// 		return;
			// 	}
			// }
		}

		if (hasVerticalInput && Input.GetButtonDown("Vertical")) {
			var direction = Math.Sign(playerInputAxis.y) * Vector2Int.up;
			// var direction = Math.Sign(playerInputAxis.y) * Forward;

			if (isGrabbing) {
				List<IGridOccupant> occupants = new() { this, highlightedBlock };
				if (GlobalGrid.TryMove(occupants, direction, true)) {
					return;
				}
			}
			else {
				// direction = Math.Sign(playerInputAxis.y) * Vector2Int.up;
				RotateToForwardAxis(direction);
				if (GlobalGrid.TryMove(this, direction, false)) {
					return;
				}
			}
		}

		//Checking for Block in front and parenting to player
		// if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || rememberSpace) {
		// 	rememberSpace = false;
		//
		// 	//Release Block
		// 	if (holdingBlock != null) {
		// 		block.parent.parent = null;
		// 		holdingBlock.parent = null;
		// 		holdingBlock = null;
		//
		// 		SetLayer(block.parent.gameObject, LayerMask.NameToLayer("Block"));
		//
		// 		ResetRotationLayout();
		//
		// 		return;
		// 	}
		//
		// 	//Grab Block
		// 	block = null;
		// 	block = CheckForBlockAt(this.transform.position + this.transform.forward);
		// 	if (block != null) {
		// 		block.parent.parent = this.transform;
		// 		holdingBlock = block.GetComponentInParent<BlockControls>().blockDestinationPoint;
		// 		holdingBlock.parent = this.transform;
		//
		// 		SetRotationLayout(block.parent);
		//
		// 		SetLayer(block.parent.gameObject, LayerMask.NameToLayer("Player"));
		// 	}
		// }
		// else {
		// 	if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) rememberSpace = true;
		// }
	}

	private void RotateCharacter(bool clockwise) {
		var newFwd = GlobalGrid.RotatePoint(Forward, Vector2Int.zero, clockwise);
		var newRight = GlobalGrid.RotatePoint(Right, Vector2Int.zero, clockwise);

		Forward = newFwd;
		Right = newRight;

		float deg = clockwise ? 90 : -90;
		transform.Rotate(new Vector3(0, 1.0f, 0), deg);
	}

	private void RotateToForwardAxis(Vector2Int direction) {
		Forward = direction;
		(Right.x, Right.y) = (Forward.y, -Forward.x);
		transform.rotation = Quaternion.Euler(new Vector3(0, Mathf.Atan2(Forward.x, Forward.y) * Mathf.Rad2Deg + 90, 0));
	}

	public void OnDestroy() {
		GlobalGrid.UnregisterOccupant(this);
	}

	private void OnDrawGizmos() {
		//Draw grid bounds
		Gizmos.color = Color.magenta;
		//bot left corner to top left corner
		Gizmos.DrawLine(new Vector3(-GlobalGrid.BoundsSize, 0, -GlobalGrid.BoundsSize), new Vector3(-GlobalGrid.BoundsSize, 0, GlobalGrid.BoundsSize));
		//top left to top right
		Gizmos.DrawLine(new Vector3(-GlobalGrid.BoundsSize, 0, GlobalGrid.BoundsSize), new Vector3(GlobalGrid.BoundsSize, 0, GlobalGrid.BoundsSize));
		//top right to bot right
		Gizmos.DrawLine(new Vector3(GlobalGrid.BoundsSize, 0, GlobalGrid.BoundsSize), new Vector3(GlobalGrid.BoundsSize, 0, -GlobalGrid.BoundsSize));
		//bot right to bot left
		Gizmos.DrawLine(new Vector3(GlobalGrid.BoundsSize, 0, -GlobalGrid.BoundsSize), new Vector3(-GlobalGrid.BoundsSize, 0, -GlobalGrid.BoundsSize));

		//Gizmos.color = new Color(0, 1.0f, 0, 0.5f);
		
		//render cube for every occupied grid cell
		// foreach (var (position, value) in GlobalGrid.GridOccupants) {
		// 	// Gizmos.DrawCube(new Vector3(position.x + 0.5f, 0, position.y + 0.5f), Vector3.one);
		// }
	}
}