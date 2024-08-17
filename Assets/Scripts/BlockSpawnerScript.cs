using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawnerScript : MonoBehaviour
{
    public int spawnTimer;
    public int spawnTimeVariance;

    public List<GameObject> blockList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        //Call BlockSpawn Methode after time
        Invoke("SpawnBlock", 0 + Random.Range(-spawnTimeVariance, spawnTimeVariance));
    }

    void SpawnBlock()
    {
        int blockNmb = Random.Range(0, blockList.Count - 1);
        GameObject block = Instantiate(blockList[blockNmb], this.transform.position, Quaternion.identity);
        block.GetComponent<BlockControls>().moveLeftOnBelt = false;

        //Recall BlockSpawn Methode after time
        Invoke("SpawnBlock", spawnTimer + Random.Range(-spawnTimeVariance, spawnTimeVariance));
    }
}
