using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TemperatureSummary : AggregateSummary
{
    public GameObject ThermometerPrefab;
    private GameObject thermometerObject;
    private int timestampIndex;
    private float temperatureValue;
    private int incrementor;
    // Start is called before the first frame update
    void Awake()
    {
        timestampIndex = -1;
        incrementor = 1;
        thermometerObject = Instantiate(ThermometerPrefab);
        thermometerObject.transform.parent = transform;
        thermometerObject.transform.localPosition = new Vector3(0, 0, -.2f);
        thermometerObject.transform.localEulerAngles = Vector3.zero;
        thermometerObject.transform.Find("LowTick/Canvas/Text").GetComponent<Text>().text = transform.parent.GetComponentInChildren<HostMetrics>().MIN_TEMPERATURE.ToString() + "\u00B0";
        thermometerObject.transform.Find("MidTick/Canvas/Text").GetComponent<Text>().text = ((transform.parent.GetComponentInChildren<HostMetrics>().MIN_TEMPERATURE + transform.parent.GetComponentInChildren<HostMetrics>().MAX_TEMPERATURE) / 2).ToString() + "\u00B0";
        thermometerObject.transform.Find("HighTick/Canvas/Text").GetComponent<Text>().text = transform.parent.GetComponentInChildren<HostMetrics>().MAX_TEMPERATURE.ToString() + "\u00B0";
    }

    // Update is called once per frame
    void Update()
    {
        //thermometerObject.transform.Find("Gear/default").localEulerAngles = usageObject.transform.Find("Gear/default").localEulerAngles +
          //  new Vector3(0, -100 * Time.deltaTime * usage, 0);
    }

    public void SetActive(bool activation)
    {
        thermometerObject.SetActive(activation);
    }

    public void UpdateValue()
    {
        GetComponentInParent<HostMetrics>().GetAggregateValue(timestampIndex);
    }

    void CycleTimestamp()
    {
        timestampIndex+=incrementor;
        HostMetrics[] grids = transform.parent.GetComponentsInChildren<HostMetrics>();
        float aggregateValue = 0;
        foreach (HostMetrics metricsGrid in grids)
        {
            aggregateValue += metricsGrid.GetAggregateValue(timestampIndex) / grids.Length;
        }

        updateThermoemeterPrefab(aggregateValue); 
        if (grids[0].GetMetricType() == "Temperature")
            transform.parent.GetComponentInChildren<HostMetrics>().WIMPiece.GetComponent<WIMPiece>().setValue((int)(100 * aggregateValue) / 100f);

    }
    public override void UpdatePlaybackSpeed(float frequency)
    {
        CancelInvoke();
        incrementor = (int)Mathf.Sign(frequency);
        InvokeRepeating("CycleTimestamp", 0, Mathf.Abs(frequency));
    }
    private void updateThermoemeterPrefab(float value)
    {
        temperatureValue = value;
        float temperaturePercentage = (value - transform.parent.GetComponentInChildren<HostMetrics>().MIN_TEMPERATURE) /
            (transform.parent.GetComponentInChildren<HostMetrics>().MAX_TEMPERATURE - transform.parent.GetComponentInChildren<HostMetrics>().MIN_TEMPERATURE);
        float lowTickPos = thermometerObject.transform.Find("LowTick").localPosition.y;
        float highTickPos = thermometerObject.transform.Find("HighTick").localPosition.y;

        thermometerObject.transform.Find("TempCylinder").localPosition = new Vector3(0, (highTickPos + lowTickPos) * temperaturePercentage / 2, 0);
        thermometerObject.transform.Find("TempCylinder").localScale = new Vector3(0.075f, (highTickPos + lowTickPos) * temperaturePercentage / 2, 0.075f);
        thermometerObject.transform.Find("TempCylinder").GetComponent<Renderer>().material.color = Color.Lerp(Color.white, new Color(222f, 45f / 255, 38f / 255), temperaturePercentage);
        thermometerObject.transform.Find("BaseBottom").GetComponent<Renderer>().material.color = Color.Lerp(Color.white, new Color(222f, 45f / 255, 38f / 255), temperaturePercentage);

        thermometerObject.transform.Find("Canvas/TemperatureText").GetComponent<Text>().text = temperatureValue.ToString() + "\u00B0";

        HostMetrics[] grids = transform.parent.GetComponentsInChildren<HostMetrics>();
        foreach (HostMetrics metric in grids)
        {
            if (thermometerObject.activeSelf && metric.WIMPiece != null)
                metric.WIMPiece.GetComponent<Renderer>().material.color = Color.Lerp(Color.white, new Color(222f, 45f / 255, 38f / 255), temperaturePercentage);
        }
        //float value = GetComponentInParent<HostMetrics>().GetAggregateValue(timestampIndex);
    }
}
