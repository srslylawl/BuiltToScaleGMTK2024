using System;
using System.Collections.Generic;
using UnityEngine;

	public class SpawnButton : MonoBehaviour, IGridOccupant {
		public void Move(Vector2Int direction) {
			return;
		}

		public void MoveTo(Vector2Int gridPosition) {
			return;
		}

		public void Rotate(List<Vector2Int> newPositions, bool clockWise) {
			return;
		}

		public List<Vector2Int> Positions { get; }
		public Vector2Int CurrentPosition { get; set; }
		public void OnRegister() {
			// throw new NotImplementedException();
		}


		private void Start() {
			CurrentPosition = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.z));
		}
		
		
		
	}
