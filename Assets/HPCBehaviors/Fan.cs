using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fan : MonoBehaviour
{
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        FlowVisualization[] flows = transform.GetComponentsInChildren<FlowVisualization>(true);
        foreach (FlowVisualization flow in flows) flow.transform.parent = transform.parent;
        transform.localEulerAngles = transform.localEulerAngles + new Vector3(0, 0, -30 * speed * Time.deltaTime);
        foreach (FlowVisualization flow in flows) flow.transform.parent = flow.start.transform;
    }
}
