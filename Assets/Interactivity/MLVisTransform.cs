using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class MLVisTransform : MonoBehaviour
{
    private bool colliding;
    // Start is called before the first frame update
    void Start()
    {
        colliding = false;
    }

    // Update is called once per frame
    void Update()
    {
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.root != transform.root && other.transform.name != "Laser")
        {
            colliding = true;
        }
        else
        {
            Debug.Log(other.transform.name);
        }

    }
    void OnTriggerExit(Collider other)
    {
        colliding = false;
    }

    public bool getCollision()
    {
        return colliding;
    }
}
