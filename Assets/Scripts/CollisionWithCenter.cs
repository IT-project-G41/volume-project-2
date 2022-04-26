using UnityEngine;

public class CollisionWithCenter : MonoBehaviour
{
    [SerializeField]
    CapsuleCollider collider;

    public Transform userObject;

    public Transform GuideOjbect;

    public LayerMask layerMask;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 posOnCly = collider.ClosestPoint(userObject.position);

        //GuideOjbect.position = posOnCly;

        RaycastHit hit;
        Ray ray = new Ray(userObject.position, posOnCly - userObject.position);
        if(Physics.Raycast(ray, out hit))
        {
            GuideOjbect.position = hit.point;
        }
        else
        {
            Debug.Log("insidebounds");
        }
        //Debug.DrawRay(ray.origin, ray.direction, Color.blue);
        //Debug.DrawLine(userObject.position, posOnCly, Color.green);
    }
}
