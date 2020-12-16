using UnityEngine;

public class DeadZone : MonoBehaviour
{
    public BoxCollider BoxCollider;

    void Start()
    {
        float tolerance = 0.6f;
        BoxCollider.size = new Vector3((
                transform.localScale.x - tolerance) / transform.localScale.x,
            (transform.localScale.y - tolerance) / transform.localScale.y,
            (transform.localScale.z - tolerance) / transform.localScale.z);
    }
}