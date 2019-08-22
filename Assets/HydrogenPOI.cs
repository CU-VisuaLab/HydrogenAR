using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DxR;
using UnityEngine.UI;

public class HydrogenPOI : MonoBehaviour
{

    public GameObject metricObject;
    public GameObject timeSeriesObject;

    public Vector3 offset;

    private GameObject connector;
    private bool visInitialized;

    private int cycleStep;
    private GameObject tooltipLabel;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!visInitialized && timeSeriesObject.transform.Find("DxRView/DxRMarks").childCount > 0) initializeVis();

        if (transform.GetComponent<RelativeToGrab>() != null && transform.GetComponent<RelativeToGrab>().grab != null)
        {
            if (transform.GetComponent<RelativeToGrab>().grab.isHolding())
            {
                timeSeriesObject.transform.position = Camera.main.transform.position + 1.05f * Camera.main.transform.forward;
                metricObject.transform.position = Camera.main.transform.position + 0.85f * Camera.main.transform.forward;
                if (transform.name.ToLower().Contains("tip"))
                {
                    timeSeriesObject.transform.position -= 0.04f * Camera.main.transform.right + 0.03f * Camera.main.transform.up;
                    metricObject.transform.position += 0.08f * Camera.main.transform.right + 0.06f * Camera.main.transform.up;
                }
                else
                {
                    timeSeriesObject.transform.position -= (0.2f * Camera.main.transform.right + 0.12f * Camera.main.transform.up);
                    metricObject.transform.position -= (0.08f * Camera.main.transform.right + 0.06f * Camera.main.transform.up);
                }
            }
            else
            {
                timeSeriesObject.transform.position = new Vector3(100,100,100);
                metricObject.transform.position = new Vector3(100,100,100);
                connector.SetActive(false);
                return;
            }
        }

        timeSeriesObject.transform.LookAt(Camera.main.transform);
        timeSeriesObject.transform.eulerAngles = new Vector3(0, timeSeriesObject.transform.eulerAngles.y + 180, 0);
        metricObject.transform.LookAt(Camera.main.transform);
        metricObject.transform.eulerAngles = new Vector3(0, metricObject.transform.eulerAngles.y, 0);

        if (connector == null) initializeConnections();
        else UpdateConnector();
    }

    private void UpdateConnector()
    {
        connector.transform.parent = null;
        connector.transform.localScale = new Vector3(0.01f, Vector3.Distance(transform.position, metricObject.transform.position) / 2, 0.01f);
        connector.transform.position = (transform.position + metricObject.transform.position) / 2;
        connector.transform.up = metricObject.transform.position - connector.transform.position;
        connector.transform.parent = transform;
        connector.SetActive(timeSeriesObject.activeSelf || metricObject.activeSelf);
    }

    private void initializeConnections()
    {
        if (GetComponent<RelativeToGrab>() != null)
        {
            if (GetComponent<RelativeToGrab>().grab == null) GetComponent<RelativeToGrab>().setGrab();
            metricObject.transform.position = transform.position + offset.x * GetComponent<RelativeToGrab>().grab.GetTransform().right +
                offset.y * GetComponent<RelativeToGrab>().grab.GetTransform().up +
                offset.z * GetComponent<RelativeToGrab>().grab.GetTransform().forward;
            timeSeriesObject.transform.localPosition = metricObject.transform.position;
        }
        connector = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        connector.transform.GetComponent<Renderer>().material = Resources.Load("Materials/TransparencyStripes") as Material;

        connector.transform.parent = transform;
    }

    private void initializeVis()
    {
        // Pop out the scale line
        Transform scaleMark = timeSeriesObject.transform.Find("DxRView/DxRMarks").GetChild(0);
        scaleMark.localScale = new Vector3(scaleMark.localScale.x, 0.5f, 1.05f * scaleMark.transform.localScale.z);
        scaleMark.localPosition = new Vector3(scaleMark.localPosition.x, 0.25f, scaleMark.transform.localPosition.z);
        scaleMark.GetComponent<Renderer>().material.color = Color.red;
        visInitialized = true;

        // Add a tooltip label
        /*tooltipLabel = Instantiate(Resources.Load("Prefabs/TooltipLabel") as GameObject);
        tooltipLabel.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        tooltipLabel.transform.parent = timeSeriesObject.transform;
        tooltipLabel.transform.localEulerAngles = new Vector3(0, 180, 0);
        cycleStep = 0;

        nextTimestep();*/
    }

    public void nextTimestep()
    {
        if (cycleStep > 0)
        {
            timeSeriesObject.transform.Find("DxRView/DxRMarks").GetChild(cycleStep).GetComponent<Renderer>().material.color = Color.white;
            timeSeriesObject.transform.Find("DxRView/DxRMarks").GetChild(cycleStep).localScale = new Vector3(
                timeSeriesObject.transform.Find("DxRView/DxRMarks").GetChild(cycleStep).localScale.x,
                timeSeriesObject.transform.Find("DxRView/DxRMarks").GetChild(cycleStep).localScale.y,
                timeSeriesObject.transform.Find("DxRView/DxRMarks").GetChild(cycleStep).localScale.z / 1.05f);
        }
        cycleStep++;
        tooltipLabel.transform.localPosition = new Vector3(timeSeriesObject.transform.Find("DxRView/DxRMarks").GetChild(cycleStep).localPosition.x, 0.5f, 0);
        timeSeriesObject.transform.Find("DxRView/DxRMarks").GetChild(cycleStep).GetComponent<Renderer>().material.color = Color.cyan;
        timeSeriesObject.transform.Find("DxRView/DxRMarks").GetChild(cycleStep).localScale = new Vector3(
            timeSeriesObject.transform.Find("DxRView/DxRMarks").GetChild(cycleStep).localScale.x,
            timeSeriesObject.transform.Find("DxRView/DxRMarks").GetChild(cycleStep).localScale.y,
            timeSeriesObject.transform.Find("DxRView/DxRMarks").GetChild(cycleStep).localScale.z * 1.05f);
        tooltipLabel.GetComponentInChildren<Text>().text = "Cycles:\n" + (50 + 250 * (cycleStep - 1)).ToString();
    }
    public void resetCycleStep()
    {
        cycleStep = 0;
        nextTimestep();
    }
}
