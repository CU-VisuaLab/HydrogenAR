using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateMetric : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void useMetric(string metric)
    {

        HostMetrics[] hosts = FindObjectsOfType<HostMetrics>();
        foreach (HostMetrics host in hosts)
        {
            host.UpdateMetric(metric);
        }
    }

    public void initializeButtons(GameObject menuObject)
    {
        Button[] buttons = menuObject.transform.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            if (button.transform.name.ToLower() != "back")
            {
                button.onClick.AddListener(() => useMetric(button.transform.name));
            }
        }
    }
}
