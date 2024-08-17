using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.UIElements;

public class ScaleManager : MonoBehaviour
{
    public GameObject Floor;
    public GameObject Scale;
    public bool win = false;
    public BlockData floorLayoutActive;
    private BlockData scaleLayoutActive;
    private List<GameObject> tileList = new();
    private List<GameObject> oldTileList = new();
    public List<BlockData> scaleLayouts = new();
    private HashSet<Transform> blocks = new HashSet<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        floorLayoutActive.GridPositions.Clear();
        for(int x = 0; x < 7; x++)
        {
            for (int z = 0; z < 7; z++)
            {
                floorLayoutActive.GridPositions.Add(new Vector2Int(x, z));
            }
        }

        scaleLayoutActive = scaleLayouts[Random.Range(0, scaleLayouts.Count)];
        
        foreach (Vector2Int v2 in scaleLayoutActive.GridPositions)
        {
            tileList.Add(Instantiate(Scale, new Vector3(v2.x, 0.01f, v2.y) + new Vector3(1f, 0, 1f) / 2f, Quaternion.Euler(90, 0, 0), this.transform));
            floorLayoutActive.GridPositions.Remove(v2);
        }

        foreach (Vector2Int v2 in floorLayoutActive.GridPositions)
        {
            tileList.Add(Instantiate(Floor, new Vector3(v2.x, 0.01f, v2.y) + new Vector3(1f, 0, 1f) / 2f, Quaternion.Euler(90, 0, 0), this.transform));
        }

        Invoke("CheckForWin", 1f);
    }

    void CheckForWin()
    {
        win = true;
        

        //Not working grid positions confusing. Andreas not around to be asked!
        foreach (Vector2Int v2 in scaleLayoutActive.GridPositions)
        {
            win = !GlobalGrid.PositionFree(v2);
            if (!win) break;
        }
        

        if (win)
        {
            foreach (Vector2Int v2 in scaleLayoutActive.GridPositions)
            {
                GlobalGrid.GridOccupants.TryGetValue(v2, out var occupant);
                blocks.Add(((MonoBehaviour)occupant).transform);
            }

            //Do Score Stuff
            GlobalGameloop.IncreaseScore(123);
            GlobalGameloop.ResetTimer();

            //Destroy Blocks for now. crane them later
            foreach (Transform block in blocks)
            {
                Destroy(block.gameObject);
            }

            //Reset Scale
            ResetScales();
        }

        Invoke("CheckForWin", 1f);
    }

    void ResetScales()
    {
        win = false;

        oldTileList.Clear();
        foreach (GameObject obj in tileList) oldTileList.Add(obj);
        tileList.Clear();

        floorLayoutActive.GridPositions.Clear();
        for (int x = 0; x < 7; x++)
        {
            for (int z = 0; z < 7; z++)
            {
                floorLayoutActive.GridPositions.Add(new Vector2Int(x, z));
            }
        }

        scaleLayoutActive = scaleLayouts[Random.Range(0, scaleLayouts.Count)];

        foreach (Vector2Int v2 in scaleLayoutActive.GridPositions)
        {
            tileList.Add(Instantiate(Scale, new Vector3(v2.x, 0.01f, v2.y) + new Vector3(1f, 0, 1f) / 2f, Quaternion.Euler(90, 0, 0), this.transform));
            floorLayoutActive.GridPositions.Remove(v2);
        }

        foreach (Vector2Int v2 in floorLayoutActive.GridPositions)
        {
            tileList.Add(Instantiate(Floor, new Vector3(v2.x, 0.01f, v2.y) + new Vector3(1f, 0, 1f) / 2f, Quaternion.Euler(90, 0, 0), this.transform));
        }

        foreach (GameObject obj in oldTileList) StartCoroutine(Flip(obj, obj.transform.eulerAngles.x));
        foreach (GameObject obj in tileList) StartCoroutine(Flip(obj, obj.transform.eulerAngles.x));
    }

    IEnumerator Flip(GameObject flip, float startX)
    {
        Debug.Log(startX);
        for (float rotation = 0f; rotation <= 180; rotation += 360f*Time.deltaTime)
        {
            flip.transform.eulerAngles = new Vector3(startX + rotation, 0, 0);
            yield return null;
        }

        flip.transform.eulerAngles = new Vector3(startX + 180f, 0, 0);
        if(startX + 180f == 270f) Destroy(flip);

    }
}
