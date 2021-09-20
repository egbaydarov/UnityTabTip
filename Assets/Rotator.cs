using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField]
    Transform target;

    [SerializeField]
    Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        lookAt();
    }

    void lookAt()
    {
        Vector3 delta = target.position - transform.position + offset;

        Quaternion rotation = Quaternion.LookRotation(delta);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, 90);
    }
}
