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

	public void MoveTo(Vector2Int gridPosition) {
		interpolationOrigin = CurrentPosition;
		CurrentPosition = gridPosition;
		StartMoveInterpolation();
	}

	public void Rotate(List<Vector2Int> newPositions) {
		//do nothing
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

		// ResetRotationLayout();
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
		if (GlobalGrid.GridOccupants.TryGetValue(CurrentPosition + Forward, out var toHighlight) && toHighlight is Block blockToHighlight) {
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

		if (isMoving) {
			interpolationProgress = Mathf.Clamp(interpolationProgress + Time.deltaTime * movementSpeed, 0, 1);
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

		bool hasRotationInput = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
		Vector2 playerInputAxis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		bool hasHorizontalInput = playerInputAxis.x != 0;
		bool hasVerticalInput = playerInputAxis.y != 0;

		bool isGrabbing = false;


		if (hasRotationInput && highlightedBlock) {
			if (GlobalGrid.TryRotate(highlightedBlock, CurrentPosition, false)) {
				return;
			}
		}

		if (hasHorizontalInput && Input.GetButtonDown("Horizontal")) {
			if (!isGrabbing) {
				bool clockWise = Math.Abs(playerInputAxis.x - 1) < 0.00001f;
				var newFwd = GlobalGrid.RotatePoint(Forward, Vector2Int.zero, clockWise);
				var newRight = GlobalGrid.RotatePoint(Right, Vector2Int.zero, clockWise);

				Forward = newFwd;
				Right = newRight;

				float deg = clockWise ? 90 : -90;
				transform.Rotate(new Vector3(0, 1.0f, 0), deg);
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

		if (hasVerticalInput) {
			var direction = Math.Sign(playerInputAxis.y) * Forward;
			if (GlobalGrid.TryMove(this, direction, false)) {
				return;
			}

			// if(hasRotationInput && GlobalGrid.GridOccupants.TryGetValue(CurrentPosition + direction, out var occupant) && occupant != this) {
			// 	if (GlobalGrid.TryRotate(occupant, CurrentPosition, true)) {
			// 		Debug.Log($"Player rotates: {occupant}.");
			// 		return;
			// 	}
			// }
		}

		// //Apply next position of viable and not blocked
		// if (nextPosition != playerDestinationPoint.position) {
		// 	if (!CheckForCollisionSphereTowards(nextPosition - playerDestinationPoint.position)) {
		// 		playerDestinationPoint.position = nextPosition;
		// 	}
		// 	else {
		// 		nextPosition = playerDestinationPoint.position;
		// 	}
		// }

		// //Set rotation and rotation direction
		// if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.25f) {
		// 	playerDestinationPoint.eulerAngles += Vector3.up * (Mathf.Abs(Input.GetAxis("Horizontal")) / Input.GetAxis("Horizontal") * 90);
		//
		// 	if (Input.GetAxis("Horizontal") > 0) rotatingRight = true;
		// 	else rotatingRight = false;
		// }

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

// Update is called once per frame
	void FixedUpdate() {
		//Apply InputChanges in FixedUpdate

		//Move on grid to destination
		// this.transform.position = Vector3.MoveTowards(this.transform.position, playerDestinationPoint.position, Time.fixedDeltaTime * movementSpeed);

		// //Rotate
		// if (Vector3.Distance(this.transform.eulerAngles, playerDestinationPoint.eulerAngles) >= 0.1) {
		// 	//Apply rotation
		// 	if (rotatingRight)
		// 		this.transform.RotateAround(playerDestinationPoint.position, Vector3.up, Time.fixedDeltaTime * rotationSpeed);
		// 	else
		// 		this.transform.RotateAround(playerDestinationPoint.position, Vector3.down, Time.fixedDeltaTime * rotationSpeed);
		//
		// 	//Check for collision during rotation
		// 	if (CheckRotationCollisions()) {
		// 		//Reverse rotation and destination back to before it was set
		// 		if (rotatingRight) playerDestinationPoint.eulerAngles -= Vector3.up * 90;
		// 		else playerDestinationPoint.eulerAngles += Vector3.up * 90;
		// 		rotatingRight = !rotatingRight;
		// 	}
		// }
	}

	// bool CheckForCollisionSphereTowards(Vector3 direction) {
	// 	bool collision = false;
	//
	// 	Vector3 heightOffset = Vector3.down * 0.5f;
	// 	for (int x = 0; x < playerBlockLayout.GetLength(0); x++) {
	// 		for (int z = 0; z < playerBlockLayout.GetLength(1); z++) {
	// 			if (playerBlockLayout[x, z] == 1) {
	// 				collision = Physics.CheckSphere(
	// 					// playerDestinationPoint.position + direction + heightOffset + this.transform.right * (x - 3) + this.transform.forward * (z - 3), 0.2f,
	// 					// layermask);
	// 			}
	//
	// 			if (collision) {
	// 				return true;
	// 			}
	// 		}
	// 	}
	//
	// 	return false;
	// }

	void ResetRotationLayout() {
		for (int x = 0; x < playerBlockLayout.GetLength(0); x++) {
			for (int z = 0; z < playerBlockLayout.GetLength(1); z++) {
				playerBlockLayout[x, z] = 0;
			}
		}

		playerBlockLayout[3, 3] = 1;
	}

	void SetRotationLayout(Transform blockToCompare) {
		Transform compareBlock = null;
		for (int x = 0; x < playerBlockLayout.GetLength(0); x++) {
			for (int z = 0; z < playerBlockLayout.GetLength(1); z++) {
				compareBlock = CheckForBlockAt(this.transform.position + this.transform.right * (x - 3) + this.transform.forward * (z - 3));

				if (compareBlock != null) {
					if (compareBlock.parent == blockToCompare) {
						playerBlockLayout[x, z] = 1;
					}
				}
			}
		}

		string output;

		for (int x = 0; x < playerBlockLayout.GetLength(0); x++) {
			output = "";
			for (int z = 0; z < playerBlockLayout.GetLength(1); z++) {
				output += playerBlockLayout[x, z] + " ";
			}
		}
	}

	bool CheckRotationCollisions() {
		bool colliding = false;

		for (int x = 0; x < playerBlockLayout.GetLength(0); x++) {
			for (int z = 0; z < playerBlockLayout.GetLength(1); z++) {
				if (playerBlockLayout[x, z] == 1) {
					colliding = CheckForCollisionBoxAt(this.transform.position + this.transform.right * (x - 3) + this.transform.forward * (z - 3));
					if (colliding) return true;
				}
			}
		}

		return false;
	}

	bool CheckForCollisionBoxAt(Vector3 location) {
		return Physics.CheckBox(location, Vector3.one * 0.4f, this.transform.rotation, layermask);
	}

	void SetLayer(GameObject gaObjToSet, int lm) {
		gaObjToSet.layer = lm;
		foreach (Transform child in gaObjToSet.transform) {
			if (child != null) {
				child.gameObject.layer = lm;
			}
		}
	}

	Transform CheckForBlockAt(Vector3 location) {
		Vector3 heightOffset = Vector3.down * 0.5f;
		Collider[] colliders = Physics.OverlapSphere(location + heightOffset, 0.2f, LayerMask.GetMask("Block"));
		if (colliders.Length > 0) {
			return colliders[0].transform;
		}

		return null;
	}
}