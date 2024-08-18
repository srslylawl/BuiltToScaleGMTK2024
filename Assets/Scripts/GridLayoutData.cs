using System.Collections.Generic;
using UnityEngine;

	[CreateAssetMenu(fileName = "Data", menuName = "GridLayoutData", order = 1)]
	public class GridLayoutData : ScriptableObject {
		public List<Vector2Int> GridPositions;
	}
