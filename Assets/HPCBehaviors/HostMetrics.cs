using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HostMetrics : MonoBehaviour
{
    public float MIN_TEMPERATURE = 35;
    public float MAX_TEMPERATURE = 50;

    private Dictionary<string, List<Tuple<string, float>>> temperatureValues;
    private Dictionary<string, LinkedList<Tuple<string, float>>> usageValues;
    private Dictionary<string, LinkedListNode<Tuple<string, float>>> currentUsageNodes;
    private List<GameObject> hostGrid;

    public enum MetricType { Temperature, Usage, Jobs };
    private MetricType displayMetric;

    private float fastforwardRatio = 0.5f;
    [Tooltip("How large of a window to aggregate over")]
    public float timestepDelta = 1;

    public GameObject WIMPiece;

    /*public TextAsset valuesList;
    public TextAsset hostsList;
    public TextAsset timeStampList;*/
    public float mem_total;
    // Start is called before the first frame update
    void Start()
    {
        InitializeGrid();
        InitializeRandomTemperatures();
        //if (hostsList == null)
        //    InitializeRandomUsages();
        //else
        InitializeListUsages();
        UpdateMetric("Usage");
        UpdatePlaybackSpeed(fastforwardRatio);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void InitializeGrid()
    {
        string[,] csvGrid = CSVReader.SplitCsvGrid(GetComponent<CSVReader>().csvFile.text);
        int rows = csvGrid.GetLength(1) - 1;
        int cols = csvGrid.GetLength(0) - 2;
        hostGrid = new List<GameObject>();

        float hostWidth = GetComponentInChildren<HostHeatmap>().hostWidth;
        float hostHeight = GetComponentInChildren<HostHeatmap>().hostHeight;
        float totalWidth = GetComponentInChildren<HostHeatmap>().totalWidth;
        float totalHeight = GetComponentInChildren<HostHeatmap>().totalHeight;

        for (var row = 1; row < rows; row++)
        {
            GameObject gridObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gridObject.transform.localScale = new Vector3(hostWidth, hostHeight, 0.01f); 
            gridObject.transform.parent = GetComponentInChildren<HostHeatmap>().transform;
            gridObject.transform.localPosition = new Vector3(totalWidth - hostWidth * int.Parse(csvGrid[1, row]), totalHeight - hostHeight * int.Parse(csvGrid[0, row]), 0) - new Vector3(totalWidth / 2, -totalHeight / 2, 0);
            gridObject.transform.name = transform.name + "n" + csvGrid[2, row];
            gridObject.transform.localEulerAngles = Vector3.zero;
            gridObject.AddComponent<DetailHover>();
            hostGrid.Add(gridObject);
        }
    }
    private void InitializeRandomTemperatures()
    {
        System.Random rnd = new System.Random();
        temperatureValues = new Dictionary<string, List<Tuple<string, float>>>();

        string rackFolder = transform.name.Split('i')[0];
        string rowFolder = "i" + transform.name.Split('i')[1];
        TextAsset hostsList = Resources.Load("RackData/" + rackFolder + "/" + rowFolder + "/" + "hosts_" + transform.name) as TextAsset;
        TextAsset timeStampList = Resources.Load("RackData/timestamps") as TextAsset;
        temperatureValues = new Dictionary<string, List<Tuple<string, float>>>();
        foreach (GameObject host in hostGrid)
        {
            temperatureValues.Add(host.transform.name, new List<Tuple<string, float>>());
        }
        string[] timestampArray = timeStampList.text.Split(',');
        string[] hostsArray = hostsList.text.Split(',');
        for (var i = 0; i < timestampArray.Length; i++)
        {
            if (temperatureValues.ContainsKey(hostsArray[i]))
                temperatureValues[hostsArray[i]].Add(Tuple.Create(timestampArray[i], (float)rnd.NextDouble() * (MAX_TEMPERATURE - MIN_TEMPERATURE) + MIN_TEMPERATURE));
        }
    }
    /*private void InitializeRandomUsages()
    {
        System.Random rnd = new System.Random();
        usageValues = new Dictionary<string, List<Tuple<string, float>>>();
        foreach (GameObject host in hostGrid)
        {
            List<Tuple<string, float>> usageList = new List<Tuple<string, float>>();
            for (var i = 0; i < 24; i++)
            {
                usageList.Add(Tuple.Create(i.ToString().PadLeft(2, '0') + ":00:00", (float)rnd.NextDouble()));
            }
            usageValues.Add(host.transform.name, usageList);
        }
    }
    private void InitializeCSVUsages()
    {

        usageValues = new Dictionary<string, List<Tuple<string, float>>>();
        string[,] metricCsvGrid = CSVReader.SplitCsvGrid(transform.parent.GetComponent<CSVReader>().csvFile.text);
        int rows = metricCsvGrid.GetLength(1) - 1;
        int cols = metricCsvGrid.GetLength(0) - 2;
        foreach (GameObject host in hostGrid)
        {
            usageValues.Add(host.transform.name, new List<Tuple<string, float>>());
        }
        for (var row = 1; row < rows; row++)
        {
            if (usageValues.ContainsKey(metricCsvGrid[1,row])) usageValues[metricCsvGrid[1, row]].Add(Tuple.Create(metricCsvGrid[0, row], 1 - float.Parse(metricCsvGrid[2, row]) / float.Parse(metricCsvGrid[3, row])));
        }
    }*/

    public void InitializeListUsages()
    {
        string rackFolder = transform.name.Split('i')[0];
        string rowFolder = "i" + transform.name.Split('i')[1];
        TextAsset valuesList = Resources.Load("RackData/" + rackFolder + "/" + rowFolder + "/" + "mem_free_" + transform.name) as TextAsset;
        TextAsset hostsList = Resources.Load("RackData/" + rackFolder + "/" + rowFolder + "/" + "hosts_" + transform.name) as TextAsset;
        TextAsset timeStampList = Resources.Load("RackData/timestamps") as TextAsset;
        usageValues = new Dictionary<string, LinkedList<Tuple<string, float>>>();
        currentUsageNodes = new Dictionary<string, LinkedListNode<Tuple<string, float>>>();
        foreach (GameObject host in hostGrid)
        {
            usageValues.Add(host.transform.name, new LinkedList<Tuple<string, float>>());
        }
        string[] valuesArray = valuesList.text.Split(',');
        string[] hostsArray = hostsList.text.Split(',');
        string[] timestampArray = timeStampList.text.Split(',');
        for (var i = 0; i < timestampArray.Length; i++)
        {
            if (usageValues.ContainsKey(hostsArray[i]))
                usageValues[hostsArray[i]].AddLast(Tuple.Create(timestampArray[i], 1f - float.Parse(valuesArray[i]) / mem_total));
        }
        foreach (GameObject host in hostGrid)
        {
            currentUsageNodes.Add(host.transform.name, usageValues[host.transform.name].Last);
        }
    }

    public List<GameObject> GetHostGrid()
    {
        return hostGrid;
    }

    public void UpdateMetric(string metric)
    {
        displayMetric = (MetricType)Enum.Parse(typeof(MetricType), metric);
        transform.parent.GetComponentInChildren<UsageSummary>().SetActive(false);
        transform.parent.GetComponentInChildren<TemperatureSummary>().SetActive(false);
        if (displayMetric == MetricType.Temperature)
        {
            transform.parent.GetComponentInChildren<TemperatureSummary>().SetActive(true);
        }
        else if (displayMetric == MetricType.Usage)
        {
            transform.parent.GetComponentInChildren<UsageSummary>().SetActive(true);
        }
        else if (displayMetric == MetricType.Jobs)
        {
            transform.parent.GetComponentInChildren<JobSummary>().SetActive(true);
        }

        WIMFocusObject[] focuses = FindObjectsOfType<WIMFocusObject>();
        foreach (WIMFocusObject focus in focuses)
        {
            focus.setMetric(metric);
        }
    }
    public float GetIndividualValue(string hostname, int timestampIndex)
    {
        if (displayMetric == MetricType.Temperature)
        {
            while (timestampIndex < 0) timestampIndex += temperatureValues[hostGrid[0].transform.name].Count;
            while (timestampIndex >= temperatureValues[hostGrid[0].transform.name].Count) timestampIndex -= temperatureValues[hostGrid[0].transform.name].Count;

            return (temperatureValues[hostname][timestampIndex].Item2 - MIN_TEMPERATURE) / (MAX_TEMPERATURE - MIN_TEMPERATURE);
        }
        if (displayMetric == MetricType.Usage)
        {
            /*while (timestampIndex < 0) timestampIndex += usageValues[hostGrid[0].transform.name].Count;
            while (timestampIndex >= usageValues[hostGrid[0].transform.name].Count) timestampIndex -= usageValues[hostGrid[0].transform.name].Count;
            return usageValues[hostname][timestampIndex].Item2;*/
            return currentUsageNodes[hostname].Value.Item2;
        }
        if (displayMetric == MetricType.Jobs)
        {
            /*
            while (timestampIndex < 0) timestampIndex += usageValues[hostGrid[0].transform.name].Count;
            while (timestampIndex >= usageValues[hostGrid[0].transform.name].Count) timestampIndex -= usageValues[hostGrid[0].transform.name].Count;

            return usageValues[hostname][timestampIndex].Item2;*/
            return currentUsageNodes[hostname].Value.Item2;
        }
        return 0;
    }
    public float GetAggregateValue(int timestepIndex)
    {
        float average = 0;
        Dictionary<string, List<Tuple<string, float>>> hostInformation = temperatureValues;
        if (displayMetric == MetricType.Temperature)
        {
            while (timestepIndex < 0) timestepIndex += temperatureValues[hostGrid[0].transform.name].Count;
            while (timestepIndex >= temperatureValues[hostGrid[0].transform.name].Count) timestepIndex -= temperatureValues[hostGrid[0].transform.name].Count;

            foreach (string key in temperatureValues.Keys)
            {
                average += (temperatureValues[key][timestepIndex].Item2) / temperatureValues.Keys.Count;
            }
        }
        else if (displayMetric == MetricType.Usage)
        {
            while (timestepIndex < 0) timestepIndex += usageValues[hostGrid[0].transform.name].Count;
            while (timestepIndex >= usageValues[hostGrid[0].transform.name].Count) timestepIndex -= usageValues[hostGrid[0].transform.name].Count;

            foreach(string key in currentUsageNodes.Keys)
            {
                average += currentUsageNodes[key].Value.Item2;
            }
            average /= currentUsageNodes.Keys.Count;
        }
        return average;
    }
    public void UpdateHostColor(GameObject gridObject, float percentage)
    {
        Color lerpedColor;
        if (displayMetric == MetricType.Temperature) lerpedColor = Color.Lerp(Color.white, new Color(222f, 45f / 255, 38f / 255), percentage);
        else lerpedColor = Color.Lerp(Color.white, Color.black, percentage);
        gridObject.GetComponent<Renderer>().material.color = lerpedColor;
    }

    public void UpdatePlaybackSpeed(float speed)
    {
        fastforwardRatio = speed;
        GetComponentInChildren<HostHeatmap>(true).UpdatePlaybackSpeed(timestepDelta / fastforwardRatio);
        AggregateSummary[] aggregates = transform.parent.GetComponentsInChildren<AggregateSummary>(true);
        foreach (AggregateSummary aggr in aggregates)
        {
            aggr.UpdatePlaybackSpeed(timestepDelta / fastforwardRatio);
        }
    }

    public string GetMetricType()
    {
        return displayMetric.ToString();
    }

    public string GetTimestamp(int timestampIndex)
    {
        if (displayMetric == MetricType.Usage)
        {
            while (timestampIndex < 0) timestampIndex += usageValues[hostGrid[0].transform.name].Count;
            while (timestampIndex >= usageValues[hostGrid[0].transform.name].Count) timestampIndex -= usageValues[hostGrid[0].transform.name].Count;

            return currentUsageNodes[hostGrid[0].transform.name].Value.Item1;
        }
        else
        {
            while (timestampIndex < 0) timestampIndex += temperatureValues[hostGrid[0].transform.name].Count;
            while (timestampIndex >= temperatureValues[hostGrid[0].transform.name].Count) timestampIndex -= temperatureValues[hostGrid[0].transform.name].Count;

            return temperatureValues[hostGrid[0].transform.name][timestampIndex].Item1;
        }
    }
    public void UpdateNodes(int incrementor)
    {
        foreach(string key in usageValues.Keys)
        {
            if (incrementor > 0)
            {
                currentUsageNodes[key] = currentUsageNodes[key].Next;
                if (currentUsageNodes[key] == null) currentUsageNodes[key] = usageValues[key].First;
            }
            else
            {
                currentUsageNodes[key] = currentUsageNodes[key].Previous;
                if (currentUsageNodes[key] == null) currentUsageNodes[key] = usageValues[key].Last;
            }
        }
    }
}
