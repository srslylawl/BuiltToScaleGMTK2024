using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.FilePathAttribute;

public class PlayerControls : MonoBehaviour
{

    public float movementSpeed = 5f;
    public float rotationSpeed = 180f;
    private bool rotatingRight = true;
    public Transform playerDestinationPoint;
    private Transform block = null;
    private Transform holdingBlock;
    public LayerMask layermask;

    private int[,] playerBlockLayout = new int[7, 7];
    private Vector3 nextLocation;

    private void Start()
    {
        nextLocation = playerDestinationPoint.position;

        ResetRotationLayout();

    }

    private void Update()
    {
        //Read Inputs in Update
        if (Vector3.Distance(this.transform.position, playerDestinationPoint.position) <= 0.05f && Vector3.Distance(this.transform.eulerAngles, playerDestinationPoint.eulerAngles) <= 0.1)
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                //Set next possible position
                if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.25f)
                {
                    nextLocation = playerDestinationPoint.position + Vector3.right * (Mathf.Abs(Input.GetAxis("Horizontal")) / Input.GetAxis("Horizontal"));
                }
                else if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.25f)
                {
                    nextLocation = playerDestinationPoint.position + Vector3.forward * (Mathf.Abs(Input.GetAxis("Vertical")) / Input.GetAxis("Vertical"));
                }

                //Apply next position of viable and not blocked
                if (nextLocation != playerDestinationPoint.position)
                {
                    if (!CheckForCollisionSphereTowards(nextLocation - playerDestinationPoint.position))
                    {
                        playerDestinationPoint.position = nextLocation;
                    }
                    else
                    {
                        nextLocation = playerDestinationPoint.position;
                    }
                }
            }
            else
            {
                //Set rotation and rotation direction
                if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.25f)
                {
                    playerDestinationPoint.eulerAngles += Vector3.up * (Mathf.Abs(Input.GetAxis("Horizontal")) / Input.GetAxis("Horizontal") * 90);

                    if (Input.GetAxis("Horizontal") > 0) rotatingRight = true;
                    else rotatingRight = false;
                }
            }

            //Checking for Block in front and parenting to player
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                //Release Block
                if (holdingBlock != null)
                {
                    block.parent.parent = null;
                    holdingBlock.parent = null;
                    holdingBlock = null;

                    SetLayer(block.parent.gameObject, LayerMask.NameToLayer("Block"));

                    ResetRotationLayout();

                    return;
                }

                //Grab Block
                block = null;
                block = CheckForBlockAt(this.transform.position + this.transform.forward);
                if (block != null)
                {
                    block.parent.parent = this.transform;
                    holdingBlock = block.GetComponentInParent<BlockControls>().blockDestinationPoint;
                    holdingBlock.parent = this.transform;

                    SetRotationLayout(block.parent);

                    SetLayer(block.parent.gameObject, LayerMask.NameToLayer("Player"));
                }
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Apply InputChanges in FixedUpdate

        //Move on grid to destination
        this.transform.position = Vector3.MoveTowards(this.transform.position, playerDestinationPoint.position, Time.fixedDeltaTime * movementSpeed);

        //Rotate
        if(Vector3.Distance(this.transform.eulerAngles, playerDestinationPoint.eulerAngles) >= 0.1)
        {
            //Apply rotation
            if(rotatingRight)
                this.transform.RotateAround(playerDestinationPoint.position, Vector3.up, Time.fixedDeltaTime * rotationSpeed);
            else
                this.transform.RotateAround(playerDestinationPoint.position, Vector3.down, Time.fixedDeltaTime * rotationSpeed);

            //Check for collision during rotation
            if (CheckRotationCollisions())
            {
                //Reverse rotation and destination back to before it was set
                if (rotatingRight) playerDestinationPoint.eulerAngles -= Vector3.up * 90;
                else playerDestinationPoint.eulerAngles += Vector3.up * 90;
                rotatingRight = !rotatingRight;
            }
        }
    }

    bool CheckForCollisionSphereTowards(Vector3 direction)
    {
        bool collision = false;

        for (int x = 0; x < playerBlockLayout.GetLength(0); x++)
        {
            for (int z = 0; z < playerBlockLayout.GetLength(1); z++)
            {
                if (playerBlockLayout[x, z] == 1)
                {
                    collision = Physics.CheckSphere(playerDestinationPoint.position + direction + this.transform.right * (x - 3) + this.transform.forward * (z - 3), 0.2f, layermask);
                }
                if (collision)
                {
                    Debug.Log(x + " " + z);
                    return true;
                }
            }
        }
        return false;
    }

    void ResetRotationLayout()
    {
        for (int x = 0; x < playerBlockLayout.GetLength(0); x++)
        {
            for (int z = 0; z < playerBlockLayout.GetLength(1); z++)
            {
                playerBlockLayout[x, z] = 0;
            }
        }

        playerBlockLayout[3, 3] = 1;
    }

    void SetRotationLayout(Transform blockToCompare)
    {
        Transform compareBlock = null; 
        for (int x = 0; x < playerBlockLayout.GetLength(0); x++)
        {
            for (int z = 0; z < playerBlockLayout.GetLength(1); z++)
            {
                compareBlock = CheckForBlockAt(this.transform.position + this.transform.right * (x - 3) + this.transform.forward * (z - 3));

                if (compareBlock != null)
                {
                    if (compareBlock.parent == blockToCompare)
                    {
                        playerBlockLayout[x, z] = 1;
                    }
                }
            }
        }

        string output;

        for (int x = 0; x < playerBlockLayout.GetLength(0); x++)
        {
            output = "";
            for (int z = 0; z < playerBlockLayout.GetLength(1); z++)
            {
                output += playerBlockLayout[x,z] + " ";
            }
            Debug.Log(output);
        }
    }

    bool CheckRotationCollisions()
    {
        bool colliding = false;

        for (int x = 0; x < playerBlockLayout.GetLength(0); x++)
        {
            for (int z = 0; z < playerBlockLayout.GetLength(1); z++)
            {
                if (playerBlockLayout[x,z] == 1)
                {
                    colliding = CheckForCollisionBoxAt(this.transform.position + this.transform.right * (x - 3) + this.transform.forward * (z - 3));
                    if (colliding) return true;
                }
            }
        }

        return false;
    }

    bool CheckForCollisionBoxAt(Vector3 location)
    {
        return Physics.CheckBox(location, Vector3.one * 0.4f, this.transform.rotation, layermask);
    }

    void SetLayer(GameObject gaObjToSet, int lm)
    {
        gaObjToSet.layer = lm;
        foreach(Transform child in gaObjToSet.transform)
        {
            if (child != null) { child.gameObject.layer = lm;}
        }
    }

    Transform CheckForBlockAt(Vector3 location)
    {
        Vector3 heightOffset = Vector3.down * 0.5f;
        Collider[] colliders = Physics.OverlapSphere(location + heightOffset, 0.2f, LayerMask.GetMask("Block"));
        if(colliders.Length > 0)
        {
            return colliders[0].transform;
        }
        return null;
    }
}
