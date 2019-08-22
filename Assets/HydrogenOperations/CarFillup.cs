using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using DxR;

public class CarFillup : MonoBehaviour
{
    private bool fillingTank;
    private float fillLevel;
    private float maxFill;

    private GameObject car;
    private GameObject tank;
    private GameObject tankFill;

    private GameObject fillCap;
    private GameObject nozzleTip_70;
    private GameObject nozzleTip_35;

    private Vector3 initialFillPosition;
    private Vector3 initialFillScale;

    private GameObject nozzleEngaged;
    private bool finishedFill;

    [Tooltip("Percentage of fill per second")]
    private float fillRate = 1;
    private int visIndex = 0;
    private float fastforwardRate = 1;

    private GameObject nozzle;
    private GameObject slider;

    private MLInputController controller;

    public TextAsset csvAsset;
    private string[,] csvGrid;

    private Material pressureMaterial;
    private Material temperatureMaterial;

    private bool allVisesInitialized;

    // Start is called before the first frame update
    void Start()
    {
        ControllerConnectionHandler _controllerConnectionHandler = FindObjectOfType<ControllerConnectionHandler>();
        controller = _controllerConnectionHandler.ConnectedController;
        MLInput.OnControllerButtonDown += ButtonDown;
        MLInput.OnControllerButtonUp += ButtonUp;

        csvGrid = CSVReader.SplitCsvGrid(csvAsset.text);

        pressureMaterial = Resources.Load("Materials/PressureMaterial") as Material;
        temperatureMaterial = Resources.Load("Materials/TemperatureMaterial") as Material;

        InitializeCar();
    }

    // Update is called once per frame
    void Update()
    {
        if (!allVisesInitialized)
        {
            Vis[] dxrVises = car.GetComponentsInChildren<Vis>(); 
            allVisesInitialized = true;
            foreach (Vis vis in dxrVises)
            {
                if (visInitialized(vis))
                {
                    if (vis.transform.Find("DxRView/DxRMarks").GetChild(vis.transform.Find("DxRView/DxRMarks").childCount - 1).GetComponent<Renderer>().enabled)
                        initializeVis(vis);
                }
                else allVisesInitialized = false;
            }
        }
        if (nozzleEngaged == null)
        {
            if (Vector3.Distance(fillCap.transform.position, nozzleTip_70.transform.position) < 0.15f)
            {
                car.GetComponentInChildren<FlowVisualization>().flowRate = 1.5f;
                nozzleEngaged = nozzleTip_70;
                fillRate = 20;
                initializeFilling();
            }
            else if (Vector3.Distance(fillCap.transform.position, nozzleTip_35.transform.position) < 0.15f)
            {
                car.GetComponentInChildren<FlowVisualization>().flowRate = .75f;
                nozzleEngaged = nozzleTip_35;
                fillRate = 10;
                initializeFilling();
            }
        }
    }

    private void InitializeCar()
    {
        nozzleEngaged = null;
        finishedFill = false;
        engageFillup();

        car.GetComponentInChildren<FlowVisualization>().InitializeFlow();
        car.GetComponentInChildren<FlowVisualization>().flowRate = 0;
        car.transform.localEulerAngles = new Vector3(0, 180, 0);
        car.transform.position = Camera.main.transform.position + new Vector3(-.3f, -1f, -.2f);

        initialFillScale = tankFill.transform.localScale;
        initialFillPosition = tankFill.transform.localPosition;

        allVisesInitialized = false;
    }

    public void engageFillup()
    {
        car = Instantiate(Resources.Load("Prefabs/Car") as GameObject);
        car.transform.position = Camera.main.transform.position + new Vector3(-1.2f, -1.5f, -.1f);
        fillCap = car.transform.Find("InCar/Cylinder").gameObject;
        nozzleTip_35 = GameObject.Find("35MPa_Nozzle/Tip");
        nozzleTip_70 = GameObject.Find("70MPa_Nozzle/Tip"); 
        tank = car.transform.Find("TankParent/Tank").gameObject;
        tankFill = car.transform.Find("TankParent/TankFill").gameObject;
        maxFill = tank.transform.localScale.y * 0.99f;
    }

    private void increaseFillLevel()
    {
        if (fillLevel >= 1) return;
        fillLevel += fillRate * Time.deltaTime / 100;
        tankFill.transform.localScale = new Vector3(tankFill.transform.localScale.x, fillLevel * maxFill, tankFill.transform.localScale.z);
        tankFill.transform.localPosition = new Vector3(tankFill.transform.localPosition.x, tankFill.transform.localPosition.y + fillRate * maxFill * Time.deltaTime / 200, tankFill.transform.localPosition.z);

        HydrogenPOI[] hydrogenPOIs = FindObjectsOfType<HydrogenPOI>();
        foreach (HydrogenPOI poi in hydrogenPOIs) 
        {
            if (poi.transform.parent == nozzleEngaged.transform.parent || 
                (poi.transform.name.Contains("35") && nozzleEngaged == nozzleTip_35) || 
                (poi.transform.name.Contains("70") && nozzleEngaged == nozzleTip_70)) 
                poi.nextTimestep();
        }
    }

    private void initializeFilling()
    {
        GameObject nozzlePrefab = new GameObject();
        if (nozzleEngaged == nozzleTip_70)
        {
            nozzlePrefab = Resources.Load("Prefabs/70MPa_Nozzle_Model") as GameObject;
        }
        else if (nozzleEngaged == nozzleTip_35)
        {
            nozzlePrefab = Resources.Load("Prefabs/35MPa_Nozzle_Model") as GameObject;
        }
        nozzle = Instantiate(nozzlePrefab);
        nozzle.transform.parent = car.transform;
        nozzle.transform.position = fillCap.transform.position;
        nozzle.transform.localEulerAngles = new Vector3(-90, -90, 0);

        GameObject sliderPrefab = Resources.Load("Prefabs/FillupSlider") as GameObject;
        slider = Instantiate(sliderPrefab);
        slider.transform.parent = car.transform;
        slider.transform.localEulerAngles = new Vector3(0, 90, 0);
        slider.transform.localPosition = new Vector3(0, 3f, 0.125f);
        slider.transform.localScale = new Vector3(2.093755f, 2.093755f, 2.093755f);
        slider.transform.GetComponentInChildren<Text>().text = "0:00";

        Invoke("IncrementFill", (2 / fastforwardRate));
    }

    public void resetTank()
    {
        tankFill.transform.localPosition = initialFillPosition;
        tankFill.transform.localScale = initialFillScale;
        fillLevel = 0;
    }

    public void moveCar(Vector3 delta)
    {
        car.transform.position += delta;
    }

    private void ButtonDown(byte controllerId, MLInputControllerButton button)
    {
        if (button == MLInputControllerButton.Bumper) fastforwardRate = 10;
        else if (button == MLInputControllerButton.HomeTap)
        {
            CancelInvoke();
            Destroy(car);
            InitializeCar();
        }
    }
    private void ButtonUp(byte controllerId, MLInputControllerButton button)
    {
        if (button == MLInputControllerButton.Bumper) fastforwardRate = 1;
    }

    private void IncrementFill()
    {
        Transform dxrMarks = car.GetComponentInChildren<Vis>().transform.Find("DxRView/DxRMarks");

        // Move the slider forward
        slider.transform.Find("Handle").localPosition += new Vector3(0.6f / dxrMarks.childCount, 0, 0);
        slider.transform.GetComponentInChildren<Text>().text = (int.Parse(csvGrid[0,visIndex + 1]) / 60).ToString() + ":" + (int.Parse(csvGrid[0, visIndex + 1]) % 60).ToString().PadLeft(2, '0');

        // Add to the fill visualization
        Vis[] dxrVises = car.GetComponentsInChildren<Vis>();
        foreach (Vis vis in dxrVises)
        {
            dxrMarks = vis.transform.Find("DxRView/DxRMarks"); 
            dxrMarks.GetChild(visIndex).GetComponent<Renderer>().enabled = true;
            if (dxrMarks.GetChild(visIndex).Find("Cylinder") != null) dxrMarks.GetChild(visIndex).Find("Cylinder").GetComponent<Renderer>().enabled = true;
        }

        if (visIndex == dxrMarks.childCount - 1)
        {
            car.GetComponentInChildren<FlowVisualization>().flowRate = 0;
            return;
        }
        // Fill up the tank
        tankFill.transform.localScale = new Vector3(tankFill.transform.localScale.x, (float)visIndex / dxrMarks.childCount, tankFill.transform.localScale.z);
        tankFill.transform.localPosition = new Vector3(1 - (float)visIndex / dxrMarks.childCount, tankFill.transform.localPosition.y, tankFill.transform.localPosition.z);

        // Change the flow color
        FlowVisualization flow = car.GetComponentInChildren<FlowVisualization>();
        flow.flowTemperature = float.Parse(csvGrid[2, visIndex + 1]);
        flow.UpdateColors();
        visIndex++;
        Invoke("IncrementFill", (2 / fastforwardRate));
    }

    private void initializeVis(Vis vis)
    {
        Transform dxrMarks = vis.transform.Find("DxRView/DxRMarks");
        float deltaX = dxrMarks.GetChild(0).transform.localPosition.x;
        float previousY = dxrMarks.GetChild(0).transform.localPosition.y;
        float previousZ = dxrMarks.GetChild(0).transform.localPosition.z;

        for (var i = 0; i < dxrMarks.childCount - 1; i++)
        {
            Transform cylinder = dxrMarks.GetChild(i).Find("Cylinder");
            cylinder.up = dxrMarks.transform.GetChild(i + 1).position - dxrMarks.transform.GetChild(i).position;
            cylinder.position = (dxrMarks.transform.GetChild(i + 1).position + dxrMarks.transform.GetChild(i).position) / 2;
            cylinder.localScale = new Vector3(cylinder.localScale.x, 0.5f *  Vector3.Distance(dxrMarks.transform.GetChild(i).position, dxrMarks.transform.GetChild(i + 1).position) / dxrMarks.transform.GetChild(i).localScale.y, cylinder.localScale.z);

            if (vis.visSpecsURL.ToLower().Contains("temperature_only"))
            {
                dxrMarks.GetChild(i).GetComponent<Renderer>().material = temperatureMaterial;
                cylinder.GetComponent<Renderer>().material = temperatureMaterial;
            }
            else if (vis.visSpecsURL.ToLower().Contains("pressure_only"))
            {
                dxrMarks.GetChild(i).GetComponent<Renderer>().material = pressureMaterial;
                cylinder.GetComponent<Renderer>().material = pressureMaterial;
            }
        }

        foreach (Transform markTransform in dxrMarks)
        {
            markTransform.position -= new Vector3(deltaX, 0, 0);
            markTransform.GetComponent<Renderer>().enabled = false;
            markTransform.Find("Cylinder").GetComponent<Renderer>().enabled = false;
        }

        if (vis.visSpecsURL.ToLower().Contains("temperature_only"))
        {
            vis.transform.Find("DxRView/DxRGuides").GetChild(1).Find("Title/Text").GetComponent<TextMesh>().color = new Color(65f/255,105f/255,225);
        }
        else if (vis.visSpecsURL.ToLower().Contains("pressure_only"))
        {
            vis.transform.Find("DxRView/DxRGuides").GetChild(1).Find("Title/Text").GetComponent<TextMesh>().color = new Color(1,138f/255,28f/255);
        }
        Destroy(dxrMarks.GetChild(dxrMarks.transform.childCount - 1).gameObject);
    }

    private bool visInitialized(Vis vis)
    {
        return (vis.transform.Find("DxRView/DxRMarks").childCount > 0);
    }
}
