using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UsageSummary : AggregateSummary
{
    public GameObject UsagePrefab;
    private GameObject usageObject;
    private int timestampIndex;
    private float usage;
    private int incrementor;
    // Start is called before the first frame update
    void Awake()
    {
        timestampIndex = -1;
        incrementor = 1;
        usageObject = Instantiate(UsagePrefab);
        usageObject.transform.localEulerAngles = Vector3.zero;
        usageObject.transform.parent = transform;
        usageObject.transform.localPosition = new Vector3(0, 0.6f, .6f);
    }

    // Update is called once per frame
    void Update()
    {
        usageObject.transform.Find("Gear/default").localEulerAngles += new Vector3(0, -100 * Time.deltaTime * usage, 0);
    }

    public void SetActive(bool activation)
    {
        usageObject.SetActive(activation);
    }

    void CycleTimestamp()
    {
        timestampIndex+=incrementor;
        float value = 0;
        HostMetrics[] hostMetrics = transform.parent.GetComponentsInChildren<HostMetrics>();
        foreach (HostMetrics metrics in hostMetrics)
        {
            value += metrics.GetAggregateValue(timestampIndex) / hostMetrics.Length;
        }
        updateUsagePrefab(value);
        if (hostMetrics[0].GetMetricType() == "Usage")
        {
            foreach(HostMetrics metrics in hostMetrics)
            {
                if (metrics.WIMPiece != null)
                    metrics.WIMPiece.GetComponent<Renderer>().material.color = Color.Lerp(Color.white, Color.black, value);
            }
            transform.parent.GetComponentInChildren<HostMetrics>().WIMPiece.GetComponent<WIMPiece>().setValue((int)(100 * usage));
        }
    }
    public override void UpdatePlaybackSpeed(float frequency)
    {
        CancelInvoke();
        incrementor = (int)Mathf.Sign(frequency);
        InvokeRepeating("CycleTimestamp", 0, Mathf.Abs(frequency));
    }
    private void updateUsagePrefab(float value)
    {
        usage = value;
        usageObject.transform.Find("Canvas/UsageText").GetComponent<Text>().text = "Usage: " + (100 * usage).ToString() + "%";
    }
}
