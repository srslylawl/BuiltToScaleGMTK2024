using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class ScaleManager : MonoBehaviour
{
    public GameObject Floor;
    public GameObject Scale;
    public GameObject Canvas;
    public GameObject ScoreNum;
    public GameObject PerfectPlay;
    public List<GameObject> scoresList = new();
    private bool win = false;
    public GridLayoutData floorLayoutActive;
    private GridLayoutData scaleLayoutActive;
    private List<GameObject> tileList = new();
    private List<GameObject> oldTileList = new();
    public List<GridLayoutData> scaleLayouts = new();
    private HashSet<Transform> blocks = new HashSet<Transform>();
    private Dictionary<Transform, int> blocksScoreCount = new Dictionary<Transform, int>();

    private Vector2Int thisPosOffset;

    // Start is called before the first frame update
    void Start()
    {
        thisPosOffset = new Vector2Int((int)this.transform.position.x, (int)this.transform.position.z);

        floorLayoutActive.GridPositions.Clear();
        for(int x = 0; x < 7; x++)
        {
            for (int z = 0; z < 7; z++)
            {
                floorLayoutActive.GridPositions.Add(new Vector2Int(x, z));
            }
        }

        scaleLayoutActive = scaleLayouts[UnityEngine.Random.Range(0, scaleLayouts.Count)];
        
        foreach (Vector2Int v2 in scaleLayoutActive.GridPositions)
        {
            tileList.Add(Instantiate(Scale, new Vector3(v2.x, 0f, v2.y) + this.transform.position, Quaternion.Euler(0, 0, 0), this.transform));
            floorLayoutActive.GridPositions.Remove(v2);
        }

        foreach (Vector2Int v2 in floorLayoutActive.GridPositions)
        {
            tileList.Add(Instantiate(Floor, new Vector3(v2.x, 0f, v2.y) + this.transform.position, Quaternion.Euler(0, 0, 0), this.transform));
        }

        Invoke("CheckForWin", 1f);
    }

    void CheckForWin()
    {
        win = true;

        foreach (Vector2Int v2 in scaleLayoutActive.GridPositions)
        {
            GlobalGrid.GridOccupants.TryGetValue(v2 + thisPosOffset, out var occupant);
            win = occupant != null && !((MonoBehaviour)occupant).CompareTag("Player");
            if (!win) break;
        }


        if (win)
        {
            int roundScore = 0;

            scoresList.Clear();

            blocks.Clear();
            foreach (Vector2Int v2 in scaleLayoutActive.GridPositions)
            {
                GlobalGrid.GridOccupants.TryGetValue(v2 + thisPosOffset, out var occupant);
                Transform oc = ((MonoBehaviour)occupant).transform;
                blocks.Add(oc);
                if (blocksScoreCount.ContainsKey(oc) && occupant != null) blocksScoreCount[oc]++;
                else blocksScoreCount.Add(oc, 1);

                if (occupant != null)
                {
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(new Vector3(v2.x + thisPosOffset.x, 0, v2.y + thisPosOffset.y));
                    GameObject sn = Instantiate(ScoreNum, screenPos, Quaternion.identity, Canvas.transform);
                    sn.GetComponent<TextMeshProUGUI>().text = "+" + blocksScoreCount[oc] * 10;
                    sn.SetActive(false);
                    scoresList.Add(sn);
                }
            }

            foreach (int value in blocksScoreCount.Values)
            {
                roundScore += (value * (value + 1) / 2) * 10;
            }

            int scoreTemp = roundScore;
            foreach (Vector2Int v2 in floorLayoutActive.GridPositions)
            {
                GlobalGrid.GridOccupants.TryGetValue(v2 + thisPosOffset, out var occupant);
                if (occupant != null && !((MonoBehaviour)occupant).CompareTag("Player"))
                {
                    roundScore -= 20;
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(new Vector3(v2.x + thisPosOffset.x, 0, v2.y + thisPosOffset.y));
                    GameObject sn = Instantiate(ScoreNum, screenPos, Quaternion.identity, Canvas.transform);
                    sn.GetComponent<TextMeshProUGUI>().text = "-20";
                    sn.SetActive(false);
                    scoresList.Add(sn);
                }
            }

            if (roundScore == scoreTemp)
            {
                roundScore += 200;
                Vector3 screenPos = new Vector3(Screen.width / 2, Screen.height / 2, 0);
                GameObject sn = Instantiate(ScoreNum, screenPos, Quaternion.identity, Canvas.transform);
            }

            int invokeCounter = 1;
            StartCoroutine(SpawnScores(scoresList));

            //Do Score Stuff
            GlobalGameloop.FinishRound(roundScore);

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

    IEnumerator SpawnScores(List<GameObject> scores)
    {
        foreach(GameObject s in scores)
        {
            s.SetActive(true);
            yield return new WaitForSeconds(0.05f);
        }
        
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

        scaleLayoutActive = scaleLayouts[UnityEngine.Random.Range(0, scaleLayouts.Count)];

        foreach (Vector2Int v2 in scaleLayoutActive.GridPositions)
        {
            GameObject obj = Instantiate(Scale, new Vector3(v2.x, 0f, v2.y) + this.transform.position, Quaternion.identity, this.transform);
            tileList.Add(obj);
            floorLayoutActive.GridPositions.Remove(v2);
        }

        foreach (Vector2Int v2 in floorLayoutActive.GridPositions)
        {
            GameObject obj = Instantiate(Floor, new Vector3(v2.x, 0f, v2.y) + this.transform.position, Quaternion.identity, this.transform);
            tileList.Add(obj);
        }

        foreach (GameObject obj in oldTileList) StartCoroutine(Flip(obj, obj.transform.eulerAngles.x));
        //foreach (GameObject obj in tileList) StartCoroutine(Flip(obj, obj.transform.eulerAngles.x));
    }

    IEnumerator Flip(GameObject flip, float startX)
    {
        for (float rotation = 0f; rotation <= 180; rotation += 360f * Time.deltaTime)
        {
            flip.transform.eulerAngles = new Vector3(startX + rotation, 0, 0);
            yield return null;
        }

        flip.transform.eulerAngles = new Vector3(startX + 180f, 0, 0);
        if (startX == 0) Destroy(flip);

    }
}
