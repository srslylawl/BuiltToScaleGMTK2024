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

    private Vector3 nextLocation;

    private void Start()
    {
        nextLocation = playerDestinationPoint.position;
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
                    if (!CheckForCollisionSphereAt(nextLocation))
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
                    return;
                }

                //Grab Block
                block = null;
                block = CheckForBlockInfront();
                if (block != null)
                {
                    block.parent.parent = this.transform;
                    holdingBlock = block.GetComponentInParent<BlockControls>().blockDestinationPoint;
                    holdingBlock.parent = this.transform;
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
            if (CheckForCollisionBox())
            {
                //Reverse rotation and destination back to before it was set
                if (rotatingRight) playerDestinationPoint.eulerAngles -= Vector3.up * 90;
                else playerDestinationPoint.eulerAngles += Vector3.up * 90;
                rotatingRight = !rotatingRight;
            }
        }
    }

    bool CheckForCollisionSphereAt(Vector3 location)
    {
        return Physics.CheckSphere(location, 0.2f);
    }

    bool CheckForCollisionBox()
    {
        return Physics.CheckBox(this.transform.position, Vector3.one * 0.4f, this.transform.rotation, layermask);
    }

    Transform CheckForBlockInfront()
    {
        Vector3 heightOffset = Vector3.down * 0.5f;
        Collider[] colliders = Physics.OverlapSphere(this.transform.position + this.transform.forward + heightOffset, 0.2f, LayerMask.GetMask("Block"));
        Debug.Log(colliders.Length);
        if(colliders.Length > 0)
        {
            return colliders[0].transform;
        }
        return null;
    }
}
