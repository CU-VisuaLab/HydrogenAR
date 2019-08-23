using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DxR;
using UnityEngine.UI;

public class WIMFocusObject : MonoBehaviour
{
    private GameObject connector;
    public GameObject[] focusObjects;
    public GameObject pointOfInterest;

    private Vector3 bottomOfObject;
    private Vector3 topOfObject;
    private int currentFocusIndex;
    private bool focusOff;

    private float value;

    private int cycleStep;
    private GameObject tooltipLabel;
    private bool visInitialized;

    private Material highlight;
    private Material defaultMaterial;

    private float frequency;
    private int incrementor;
    private bool incrementStarted;

    // Start is called before the first frame update
    void Start()
    {
        if (focusObjects.Length == 0) return;
        focusOff = false;
        connector = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        connector.transform.GetComponent<Renderer>().material = Resources.Load("Materials/TransparencyStripes") as Material;
        foreach (GameObject focusObject in focusObjects) focusObject.SetActive(false);
        currentFocusIndex = 0;
        focusObjects[currentFocusIndex].SetActive(true);
        visInitialized = false;
        highlight = Resources.Load("Materials/HydrogenFill") as Material;
        defaultMaterial = Resources.Load("Materials/Default") as Material;

        incrementStarted = false;
        cycleStep = -1;
        incrementor = 1;
        UpdatePlaybackSpeed(0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (focusObjects.Length == 0) return;
        if (!visInitialized && focusObjects[1].transform.Find("DxRView/DxRMarks").childCount > 0) initializeVis();
        UpdateLabel();
        UpdateConnector();
        FaceUser();
    }

    private void UpdateConnector()
    {
        connector.transform.parent = null;

        // Determine where the connector should end on the focus object
        if (focusObjects[currentFocusIndex].GetComponent<RectTransform>() != null)
        {
            bottomOfObject = focusObjects[currentFocusIndex].transform.position - new Vector3(0, focusObjects[currentFocusIndex].GetComponent<RectTransform>().rect.height / 2, 0);
            topOfObject = focusObjects[currentFocusIndex].transform.position + new Vector3(0, focusObjects[currentFocusIndex].GetComponent<RectTransform>().rect.height / 2, 0);
        }
        else if (focusObjects[currentFocusIndex].GetComponent<Vis>() != null)
        {
            bottomOfObject = focusObjects[currentFocusIndex].transform.position;
            topOfObject = focusObjects[currentFocusIndex].transform.position;
        }

        // Dynamically update the connector position, scale and pose based on start/end points.
        if (focusObjects[currentFocusIndex].transform.position.y < pointOfInterest.transform.position.y)
        {
            connector.transform.localScale = new Vector3(0.01f, Vector3.Distance(topOfObject, pointOfInterest.transform.position) / 2, 0.01f);
            connector.transform.position = (pointOfInterest.transform.position + topOfObject) / 2;
            connector.transform.up = topOfObject - connector.transform.position;
            connector.transform.parent = transform;
        }
        else
        {
            connector.transform.localScale = new Vector3(0.01f, Vector3.Distance(bottomOfObject, pointOfInterest.transform.position) / 2, 0.01f);
            connector.transform.position = (pointOfInterest.transform.position + bottomOfObject) / 2;
            connector.transform.up = bottomOfObject - connector.transform.position;
            connector.transform.parent = transform;
        }
    }
    private void FaceUser()
    {
        focusObjects[currentFocusIndex].transform.LookAt(Camera.main.transform);
        focusObjects[currentFocusIndex].transform.localEulerAngles = new Vector3(0, 180 + focusObjects[currentFocusIndex].transform.localEulerAngles.y, 0);
    }

    public void cycleFocusObject()
    {
        // Turn off focus objects
        if (currentFocusIndex + 1 == focusObjects.Length && !focusOff)
        {
            focusObjects[currentFocusIndex].SetActive(false);
            focusOff = true;
            connector.SetActive(false);
        }
        // Turn back on focus objects
        else if (focusOff)
        {
            focusOff = false;
            currentFocusIndex = 0;
            focusObjects[0].SetActive(true);
            connector.SetActive(true);
        }
        // Cycle through focus objects
        else
        {
            focusObjects[currentFocusIndex].SetActive(false);
            focusObjects[(currentFocusIndex + 1) % focusObjects.Length].transform.position = focusObjects[currentFocusIndex].transform.position;
            currentFocusIndex = (currentFocusIndex + 1) % focusObjects.Length;
            focusObjects[currentFocusIndex].SetActive(true);
        }
    }

    public void setMetric(string metric)
    {
        string oldText = focusObjects[0].transform.Find("Text").GetComponent<Text>().text;
        focusObjects[0].transform.Find("Text").GetComponent<Text>().text = metric + ":" + oldText.Split(':')[1];
    }

    public void UpdateLabel()
    {
        string oldText = focusObjects[0].transform.Find("Text").GetComponent<Text>().text;
        string valueString = value.ToString();
        if (FindObjectOfType<HostMetrics>().GetMetricType() == "Usage") valueString += "%";
        else if (FindObjectOfType<HostMetrics>().GetMetricType() == "Temperature") valueString += "\u00B0";
        focusObjects[0].transform.Find("Text").GetComponent<Text>().text = oldText.Split(':')[0] + ": " + valueString;
        if (tooltipLabel != null) tooltipLabel.transform.Find("Canvas/Image/Text").GetComponent<Text>().text = valueString;
    }
    public void UpdateValue(float val)
    {
        value = val;
    }

    private void initializeVis()
    {
        tooltipLabel = Instantiate(Resources.Load("Prefabs/TooltipLabel") as GameObject);
        tooltipLabel.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        tooltipLabel.transform.parent = focusObjects[1].transform;
        tooltipLabel.transform.localEulerAngles = new Vector3(0, 180, 0);

        while (cycleStep < 0) cycleStep += focusObjects[1].transform.Find("DxRView/DxRMarks").childCount;
        cycleStep = cycleStep % focusObjects[1].transform.Find("DxRView/DxRMarks").childCount;
        tooltipLabel.transform.localPosition = new Vector3(focusObjects[1].transform.Find("DxRView/DxRMarks").GetChild(cycleStep).localPosition.x, 0.5f, 0);

        visInitialized = true;
    }

    public void UpdateTimestepIndex()
    {
        //if (cycleStep == index) return;
        Transform dxrMarks = focusObjects[1].transform.Find("DxRView/DxRMarks");
        int childCount = dxrMarks.childCount;

        if (childCount > 0 && cycleStep > 0)
        {
            Transform oldChild = dxrMarks.GetChild(cycleStep);
            oldChild.localScale = new Vector3(oldChild.localScale.x, oldChild.localScale.y, .01f);
            oldChild.GetComponent<Renderer>().material = defaultMaterial;
        }

        dxrMarks.GetChild(0).GetComponent<Renderer>().material = defaultMaterial;
        //cycleStep = index;
        cycleStep += incrementor;
        if (cycleStep < 0) cycleStep += childCount;
        else if (cycleStep > childCount) cycleStep -= childCount;

        if (childCount > 0)
        {
            while (cycleStep < 0) cycleStep += childCount;
            cycleStep = cycleStep % childCount;
            Transform newChild = dxrMarks.GetChild(cycleStep);
            if (incrementStarted) newChild.GetComponent<Renderer>().material = highlight;
            newChild.localScale = new Vector3(newChild.localScale.x, newChild.localScale.y, .015f);
            if (tooltipLabel != null)  tooltipLabel.transform.localPosition =
                new Vector3(newChild.localPosition.x, newChild.transform.localScale.y + 0.05f, 0);

        }
        incrementStarted = true;
    }

    public void UpdatePlaybackSpeed(float playback)
    {
        CancelInvoke();
        frequency = Mathf.Abs(playback);
        incrementor = (int)Mathf.Sign(playback);
        InvokeRepeating("UpdateTimestepIndex", 0, 1/frequency);
    }
}
