using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationSelectionBehaviour : MonoBehaviour
{
    [SerializeField]
    Transform TargetPoint;

    [SerializeField]
    Transform PositionPoint;

    [SerializeField]
    Transform UserRoationObject;

    [SerializeField]
    float ZOffset;

    [SerializeField]
    float MaxRandomOffSet = 0;

    [SerializeField]
    LineRenderer _LineRenderer;
    
    Vector3 noise;

    [SerializeField]
    Vector3 startLocalPos;

    private void OnEnable()
    {
        noise = new Vector3(
            Random.Range(-MaxRandomOffSet, MaxRandomOffSet),
            Random.Range(-MaxRandomOffSet, MaxRandomOffSet),
            Random.Range(-MaxRandomOffSet, MaxRandomOffSet)
            );
        // TODO move to start once done with testing
        PositionPoint.LookAt(TargetPoint.position + noise);

        Vector3 rNoise = new Vector3(
            Random.Range(-MaxRandomOffSet, MaxRandomOffSet),
            Random.Range(-MaxRandomOffSet, MaxRandomOffSet),
            Random.Range(-MaxRandomOffSet, MaxRandomOffSet)
            );

        UserRoationObject.localPosition = startLocalPos + rNoise;
    }

    // Update is called once per frame
    void Update()
    {
        // if the user position has moved below a accepted point put it back 
        UserRoationObject.localPosition = FixRoationObjectIfNeeded(0);

        // draw the line between the two objects the user is interacting with
        this.DrawLine();
    }

    private Vector3 FixRoationObjectIfNeeded(float minDistance = 0)
    {
        Vector3 newPos = UserRoationObject.localPosition;

        //if (UserRoationObject.localPosition.x < minDistance)
        //{
        //    newPos = new Vector3(minDistance, newPos.y, newPos.z);
        //}

        //if (UserRoationObject.localPosition.y > minDistance)
        //{
        //    newPos = new Vector3(newPos.x, minDistance, newPos.z);
        //}

        float zoffSet = minDistance + calcZOffSet(ZOffset);
        if (UserRoationObject.localPosition.z > zoffSet)
        {
            newPos = new Vector3(newPos.x, newPos.y, zoffSet);
        }

        return newPos;
    }

    private float calcZOffSet(float offSet)
    {
        // get the highest value the user has traveled
        float xIsGreaterThanY = Mathf.Max(Mathf.Abs(UserRoationObject.localPosition.x), Mathf.Abs(UserRoationObject.localPosition.y));

        return xIsGreaterThanY * offSet;
    }

    private void DrawLine()
    {
        _LineRenderer.SetPosition(0, PositionPoint.position);
        _LineRenderer.SetPosition(1, UserRoationObject.position);
    }
}
