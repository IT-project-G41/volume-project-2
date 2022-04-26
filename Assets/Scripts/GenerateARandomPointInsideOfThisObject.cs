using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(SphereCollider))]
public class GenerateARandomPointInsideOfThisObject : MonoBehaviour
{
    SphereCollider outerBounds;
    
    MeshCollider InnerObject;

    public GameObject Target;

    private void Start()
    {
        // get the colliders fromt the local space
        InnerObject = this.GetComponent<MeshCollider>();
        outerBounds = this.GetComponent<SphereCollider>();

        if (outerBounds == null)
        {
            outerBounds = this.GetComponent<SphereCollider>();
        }

        if (InnerObject == null)
        {
            InnerObject = this.GetComponent<MeshCollider>();
        }
    }

    public Vector3 CalculateNewRandomPosition()
    {
        int index = 0;
        Vector3 output;
        do
        {
            // find a new spot
            output = (Random.insideUnitSphere * outerBounds.radius) + outerBounds.center;
            index++;
        } while (!IsInsideOfObjectCheck.IsInCollider(InnerObject,
            outerBounds.transform.TransformPoint(output)) && index < 1000);

        if (index < 1000)
            Target.transform.position = this.transform.TransformPoint(output);

        Debug.Log(output);

        return this.transform.TransformPoint(output);
    }

    /// <summary>
    /// sets the position to a pre cacluated position
    /// </summary>
    /// <param name="TheWorldPosFor the object that is to be set"></param>
    public void CalculateNewRandomPosition(Vector3 worldPos)
    {
        this.Target.transform.position = worldPos;
    }
}
