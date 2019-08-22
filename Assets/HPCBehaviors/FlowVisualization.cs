using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlowVisualization : GazeFocusable
{
    public GameObject start;
    public GameObject end;
    public float flowRate;
    public float flowTemperature;
    public float flowVolume;
    public Material lineMaterial;

    public float minValue = 30;
    public float maxValue = 50;

    private Material[] instantiatedMaterials;
    public Color minColor = Color.white;
    public Color maxColor = Color.red;

    private Material jointMaterial;
    private float groundLevel = 0;
    [Tooltip("Only needed if in constant height mode")]
    public float heightLevel = 0;

    private int numPoints = 50;
    private Vector3 bezPointA;
    private Vector3 bezPointB;
    private Vector3 bezPointC;
    private float lineLength = 0;

    public enum DisplayType { CurvedLine, Grounded, Constant_Height, Waypoints, Dashed};
    public DisplayType displayType;
    [Tooltip("Only needed if in waypoint mode")]
    public Vector3[] waypoints;
    public bool localWaypoints = false;

    private bool initialized;

    // Start is called before the first frame update
    void Awake()
    {
        bezPointB = new Vector3((start.transform.position.x + end.transform.position.x) / 2, 
            Mathf.Max(start.transform.position.y, end.transform.position.y) + Mathf.Abs(start.transform.position.y - end.transform.position.y), 
            (start.transform.position.z + end.transform.position.z) / 2);

        bezPointA = start.transform.position; //+ new Vector3(0,0.5f,0);
        bezPointC = end.transform.position;// + new Vector3(0,0.5f,0);
        InitializeMaterials();
        initialized = false;
        //InitializeFlow();
    }

    // Update is called once per frame
    void Update()
    {
        if (instantiatedMaterials == null) return;
        if (displayType == DisplayType.Grounded || displayType == DisplayType.Waypoints || 
            displayType == DisplayType.Dashed || displayType == DisplayType.Constant_Height)
        {
            foreach (Material instantiatedMaterial in instantiatedMaterials)
            {
                instantiatedMaterial.mainTextureOffset += new Vector2(0, -flowRate * Time.deltaTime);
            }
            if (displayType != DisplayType.Dashed) jointMaterial.mainTextureOffset = new Vector2(0, -flowRate * Time.time);
        }
        else if (displayType == DisplayType.CurvedLine)
        {
            instantiatedMaterials[0].mainTextureOffset = new Vector2(-flowRate * Time.deltaTime, 0);
        }
    }

    public void InitializeHeightFlow(bool grounded)
    {
        float level = 0;
        if (grounded)
        {
            if (!FindObjectOfType<SetFloorLevel>().FloorInitialized())
               FindObjectOfType<SetFloorLevel>().SetLevel();
            level = FindObjectOfType<SetFloorLevel>().GetFloorLevel();
        }
        else level = heightLevel + transform.position.y;
        Vector3 heightStart = new Vector3(start.transform.position.x, level, start.transform.position.z);
        Vector3 heightEnd = new Vector3(end.transform.position.x, level, end.transform.position.z);

        // First Cylinder: Start -> Ground_start
        GameObject cylinder1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder1.transform.localScale = new Vector3(flowVolume, Vector3.Distance(start.transform.position, heightStart) / 2, flowVolume);
        cylinder1.transform.position = (start.transform.position + heightStart) / 2;
        cylinder1.transform.up = heightStart - cylinder1.transform.position;
        instantiatedMaterials[0].mainTextureScale = new Vector2(1, .5f * Vector3.Distance(start.transform.position, heightStart) / flowVolume);
        cylinder1.GetComponent<Renderer>().material = instantiatedMaterials[0];
        cylinder1.transform.parent = transform;

        // Add joint sphere
        GameObject joint1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        joint1.transform.localScale = new Vector3(flowVolume, flowVolume, flowVolume);
        joint1.transform.position = heightStart;
        joint1.transform.up = cylinder1.transform.up;
        joint1.GetComponent<Renderer>().material = jointMaterial;
        joint1.transform.parent = transform;

        // Second Cylinder: Ground_start -> Ground_end
        GameObject cylinder2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder2.transform.localScale = new Vector3(flowVolume, Vector3.Distance(heightStart, heightEnd) / 2, flowVolume);
        cylinder2.transform.position = (heightStart + heightEnd) / 2;
        cylinder2.transform.up = heightEnd - cylinder2.transform.position;
        instantiatedMaterials[1].mainTextureScale = new Vector2(1, .5f * Vector3.Distance(heightStart, heightEnd) / flowVolume);
        cylinder2.GetComponent<Renderer>().material = instantiatedMaterials[1];
        cylinder2.transform.parent = transform;

        // Add joint sphere
        GameObject joint2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        joint2.transform.localScale = new Vector3(flowVolume, flowVolume, flowVolume);
        joint2.transform.position = heightEnd;
        joint2.transform.up = cylinder2.transform.up;
        joint2.GetComponent<Renderer>().material = jointMaterial;
        joint2.transform.parent = transform;

        // Third Cylinder: Ground_end -> End
        GameObject cylinder3 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder3.transform.localScale = new Vector3(flowVolume, Vector3.Distance(heightEnd, end.transform.position) / 2, flowVolume);
        cylinder3.transform.position = (heightEnd + end.transform.position) / 2;
        cylinder3.transform.up = end.transform.position - cylinder3.transform.position;
        instantiatedMaterials[2].mainTextureScale = new Vector2(1, 0.5f * Vector3.Distance(heightEnd, end.transform.position) / flowVolume);
        cylinder3.GetComponent<Renderer>().material = instantiatedMaterials[2];
        cylinder3.transform.parent = transform;
    }

    public void InitializeMidAirCurvedFlow()
    {
        LineRenderer line = gameObject.AddComponent<LineRenderer>();
        line.startWidth = flowVolume;
        line.startWidth = flowVolume;
        line.positionCount = numPoints;
        DrawQuadraticCurve();

        Material mat = new Material(lineMaterial);
        mat.mainTextureScale = new Vector2(7.5f * lineLength, 1);
        instantiatedMaterials = new Material[1];
        instantiatedMaterials[0] = mat;
        line.material = mat;

        float valueRatio = (flowTemperature - minValue) / (maxValue - minValue);
        /*float tempRatio = (flowTemperature - minValue) / (maxValue - minValue);
        float labConstant = tempRatio * 256 - 128;
        Vector3 rgbVector = LabToRGB.Convert(new Vector3(50, labConstant, labConstant));*/
        line.startColor = Color.Lerp(minColor, maxColor, valueRatio);// new Color(rgbVector.x, rgbVector.y, rgbVector.z);
        line.endColor = Color.Lerp(minColor, maxColor, valueRatio); //new Color(rgbVector.x, rgbVector.y, rgbVector.z);
    }

    private void DrawLinearCurve()
    {
        Vector3[] bezierPositions = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            float t = (i + 1) / numPoints;
            bezierPositions[i] = CalculateLinearBezierPoint(t, start.transform.position, end.transform.position);
        }
        GetComponent<LineRenderer>().SetPositions(bezierPositions);
    }
    private void DrawQuadraticCurve()
    {
        Vector3[] bezierPositions = new Vector3[numPoints];
         for (int i = 0; i < numPoints; i++)
        {
            float t = (i + 1) / (float)numPoints;
            bezierPositions[i] = CalculateQuadraticBezierPoint(t, bezPointA, bezPointB, bezPointC);
            if (i > 0) lineLength += Vector3.Distance(bezierPositions[i], bezierPositions[i - 1]);
        }
        GetComponent<LineRenderer>().SetPositions(bezierPositions);
    }

    private Vector3 CalculateLinearBezierPoint(float t, Vector3 p0, Vector3 p1)
    {
        return p0 + t * (p1 - p0);
    }
    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;

        return p;
    }

    public void InitializeWaypointFlow()
    {
        // Create a list with the starting point prepended and ending point appended
        if (waypoints.Length == 0) return;
        Vector3 startingPoint = start.transform.position;
        int materialIndex = 0;
        foreach (Vector3 waypoint_raw in waypoints)
        {
            Vector3 waypoint;
            if (localWaypoints) waypoint = waypoint_raw + transform.position;
            else waypoint = waypoint_raw;
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.transform.localScale = new Vector3(flowVolume, Vector3.Distance(startingPoint, waypoint) / 2, flowVolume);
            cylinder.transform.position = (startingPoint + waypoint) / 2;
            cylinder.transform.up = waypoint - cylinder.transform.position;
            instantiatedMaterials[materialIndex].mainTextureScale = new Vector2(1, .5f * Vector3.Distance(startingPoint, waypoint) / flowVolume);
            cylinder.GetComponent<Renderer>().material = instantiatedMaterials[materialIndex];
            cylinder.transform.parent = transform;

            GameObject joint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            joint.transform.localScale = new Vector3(flowVolume, flowVolume, flowVolume);
            joint.transform.position = waypoint;
            joint.GetComponent<Renderer>().material = jointMaterial;
            joint.transform.up = cylinder.transform.up;
            joint.transform.parent = transform;

            startingPoint = waypoint;
            materialIndex++;
        }
        Vector3 lastWaypoint;
        Vector3 lastWaypoint_raw = waypoints[waypoints.Length - 1];
        if (localWaypoints) lastWaypoint = lastWaypoint_raw + transform.position;
        else lastWaypoint = lastWaypoint_raw;

        GameObject lastCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lastCylinder.transform.localScale = new Vector3(flowVolume, Vector3.Distance(end.transform.position, lastWaypoint) / 2, flowVolume);
        lastCylinder.transform.position = (end.transform.position + lastWaypoint) / 2;
        lastCylinder.transform.up = end.transform.position - lastCylinder.transform.position;
        instantiatedMaterials[materialIndex].mainTextureScale = new Vector2(1, .5f * Vector3.Distance(end.transform.position, lastWaypoint) / flowVolume);
        lastCylinder.GetComponent<Renderer>().material = instantiatedMaterials[materialIndex];
        lastCylinder.transform.parent = transform;
    }

    public void InitializeDashedFlow()
    {
        Vector3 oneThird = 2 * start.transform.position / 3 + end.transform.position / 3;
        Vector3 twoThirds = start.transform.position / 3 + 2 * end.transform.position / 3;

        GameObject frontCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        frontCylinder.transform.localScale = new Vector3(flowVolume, Vector3.Distance(start.transform.position, oneThird) / 2, flowVolume);
        frontCylinder.transform.position = (start.transform.position + oneThird) / 2;
        frontCylinder.transform.up = frontCylinder.transform.position - oneThird; 
        instantiatedMaterials[0].mainTextureScale = new Vector2(1, .5f * Vector3.Distance(start.transform.position, oneThird) / flowVolume);
        frontCylinder.GetComponent<Renderer>().material = instantiatedMaterials[0];
        frontCylinder.transform.parent = transform;

        GameObject middleCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        middleCylinder.transform.localScale = new Vector3(flowVolume, Vector3.Distance(oneThird, twoThirds) / 2, flowVolume);
        middleCylinder.transform.position = (oneThird + twoThirds) / 2;
        middleCylinder.transform.up = middleCylinder.transform.position - twoThirds;
        middleCylinder.GetComponent<Renderer>().material = Resources.Load("Materials/TransparencyStripes") as Material;
        middleCylinder.transform.parent = transform;

        GameObject endCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        endCylinder.transform.localScale = new Vector3(flowVolume, Vector3.Distance(twoThirds, end.transform.position) / 2, flowVolume);
        endCylinder.transform.position = (twoThirds + end.transform.position) / 2;
        endCylinder.transform.up =  frontCylinder.transform.position - end.transform.position;
        instantiatedMaterials[1].mainTextureScale = new Vector2(1, .5f * Vector3.Distance(twoThirds, end.transform.position) / flowVolume);
        endCylinder.GetComponent<Renderer>().material = instantiatedMaterials[1];
        endCylinder.transform.parent = transform;
    }

    private void InitializeMaterials()
    {
        float valueRatio = (flowTemperature - minValue) / (maxValue - minValue);
        /*float tempRatio = (flowTemperature - minValue) / (maxValue - minValue);
        float labConstant = tempRatio * 256 - 128;
        Vector3 rgbVector = LabToRGB.Convert(new Vector3(50, labConstant, labConstant));*/
        Color flowColor = Color.Lerp(minColor, maxColor, valueRatio);// new Color(rgbVector.x, rgbVector.y, rgbVector.z);
        //line.endColor = Color.Lerp(minColor, maxColor, valueRatio); //new Color(rgbVector.x, rgbVector.y, rgbVector.z);
        if (displayType == DisplayType.Waypoints) instantiatedMaterials = new Material[waypoints.Length + 1];
        else if (displayType == DisplayType.Dashed) instantiatedMaterials = new Material[2];
        else instantiatedMaterials = new Material[3];
        for (var i = 0; i < instantiatedMaterials.Length; i++)
        {
            instantiatedMaterials[i] = new Material(lineMaterial);
            instantiatedMaterials[i].color = flowColor;//new Color(rgbVector.x, rgbVector.y, rgbVector.z);
        }
        jointMaterial = new Material(lineMaterial);
        jointMaterial.color = flowColor;
    }

    public void UpdateColors()
    {
        float valueRatio = (flowTemperature - minValue) / (maxValue - minValue);
        foreach (Material mat in instantiatedMaterials)
        {
            mat.color = Color.Lerp(minColor, maxColor, valueRatio);
        }
        jointMaterial.color = Color.Lerp(minColor, maxColor, valueRatio);
    }

    public void InitializeFlow()
    {
        if ((end.GetComponentInParent<LocalToAnchor>() != null && !end.GetComponentInParent<LocalToAnchor>().isInitialized()) || 
                (start.GetComponentInParent<LocalToAnchor>() != null && !start.GetComponentInParent<LocalToAnchor>().isInitialized())) return;

        if (displayType == DisplayType.Grounded) InitializeHeightFlow(true);
        else if (displayType == DisplayType.Constant_Height) InitializeHeightFlow(false);
        else if (displayType == DisplayType.CurvedLine) InitializeMidAirCurvedFlow();
        else if (displayType == DisplayType.Waypoints) InitializeWaypointFlow();
        else if (displayType == DisplayType.Dashed) InitializeDashedFlow();
        foreach (Transform t in transform)
        {
            t.gameObject.AddComponent<MLVisTransform>();
            if (t.GetComponent<SphereCollider>() != null) t.GetComponent<SphereCollider>().radius = 1.5f;
            else if (t.GetComponent<CapsuleCollider>() != null) t.GetComponent<CapsuleCollider>().radius = 1.5f;
        }
        initialized = true;
    }

    public bool isInitialized()
    {
        return initialized;
    }

    public override void InvokeGaze(Vector3 hitPoint, Vector3 hitForward)
    {
        if (GetComponentInChildren<GazeFocusDetails>() != null) return;
        GameObject GazeFocusDetailsPrefab = Resources.Load("Prefabs/GazeFocusDetails") as GameObject;
        GameObject GazeFocusDetails = Instantiate(GazeFocusDetailsPrefab);
        GazeFocusDetails.transform.Find("TemperatureLabel/Canvas/Image/Text").GetComponent<Text>().text = "Temperature: " + flowTemperature.ToString();
        GazeFocusDetails.transform.Find("FlowRateLabel/Canvas/Image/Text").GetComponent<Text>().text = "Flow rate: " + flowRate.ToString();
        GazeFocusDetails.transform.Find("FlowVolumeLabel/Canvas/Image/Text").GetComponent<Text>().text = "Flow volume: " + flowVolume.ToString();
        GazeFocusDetails.transform.position = hitPoint + 0.1f * hitForward;
        GazeFocusDetails.transform.parent = transform;
        invoke();
    }
}
