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

public static class GlobalGrid {
	public static Dictionary<Vector2Int, IGridOccupant> GridOccupants = new();

	public const int BoundsSize = 10;

	//can handily pass allowoutofbounds to then just return true
	private static bool InBounds(Vector2Int position, bool allowOutOfBounds = false) {
		return allowOutOfBounds || Math.Abs(position.x) < BoundsSize && Math.Abs(position.y) < BoundsSize;
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

	public static Vector2Int RotatePoint(Vector2Int point, Vector2Int origin, bool clockWise) {
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
		List<Vector2Int> rotated = new(occupant.Positions.AsReadOnly());

		for (int i = 0; i < rotated.Count; i++) {
			var previous = rotated[i];
			var rot = RotatePoint(previous, origin, clockWise);
			rotated[i] = rot;
		}

		//check if rotation has space
		foreach (var pos in rotated) {
			if (GridOccupants.TryGetValue(pos, out var nextOccupant) && nextOccupant != occupant || !InBounds(pos))
				return false; //if blocked and not by self, pass
		}

		occupant.CurrentPosition = RotatePoint(occupant.CurrentPosition, origin, clockWise);

		UnregisterOccupant(occupant);
		occupant.Rotate(rotated);
		RegisterOccupant(occupant);

		return true;
	}

	public static bool TryMove(IGridOccupant occupant, Vector2Int direction, bool pushAllowed = true, bool allowOutOfBounds = false) {
		//basically check for each occupant that would be affected if they can move recursively
		List<IGridOccupant> occupants = new(1) { occupant };
		//keep track of occupants so we dont loop endlessly
		if (pushAllowed) {
			return TryMoveRecursive(occupant, occupants, direction, allowOutOfBounds:true);
		}

		foreach (var oPos in occupant.Positions) {
			Vector2Int desiredPos = oPos + direction;
			if (!InBounds(desiredPos, allowOutOfBounds)) return false;
			//check if position is free
			if (GridOccupants.TryGetValue(desiredPos, out var nextOccupant) && occupant != nextOccupant)
				return false; //if occupied and not us then we cant move
		}

		//all cells are free and we can move, lets do it
		PerformMovement(occupant, direction);

		return true;
	}

	//move a list of occupants at the same time, such as player+what they are grabbing
	public static bool TryMove(List<IGridOccupant> occupants, Vector2Int direction, bool pushAllowed = true, bool allowOutOfBounds = false) {
		List<Vector2Int> joinedPositions = new();
		foreach (var gridOccupant in occupants) {
			foreach (var gridOccupantPosition in gridOccupant.Positions) {
				joinedPositions.Add(gridOccupantPosition);
			}
		}

		List<IGridOccupant> trackedOccupants = new List<IGridOccupant>(occupants.AsReadOnly());
		foreach (var oPos in joinedPositions) {
			Vector2Int desiredPos = oPos + direction;
			if (!InBounds(desiredPos, allowOutOfBounds)) return false;
			//check if position is free
			if (GridOccupants.TryGetValue(desiredPos, out var nextOccupant) && !trackedOccupants.Contains(nextOccupant) && pushAllowed) {
				//not free, can we push it?
				trackedOccupants.Add(nextOccupant);

				if (!TryMoveRecursive(nextOccupant, trackedOccupants, direction)) {
					return false;
				}
			}
		}

		//unregister all
		foreach (var gridOccupant in occupants) {
			UnregisterOccupant(gridOccupant);
		}

		//move
		foreach (var gridOccupant in occupants) {
			gridOccupant.Move(direction);
		}

		//register again
		foreach (var gridOccupant in occupants) {
			RegisterOccupant(gridOccupant);
		}

		return true;
	}

	public static void UnregisterOccupant(IGridOccupant occupant) {
		foreach (var occupantPosition in occupant.Positions) {
			GridOccupants.Remove(occupantPosition);
		}
	}


	public static void RegisterOccupant(IGridOccupant occupant) {
		foreach (var occupantPosition in occupant.Positions) {
			GridOccupants[occupantPosition] = occupant;
		}
	}


	private static bool TryMoveRecursive(IGridOccupant occupant, List<IGridOccupant> trackedOccupants, Vector2Int direction, bool allowPush = true, bool allowOutOfBounds = false) {
		foreach (var oPos in occupant.Positions) {
			Vector2Int desiredPos = oPos + direction;
			
			if (!InBounds(desiredPos, allowOutOfBounds)) return false;
			//check if position is free
			if (!GridOccupants.TryGetValue(desiredPos, out var nextOccupant) || occupant == nextOccupant) continue; //either free or ourself

			//occupied by this guy, see if we are already tracking it
			if (trackedOccupants.Contains(nextOccupant)) continue; //yes, then ignore

			//we are not tracking it, so tell it to move if possible
			trackedOccupants.Add(nextOccupant);
			if (!allowPush || !TryMoveRecursive(nextOccupant, trackedOccupants, direction, allowPush, allowOutOfBounds)) {
				//it cant move, abort
				return false;
			}
		}

		//all cells are free and we can move, lets do it
		PerformMovement(occupant, direction);
		return true;
	}

	private static void PerformMovement(IGridOccupant occupant, Vector2Int direction) {
		UnregisterOccupant(occupant);
		occupant.Move(direction);
		RegisterOccupant(occupant);
	}
}