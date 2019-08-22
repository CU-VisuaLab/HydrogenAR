using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanWall : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        FlowVisualization[] flows = GetComponentsInChildren<FlowVisualization>();
        foreach(FlowVisualization flow in flows)
        {
            if (!flow.isInitialized()) flow.InitializeFlow(); 
        }
    }
}
