using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleFlows : MonoBehaviour
{
    private bool flowsEnabled;
    // Start is called before the first frame update
    void Start()
    {
        flowsEnabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void toggle()
    {
        flowsEnabled = !flowsEnabled;
        FlowVisualization[] flows = Resources.FindObjectsOfTypeAll(typeof(FlowVisualization)) as FlowVisualization[];
        foreach(FlowVisualization flow in flows)
        {
            flow.gameObject.SetActive(flowsEnabled);
        }
    }
}
