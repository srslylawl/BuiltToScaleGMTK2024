	using System;
	using System.Collections.Generic;
	using UnityEngine;


	public interface IGridOccupant {
		public void Move(Vector2Int direction);
		public void MoveTo(Vector2Int gridPosition);
		public void Rotate(List<Vector2Int> newPositions);

		public List<Vector2Int> Positions { get; }
		public Vector2Int CurrentPosition { get; set; }
	}

	public class GlobalGrid : MonoBehaviour {
		public static GlobalGrid I;

		public static Dictionary<Vector2Int, IGridOccupant> GridOccupants = new();
		
		
		private void Awake() {
			if (I) throw new Exception("Duplicate Grid Instance");
			
			I = this;
		}

		public static bool PositionFree(Vector2Int position) {
			return !GridOccupants.ContainsKey(position);
		}

		public static bool PositionsFree(in List<Vector2Int> positions) {
			foreach (var pos in positions) {
				if (GridOccupants.ContainsKey(pos)) return false;
			}

			return true;
		}

		private static Vector2Int RotatePoint(Vector2Int point, Vector2Int origin, bool clockWise) {
			var temp = point - origin;
			if (clockWise) {
				(temp.x, temp.y) = (temp.y, -temp.x);
			}
			else {
				(temp.x, temp.y) = (-temp.y, temp.x);
			}
			return temp + origin;
		}

		public static bool TryRotate(IGridOccupant occupant, Vector2Int origin, bool clockWise) {
			List<Vector2Int> rotated = new (occupant.Positions.AsReadOnly());

			for (int i = 0; i < rotated.Count; i++) {
				var previous = rotated[i];
				var rot = RotatePoint(previous, origin, clockWise);
				rotated[i] = rot;
			}
			
			//check if rotation has space
			foreach (var pos in rotated) {
				if (GridOccupants.TryGetValue(pos, out var nextOccupant) && nextOccupant != occupant) return false; //if blocked and not by self, pass
			}

			occupant.CurrentPosition = RotatePoint(occupant.CurrentPosition, origin, clockWise);
			
			foreach (var occupantPosition in occupant.Positions) {
				GridOccupants.Remove(occupantPosition);
			}
			occupant.Rotate(rotated);
			// Debug.Log($"Moving {occupant} to {direction}");
			//register new
			foreach (var occupantPosition in occupant.Positions) {
				GridOccupants[occupantPosition] = occupant;
			}
			
			return true;
		}
		
		public static bool TryMove(IGridOccupant occupant, Vector2Int direction, bool pushAllowed = true) {
			//basically check for each occupant that would be affected if they can move recursively
			List<IGridOccupant> occupants = new(1) { occupant };
			//keep track of occupants so we dont loop endlessly
			if (pushAllowed) {
				return TryMoveRecursive(occupant, occupants, direction);
			}
			
			foreach (var oPos in occupant.Positions) {
				Vector2Int desiredPos = oPos + direction;
				//check if position is free
				if (GridOccupants.TryGetValue(desiredPos, out var nextOccupant) && occupant != nextOccupant) return false; //if occupied and not us then we cant move
			}
			
			//all cells are free and we can move, lets do it
			PerformMovement(occupant, direction);

			return true;
		}
		

		private static bool TryMoveRecursive(IGridOccupant occupant, List<IGridOccupant> trackedOccupants, Vector2Int direction) {
			foreach (var oPos in occupant.Positions) {
				Vector2Int desiredPos = oPos + direction;
				//check if position is free
				if (!GridOccupants.TryGetValue(desiredPos, out var nextOccupant) || occupant == nextOccupant) continue; //either free or ourself
				
				//occupied by this guy, see if we are already tracking it
				if (trackedOccupants.Contains(nextOccupant)) continue; //yes, then ignore
				
				//we are not tracking it, so tell it to move if possible
				trackedOccupants.Add(nextOccupant);
				if (!TryMoveRecursive(nextOccupant, trackedOccupants, direction)) {
					//it cant move, abort
					return false;
				}
			}
			
			//all cells are free and we can move, lets do it
			PerformMovement(occupant, direction);
			return true;
		}

		private static void PerformMovement(IGridOccupant occupant, Vector2Int direction) {
			//unregister all current tiles
			foreach (var occupantPosition in occupant.Positions) {
				GridOccupants.Remove(occupantPosition);
			}
			occupant.Move(direction);
			// Debug.Log($"Moving {occupant} to {direction}");
			//register new
			foreach (var occupantPosition in occupant.Positions) {
				GridOccupants[occupantPosition] = occupant;
			}
		}
		
	}
