using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoadFlowData : MonoBehaviour
{
    public FlowVisualization flowComponent;
    public TextAsset credentials;
    public string cdu_name;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RealtimeFlowData());
    }

    // Update is called once per frame
    void Update()
    {

    }
    private IEnumerator RealtimeFlowData()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://api.hpc.nrel.gov/esif/api/ts/metrics/hpc-xcdu/last?dataset=hpc-xcdu&from=2019-01-01T00:00:00&to=2019-12-31T00:00:00&hosts=" + cdu_name + "&metrics=fac_out_t_disp,fac_in_t_disp");
        www.SetRequestHeader("AUTHORIZATION", "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(credentials.text)));
        yield return www.Send();
        List<RealtimeMetric> realtimeMetricList = JsonConvert.DeserializeObject<List<RealtimeMetric>>(www.downloadHandler.text);
        foreach (RealtimeMetric metric in realtimeMetricList)
        {
            if ((metric.metric == "fac_out_t_disp" && flowComponent.start.transform.parent.name.ToUpper().Contains("CDU") ||
                (metric.metric == "fac_in_t_disp" && !flowComponent.start.transform.parent.name.ToUpper().Contains("CDU"))))
            {
                flowComponent.flowRate = 0.5f;
                flowComponent.flowVolume = 0.05f;
                flowComponent.flowTemperature = (float)metric.value;
            }
            flowComponent.InitializeFlow();
            //if (metric.metric == "fac_in_t_disp") Debug.Log("In: " + metric.value);
        }
        /*
        for (var i = 0; i < FindObjectOfType<VisSpatialMappingTracking>().AnchoredObjects.Length; i++)
        {
            if (FindObjectOfType<VisSpatialMappingTracking>().AnchoredObjects[i].transform.position == Vector3.zero) continue;
            Debug.Log(FindObjectOfType<VisSpatialMappingTracking>().AnchoredObjects[i].transform.name + ": " + FindObjectOfType<VisSpatialMappingTracking>().AnchoredObjects[i].transform.Find("Realtime/Canvas/Text"));
            FindObjectOfType<VisSpatialMappingTracking>().AnchoredObjects[i].transform.Find("Realtime/Canvas/Text").GetComponent<Text>().text =
                "Host: " + realtimeMetricList[i].host + "\nMetric: " + realtimeMetricList[i].metric + "\nValue: " +
                realtimeMetricList[i].value.ToString();
        }*/
    }
}
