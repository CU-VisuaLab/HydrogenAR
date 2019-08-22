using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class InitializeHostMetrics : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void InitializeMetrics()
    {
        HostMetrics[] metrics = FindObjectsOfType<HostMetrics>();
        foreach (HostMetrics metric in metrics)
        {
            metric.InitializeListUsages();
        }
    }
}
