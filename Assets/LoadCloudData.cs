using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DxR;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UI;
using System;
using Json;
using System.Json;
using System.Text;
//using System.Runtime.Serialization;
//using System.Web.Script.Serialization;
public class LoadCloudData : MonoBehaviour
{
    public string authUrl;
    public string dataUrl;
    public string APIEndpoint;
    public TextAsset credentials;
    public Vis[] visObjects;
    public string[] metrics;
    private List<string> hostnameMapping;
    private int visUpdates;
    
    // Start is called before the first frame update
    void Awake()
    {
        hostnameMapping = new List<string>();
        hostnameMapping.Add("cbb10.lower");
        hostnameMapping.Add("cbb11.lower");
        hostnameMapping.Add("cbb10.upper");
        hostnameMapping.Add("cbb11.upper");
        hostnameMapping.Add("cbb12.upper");
    }

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator GetData(string uri)
    {
        WWWForm form = new WWWForm();
        form.AddField("filename", "db9e16c4-46f0-477c-94b2-b08d87eec353");
        UnityWebRequest www = UnityWebRequest.Get(uri);
        www.SetRequestHeader("AUTHORIZATION", "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(credentials.text)));
        yield return www.Send();
    }
    IEnumerator PopulateVis(string uri, Vis vis)
    {
        using (UnityWebRequest getRequest = UnityWebRequest.Get(uri))
        {
            getRequest.SetRequestHeader("AUTHORIZATION", "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(credentials.text)));

            yield return getRequest.SendWebRequest();
            string jsonText = getRequest.downloadHandler.text;
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            List<MetricTimeSeries> series = JsonConvert.DeserializeObject<List<MetricTimeSeries>>(jsonText, settings);
            string processedJsonString = ProcessTimeSeriesData(series);
            vis.UpdateVisSpecsFromTextSpecs(processedJsonString);
            visUpdates++;
            if (visUpdates == visObjects.Length) GameObject.Find("DebugText").GetComponent<Text>().text = "";
        }
    }
    // Really just for debugging purposes--using a particular string instead of result of HTTP request
    /*private void GetDataFromString(string str, Vis vis)
    {
        TestData testData = JsonConvert.DeserializeObject<TestData>(str);
        ProcessedTestData[] processedDataArray = ProcessTestData(testData);
        string processedJsonString = JsonConvert.SerializeObject(processedDataArray);
        vis.UpdateVisSpecsFromTextSpecs(processedJsonString);
    }
    private ProcessedTestData[] ProcessTestData(TestData testData)
    {
        ProcessedTestData[] processedData = new ProcessedTestData[testData.data_3d.GetLength(0)];
        for (var i = 0; i < testData.data_3d.GetLength(0); i++) 
        {
            ProcessedTestData processedPoint = new ProcessedTestData();
            processedPoint.x = testData.data_3d[i, 0];
            processedPoint.y = testData.data_3d[i, 1];
            processedPoint.z = testData.data_3d[i, 2];
            processedData[i] = processedPoint;
        }
        return processedData;
    }*/
    private string ProcessTimeSeriesData(List<MetricTimeSeries> series)
    {
        string processedString = "[";
        for (var i = 0; i < series.Count; i++)
        {
            processedString += ("{\"time\": " + "\"" + series[i].timestamp + "\"");
            processedString += (",\"value\": " + series[i].value.avg + "}");
            if (i < series.Count - 1) processedString += ",";
        }
        processedString += "]";
        return processedString;
    }
    private IEnumerator RealtimeMetric(string metric)
    {
        UnityWebRequest www = UnityWebRequest.Get("https://api.hpc.nrel.gov/esif/api/ts/metrics/hpc-stonefish/last?dataset=hpc-stonefish&from=2019-01-01T00:00:00&to=2019-12-31T00:00:00&hosts=*&metrics=" + metric);
        www.SetRequestHeader("AUTHORIZATION", "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(credentials.text)));
        yield return www.Send();
        List<RealtimeMetric> realtimeMetricList = JsonConvert.DeserializeObject<List<RealtimeMetric>>(www.downloadHandler.text);
        Debug.Log(metric + " " +www.downloadHandler.text);

        for (var i = 0; i < FindObjectOfType<VisSpatialMappingTracking>().AnchoredObjects.Length; i++)
        {
            if (FindObjectOfType<VisSpatialMappingTracking>().AnchoredObjects[i].transform.position == Vector3.zero) continue;
            Debug.Log(FindObjectOfType<VisSpatialMappingTracking>().AnchoredObjects[i].transform.name + ": " + FindObjectOfType<VisSpatialMappingTracking>().AnchoredObjects[i].transform.Find("Realtime/Canvas/Text"));
            FindObjectOfType<VisSpatialMappingTracking>().AnchoredObjects[i].transform.Find("Realtime/Canvas/Text").GetComponent<Text>().text =
                "Host: " + realtimeMetricList[i].host + "\nMetric: " + realtimeMetricList[i].metric + "\nValue: " + 
                realtimeMetricList[i].value.ToString();
        }
    }
    public void UpdateWithMetric(string metric)
    {
        GameObject.Find("DebugText").GetComponent<Text>().text = "Loading Data...";
        visUpdates = 0;
        StartCoroutine(RealtimeMetric(metric));
        for (var i = 0; i < visObjects.Length; i++)
        {
            StartCoroutine(PopulateVis("https://api.hpc.nrel.gov/esif/api/ts/metrics/hpc-stonefish?dataset=hpc-stonefish&from=2019-01-01T00:00:00&to=2019-01-10T00:00:00&hosts=" + hostnameMapping[i] + "&window=4h&metrics=" + metric, visObjects[i]));
        }
    }
}
