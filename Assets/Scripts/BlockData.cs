using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "BlockData", order = 1)]
public class BlockData : ScriptableObject {
	public GridLayoutData GridLayoutData;
	public List<GameObject> MeshVariants;
}