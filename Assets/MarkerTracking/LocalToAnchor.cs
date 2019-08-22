using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalToAnchor : MonoBehaviour
{
    public GameObject localTo;
    public Vector3 offsetFrom;
    public Vector3 angleOffsetFrom;
    private bool initialized;

    // Start is called before the first frame update
    void Start()
    {
        initialized = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool isInitialized()
    {
        return initialized;
    }

    public void initialize()
    {
        Transform previousParent = transform.parent;
        transform.parent = localTo.transform;
        transform.localPosition = offsetFrom;
        transform.localEulerAngles = angleOffsetFrom;
        transform.parent = previousParent;
        initialized = true;
    }
}
