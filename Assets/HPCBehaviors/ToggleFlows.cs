using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleFlows : MonoBehaviour
{
    private int modeEnabled;
    // Start is called before the first frame update
    void Start()
    {
        modeEnabled = 2;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void toggle()
    {
        modeEnabled = (modeEnabled + 1) % 3;
        bool flowsEnabled = true;
        bool hostsEnabled = true;
        if (modeEnabled == 0)
        {
            flowsEnabled = false;
        }
        else if (modeEnabled == 1)
        {
            hostsEnabled = false;
        }

        FlowVisualization[] flows = Resources.FindObjectsOfTypeAll(typeof(FlowVisualization)) as FlowVisualization[];
        foreach(FlowVisualization flow in flows)
        {
            flow.gameObject.SetActive(flowsEnabled);
        }

        ProxemicViewManagement[] proxes = Resources.FindObjectsOfTypeAll(typeof(ProxemicViewManagement)) as ProxemicViewManagement[];
        foreach(ProxemicViewManagement prox in proxes)
        {
            foreach (GameObject individual in prox.closeRangeObjects)
            {
                Renderer[] renderers = individual.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in renderers)
                {
                    rend.enabled = hostsEnabled;
                }
            }
            foreach (GameObject aggr in prox.farRangeObjects)
            {
                Renderer[] renderers = aggr.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in renderers)
                {
                    rend.enabled = hostsEnabled;
                }
            }
        }
    }
}
