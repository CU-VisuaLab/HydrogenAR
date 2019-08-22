using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashedLine : MonoBehaviour
{
    public GameObject endPoint;
    public Material dashedMaterial;
    public float width;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Initialize()
    {
        GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        line.GetComponent<Renderer>().material = dashedMaterial;
        line.transform.localScale = new Vector3(width, Vector3.Distance(transform.position, endPoint.transform.position) / 2, width);
        line.transform.position = (transform.position + endPoint.transform.position) / 2;
        line.transform.up = endPoint.transform.position - line.transform.position;
        line.transform.parent = transform;
    }
}
