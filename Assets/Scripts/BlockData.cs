using System.Collections.Generic;
using UnityEngine;

	[CreateAssetMenu(fileName = "Data", menuName = "BLOCKDATA", order = 1)]
	public class BlockData : ScriptableObject {
		public GameObject Prefab;
		public List<Vector2Int> GridPositions;
	}
