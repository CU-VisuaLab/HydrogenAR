using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoadMemoryUsage : MonoBehaviour
{
    public GameObject UsagePrefab;
    public TextAsset credentials;
    public GameObject HostRack;
    public int nodeMin;
    public int nodeMax;
    public string nodePrefix;

    private float usage;
    private GameObject usageObject;

    // Start is called before the first frame update
    void Start()
    {
        usageObject = Instantiate(UsagePrefab);
        usageObject.transform.parent = HostRack.transform;
        usageObject.transform.localEulerAngles = Vector3.zero;
        StartCoroutine(LoadAggregateData());
    }

    // Update is called once per frame
    void Update()
    {
        if (usage == 0f) usageObject.transform.localPosition = new Vector3(0, 0.01f * Mathf.Sin(5*Time.time), -0.6f);
        usageObject.transform.Find("Gear/default").localEulerAngles = new Vector3(0, -100 * Time.time * usage, 0);
    }

    private IEnumerator LoadAggregateData()
    {
        string nodeString = "";
        for (var i = nodeMin; i <= nodeMax; i++)
        {
            nodeString += (nodePrefix + i.ToString().PadLeft(2, '0'));
            if (i < nodeMax) nodeString += ",";
        }
        UnityWebRequest www = UnityWebRequest.Get("https://api.hpc.nrel.gov/esif/api/ts/metrics/hpc-eagle-ganglia/last?dataset=hpc-eagle-ganglia&from=2019-01-01T00:00:00&to=2019-12-31T00:00:00&hosts=" + nodeString + "&metrics=mem_free,mem_total");
        www.SetRequestHeader("AUTHORIZATION", "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(credentials.text)));
        yield return www.Send();
        List<RealtimeMetric> realtimeMetricList = JsonConvert.DeserializeObject<List<RealtimeMetric>>(www.downloadHandler.text);
        float free = 0;
        float total = 0;
        foreach (RealtimeMetric metric in realtimeMetricList)
        {
            if (metric.metric == "mem_free") free += (float)metric.value;
            else if (metric.metric == "mem_total") total += (float)metric.value;
        }
        usage = (total - free) / total;
        usageObject.transform.Find("Canvas/UsageText").GetComponent<Text>().text = "Usage: " + (100 * usage).ToString() + "%";
        usageObject.transform.localPosition = new Vector3(0, 0, -.6f);
    }
}
