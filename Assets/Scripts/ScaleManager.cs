using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.UIElements;

public class ScaleManager : MonoBehaviour
{
    public Material scaleMat;
    private List<GameObject> scales = new List<GameObject>();
    private List<GameObject> floors = new List<GameObject>();
    private bool win = false;
    private HashSet<Transform> blocks = new HashSet<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        scales = GameObject.FindGameObjectsWithTag("Scale").ToList();
        floors = GameObject.FindGameObjectsWithTag("Floor").ToList();

        Debug.Log(scales.Count);

        Invoke("CheckForWin", 1f);
    }

    void CheckForWin()
    {
        win = true;
        foreach(GameObject scale in scales)
        {
            win = Physics.CheckSphere(scale.transform.position, 0.2f, LayerMask.GetMask("Block"));
            if (!win) break; 
        }

        if (win)
        {
            blocks.Clear();
            foreach (GameObject scale in scales)
            {
                blocks.Add(Physics.OverlapSphere(scale.transform.position, 0.2f, LayerMask.GetMask("Block"))[0].transform.parent);
            }

            for (int f = 0; f < floors.Count; f++)
            {
                Collider[] c = Physics.OverlapSphere(floors[f].transform.position, 0.2f, LayerMask.GetMask("Block"));
                if (c.Length > 0)
                {
                    if (blocks.Contains(c[0].transform.parent))
                    {
                        floors[f].name = "Scale";
                        floors[f].tag = "Scale";
                        floors[f].GetComponent<Renderer>().material = scaleMat;

                        scales.Add(floors[f]);
                        floors.RemoveAt(f);
                        f--;
                    }
                }
            }

            foreach (Transform block in blocks)
            {
                Destroy(block.GetComponent<BlockControls>().blockDestinationPoint.gameObject);
                Destroy(block.gameObject);
            }

            
        }

        Invoke("CheckForWin", 1f);
    }
}
