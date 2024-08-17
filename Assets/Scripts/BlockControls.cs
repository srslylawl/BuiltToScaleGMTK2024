using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockControls : MonoBehaviour
{
    public bool moveLeftOnBelt;
    public float movementSpeed = 5f;
    public float rotationSpeed = 180f;
    public Transform blockDestinationPoint;
    public List<List<int>> blockLayout = new List<List<int>>();

    // Start is called before the first frame update
    void Start()
    {
        blockDestinationPoint.parent = null;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Move Block on grid to destination
        this.transform.position = Vector3.MoveTowards(this.transform.position, blockDestinationPoint.position, Time.fixedDeltaTime * movementSpeed);

        if (Vector3.Distance(this.transform.position, blockDestinationPoint.position) <= 0.05f)
        {
            if (CheckForBeltCollision() && this.gameObject.layer == LayerMask.NameToLayer("Block"))
            {
                if (moveLeftOnBelt) blockDestinationPoint.position += Vector3.left;
                else blockDestinationPoint.position += Vector3.right;
            }
        }
    }

    bool CheckForBeltCollision()
    {
        return Physics.CheckSphere(this.transform.position + new Vector3(0.5f, 0, 0.5f), 0.4f, LayerMask.GetMask("Transport"));
    }
}
