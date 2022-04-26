using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMagneticCollision : MonoBehaviour
{
    public Transform DistancePoint;

    public Transform CollisionPoint;

    BaryCentricDistance distanceObj = null;

    public MeshFilter filter;

    // Start is called before the first frame update
    void OnEnable()
    {
        if (filter == null)
        {
            filter = this.GetComponent<MeshFilter>();
        }

        distanceObj = new BaryCentricDistance(filter);
    }

    // Update is called once per frame
    void Update()
    {
        BaryCentricDistance.Result r = distanceObj.GetClosestTriangleAndPoint(this.DistancePoint.position);

        //Debug.Log(r.distance);

        CollisionPoint.position = r.closestPoint;
    }
}
