using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using static MagicLeap.ImageTrackingExample;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using MagicLeap;
using System.Threading.Tasks;

[RequireComponent(typeof(PrivilegeRequester))]
public class VisSpatialMappingTracking : MonoBehaviour
{
    public GameObject[] TrackerBehaviours;
    public GameObject[] AnchoredObjects;

    #region Private Variables
    private Vector3[] trackedObjectPositions;

    private ViewMode _viewMode = ViewMode.TrackingCubeOnly;

    [SerializeField, Tooltip("Image Tracking Visualizers to control")]
    private ImageTrackingVisualizer[] _visualizers = null;

    [Space, SerializeField, Tooltip("ControllerConnectionHandler reference.")]
    private ControllerConnectionHandler _controllerConnectionHandler = null;

    private PrivilegeRequester _privilegeRequester = null;

    private bool _hasStarted = false;

    private GameObject debugCube;
    private GameObject debugCube2;
    private Transform meshParent;

    private Vector3[] anchoredPositions;
    private Vector3[] anchoredLookAts;
    private bool mutex;

    private bool[] calibrating;
    private List<Vector3>[] totalPositions;
    private List<Vector3>[] totalLookAts;

    private const float DISTANCE_THRESHOLD = 25;
    private const float ANGLE_THRESHOLD = 30;

    private int currentFocus;

    public GameObject _camera;

    private int currentIndex;

    #endregion
    // Using Awake so that Privileges is set before PrivilegeRequester Start
    void Awake()
    {
        currentIndex = 0;
        UpdateVisualizers();

        // If not listed here, the PrivilegeRequester assumes the request for
        // the privileges needed, CameraCapture in this case, are in the editor.
        _privilegeRequester = GetComponent<PrivilegeRequester>();

        // Before enabling the MLImageTrackerBehavior GameObjects, the scene must wait until the privilege has been granted.
        _privilegeRequester.OnPrivilegesDone += HandlePrivilegesDone;

        /*debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debugCube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debugCube.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
        debugCube2.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
        */
        meshParent = FindObjectOfType<MLSpatialMapper>().meshParent;
        anchoredPositions = new Vector3[TrackerBehaviours.Length];
        anchoredLookAts = new Vector3[TrackerBehaviours.Length];

        totalPositions = new List<Vector3>[TrackerBehaviours.Length];
        totalLookAts = new List<Vector3>[TrackerBehaviours.Length];
        calibrating = new bool[TrackerBehaviours.Length];
        for (var i = 0; i < calibrating.Length; i++) startCalibrating(i);

        //finishCalibrating(0);
        //finishCalibrating(1);
        /*Invoke("fin", 1);
        Invoke("fin", 2);
        Invoke("fin", 3);
        Invoke("fin", 4);
        Invoke("fin", 5);
        Invoke("fin", 6);
        Invoke("fin", 7);
        Invoke("fin", 8);
        Invoke("fin", 9);
        Invoke("fin", 10);
        Invoke("fin", 11);
        Invoke("fin", 12);
        Invoke("fin", 13);
        Invoke("fin", 14);*/
        currentFocus = -1;
        mutex = false;
    }

    void fin()
    {
        if (calibrating[0]) finishCalibrating(0);
        else if (calibrating[1]) finishCalibrating(1);
        else if (calibrating[2]) finishCalibrating(2);
        else if (calibrating[3]) finishCalibrating(3);
        else if (calibrating[4]) finishCalibrating(4);
        else if (calibrating[5]) finishCalibrating(5);
        else if (calibrating[6]) finishCalibrating(6);
        else if (calibrating[7]) finishCalibrating(7);
        else if (calibrating[8]) finishCalibrating(8);
        else if (calibrating[9]) finishCalibrating(9);
        else if (calibrating[10]) finishCalibrating(10);
        else if (calibrating[11]) finishCalibrating(11);
        else if (calibrating[12]) finishCalibrating(12);
        else finishCalibrating(13);
    }
    // Update is called once per frame
    void Update()
    {
        try {
            for (var i = 0; i < anchoredPositions.Length; i++)
            {
                if (totalPositions[i].Count == 1) AnchoredObjects[i].transform.position = anchoredPositions[i];
                else AnchoredObjects[i].transform.position = anchoredPositions[i];
                AnchoredObjects[i].transform.LookAt(anchoredLookAts[i]);
            }
            if (!mutex)
            {
                Task.Factory.StartNew(UpdateSpatialAnchors);//new Task(UpdateSpatialAnchors).Start();
            }
        }
        catch (Exception e)
        {
            GameObject.Find("DebugText").GetComponent<Text>().text = "Update: " + e.Message;
        }
    }

    void UpdateSpatialAnchors()
    {
        try
        {
            mutex = true;
            for (var i = currentIndex; i < currentIndex + 1; i++)
            {
                GameObject trackedObject = TrackerBehaviours[i];
                //GameObject.Find("DebugText").GetComponent<Text>().text = "Looking for Marker: " + trackedObject.transform.name.ToUpper().Replace("MARKER", "");
                if (!calibrating[i] || totalPositions[i].Count == 8)
                {
                    finishCalibrating(i);
                    continue;
                }
                if (!trackedObject.GetComponent<ImageTrackingVisualizer>()._targetFound)
                {
                    continue;
                }
                //GameObject.Find("DebugText").GetComponent<Text>().text = "Got past continues: " + i.ToString();
                Vector3 adjustedPos;// = trackedObject.transform.position + 0.4f * trackedObject.transform.up - 0.05f * trackedObject.transform.forward - 0.05f * trackedObject.transform.right;
                RaycastHit hit;
                if (Physics.Linecast(_camera.transform.position, trackedObject.transform.position, out hit))
                {
                    adjustedPos = hit.point;

                }
                else continue;
                //GameObject.Find("DebugText").GetComponent<Text>().text = "Got past raycast: " + i.ToString();

                float closestDistance = Mathf.Infinity;
                Transform closestMesh = null;
                Vector3 closestPoint = Vector3.zero;
                Vector3 closestNormal = Vector3.zero;
                foreach (Transform meshTransform in meshParent)
                {
                    if (meshTransform.name.Contains("MLSpatialMapper") ||
                        meshTransform.GetComponent<MeshFilter>().mesh == null)
                    {
                        continue;
                    }
                    try
                    {
                        for (var j = 0; j < meshTransform.GetComponent<MeshFilter>().mesh.vertexCount; j += 3)
                        {
                            Vector3 meshVertex = meshTransform.GetComponent<MeshFilter>().mesh.vertices[j];
                            Vector3 potentialClosestPoint = meshTransform.TransformPoint(meshVertex);
                            Vector3 potentialClosestNormal = meshTransform.GetComponent<MeshFilter>().mesh.normals[j];
                            if (Vector3.Distance(potentialClosestPoint, adjustedPos) < closestDistance &&
                                Vector3.Angle(potentialClosestNormal, trackedObject.transform.up) < ANGLE_THRESHOLD &&
                                Vector3.Distance(adjustedPos, meshVertex) < DISTANCE_THRESHOLD)
                            {
                                closestMesh = meshTransform;
                                closestNormal = potentialClosestNormal;
                                closestDistance = Vector3.Distance(potentialClosestPoint, adjustedPos);
                                closestPoint = potentialClosestPoint;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
                        //GameObject.Find("DebugText").GetComponent<Text>().text = e.Message;
                    }
                }
                //GameObject.Find("DebugText").GetComponent<Text>().text = "Got past foreach: " + i.ToString();

                if (closestPoint != Vector3.zero)
                {
                    //GameObject.Find("DebugText").GetComponent<Text>().text = "Non-zero";
                    trackedObject.GetComponent<ImageTrackingVisualizer>()._demo.transform.position = closestMesh.position;
                    //if (totalPositions[i].Count == 20) totalPositions[i].RemoveAt(0);
                    //if (totalLookAts[i].Count == 20) totalLookAts[i].RemoveAt(0);
                    totalPositions[i].Add(closestPoint + 0.2f * closestNormal);
                    totalLookAts[i].Add(closestPoint + closestNormal);
                    anchoredPositions[i] = Vector3Average(totalPositions[i]);
                    anchoredLookAts[i] = Vector3Average(totalLookAts[i]);
                    //GameObject.Find("DebugText").GetComponent<Text>().text = "Vectors: " + totalPositions[i].Count;
                }
            }
            mutex = false;

        }
        catch (Exception e)
        {
            GameObject.Find("DebugText").GetComponent<Text>().text = "Update Anchors: " + e.Message;
        }

    }
    private Vector3 Vector3Average(List<Vector3> vec3List)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;
        if (vec3List.Count == 0) return sum;
        foreach (Vector3 vec3 in vec3List)
        {
            sum += vec3;
            count++;
        }
        return sum / count;
    }

    public void startCalibrating(int i)
    {
        totalPositions[i] = new List<Vector3>();
        totalLookAts[i] = new List<Vector3>();
        calibrating[i] = true;
        currentFocus = i;
    }

    void ClearText()
    {
        GameObject.Find("DebugText").GetComponent<Text>().text = "";
    }

    public void finishCalibrating(int i)
    {
        try {
            currentIndex++;
            calibrating[i] = false;
            LocalToAnchor[] anchoredComponents = FindObjectsOfType<LocalToAnchor>();
            if (anchoredComponents != null)
            {
                foreach (LocalToAnchor anchor in anchoredComponents)
                {
                    if (anchor.localTo == AnchoredObjects[i]) anchor.initialize();
                }
            }

            FlowVisualization[] allFlows = FindObjectsOfType<FlowVisualization>();
            /*foreach (FlowVisualization flow in allFlows)
            {
                if (flow.end.GetComponentInParent<LocalToAnchor>() != null && flow.end.GetComponentInParent<LocalToAnchor>().localTo == AnchoredObjects[i] &&
                    ((flow.end.GetComponent<LocalToAnchor>() != null && !flow.end.GetComponent<LocalToAnchor>().isInitialized()) ||
                    (flow.start.GetComponent<LocalToAnchor>() != null && !flow.start.GetComponent<LocalToAnchor>().isInitialized())))
                {
                    for (var j = 0; j < AnchoredObjects.Length; j++)
                    {
                        if (calibrating[j]) continue;
                        FlowVisualization[] objectFlows = AnchoredObjects[j].GetComponentsInChildren<FlowVisualization>();
                        foreach (FlowVisualization otherFlow in objectFlows)
                        {
                            if (otherFlow.end == flow.end)
                            {
                                otherFlow.InitializeFlow();
                            }
                        }
                    }
                }
            }
            /*
            FlowVisualization[] anchoredFlows = AnchoredObjects[i].GetComponentsInChildren<FlowVisualization>(true);
            foreach (FlowVisualization flow in anchoredFlows)
            {
                if (!flow.isInitialized())
                {
                    flow.InitializeFlow();
                }
            }*/
            if (allFlows != null)
            {
                foreach (FlowVisualization flow in allFlows)
                {
                    if (!flow.isInitialized()) flow.InitializeFlow();
                }
            }

        }
        catch (Exception e)
        {
            GameObject.Find("DebugText").GetComponent<Text>().text = "Finish Calibrating: " + e.Message;
        }
    }

    public Vector3 anchorPosition(int i)
    {
        return anchoredPositions[i];
    }

    /// <summary>
    /// Unregister callbacks and stop input API.
    /// </summary>
    void OnDestroy()
    {
        //MLInput.OnControllerButtonDown -= HandleOnButtonDown;
        /*
        if (MLPersistentCoordinateFrames.IsStarted)
        {
            MLPersistentCoordinateFrames.Stop();
        }

        if (MLPersistentStore.IsStarted)
        {
            MLPersistentStore.Stop();
        }*/

        if (_privilegeRequester != null)
        {
            _privilegeRequester.OnPrivilegesDone -= HandlePrivilegesDone;
        }
    }

    /// <summary>
    /// Cannot make the assumption that a privilege is still granted after
    /// returning from pause. Return the application to the state where it
    /// requests privileges needed and clear out the list of already granted
    /// privileges. Also, unregister callbacks.
    /// </summary>
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            //MLInput.OnControllerButtonDown -= HandleOnButtonDown;

            UpdateImageTrackerBehaviours(false);

            _hasStarted = false;
        }
    }

    #region Private Methods
    /// <summary>
    /// Enable/Disable the correct objects depending on view options
    /// </summary>
    void UpdateVisualizers()
    {
        foreach (ImageTrackingVisualizer visualizer in _visualizers)
        {
            visualizer.UpdateViewMode(_viewMode);
        }
    }

    /// <summary>
    /// Control when to enable to image trackers based on
    /// if the correct privileges are given.
    /// </summary>
    void UpdateImageTrackerBehaviours(bool enabled)
    {
        foreach (GameObject obj in TrackerBehaviours)
        {
            obj.SetActive(enabled);
        }
    }

    /// <summary>
    /// Once privileges have been granted, enable the camera and callbacks.
    /// </summary>
    void StartCapture()
    {
        if (!_hasStarted)
        {
            UpdateImageTrackerBehaviours(true);

            /*if (_visualizers.Length < 1)
            {
                GameObject.Find("DebugText").GetComponent<Text>().text = "no visualizers set";
                Debug.LogError("Error: ImageTrackingExample._visualizers is not set, disabling script.");
                enabled = false;
                return;
            }*/

            //MLInput.OnControllerButtonDown += HandleOnButtonDown;

            _hasStarted = true;
        }
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Responds to privilege requester result.
    /// </summary>
    /// <param name="result"/>
    void HandlePrivilegesDone(MLResult result)
    {
        if (!result.IsOk)
        {
            if (result.Code == MLResultCode.PrivilegeDenied)
            {
                Instantiate(Resources.Load("PrivilegeDeniedError"));
            }

            Debug.LogErrorFormat("Error: ImageTrackingExample failed to get requested privileges, disabling script. Reason: {0}", result);
            enabled = false;
            return;
        }
        StartCapture();
        /*result = MLPersistentStore.Start();
        if (!result.IsOk)
        {
            GameObject.Find("DebugText").GetComponent<Text>().text = "Persistent store bad";
            return;
        }
        result = MLPersistentCoordinateFrames.Start();
        if (!result.IsOk)
        {
            GameObject.Find("DebugText").GetComponent<Text>().text = "PCF Bad";
            return;
        }

        if (MLPersistentCoordinateFrames.IsReady)
        {
            PerformPersistenceStartup();
        }
        else
        {
            MLPersistentCoordinateFrames.OnInitialized += HandleInitialized;
        }*/
    }
    /*
    void HandleInitialized(MLResult status)
    {
        MLPersistentCoordinateFrames.OnInitialized -= HandleInitialized;

        if (status.IsOk)
        {
            PerformPersistenceStartup();
        }
    }

    private void PerformPersistenceStartup()
    {
        List<MLContentBinding> allBindings = MLPersistentStore.AllBindings;
        foreach (MLContentBinding binding in allBindings)
        {
            if (binding.GameObject.transform.name.Contains("Rack1") || binding.GameObject.transform.name.Contains("Rack2"))
            {
                GameObject.Find("DebugText").GetComponent<Text>().text += binding.GameObject.transform.name + ": ";
                GameObject.Find("DebugText").GetComponent<Text>().text += binding.GameObject.transform.position;
                GameObject.Find(binding.GameObject.transform.name).transform.position = binding.GameObject.transform.position;
                GameObject.Find(binding.GameObject.transform.name).transform.rotation = binding.GameObject.transform.rotation;
            }
        }
    }
    */
    /// <summary>
    /// Handles the event for button down.
    /// </summary>
    /// <param name="controllerId">The id of the controller.</param>
    /// <param name="button">The button that is being released.</param>
    private void HandleOnButtonDown(byte controllerId, MLInputControllerButton button)
    {
        if (_controllerConnectionHandler.IsControllerValid(controllerId) && button == MLInputControllerButton.Bumper)
        {
            _viewMode = (ViewMode)((int)(_viewMode + 1) % Enum.GetNames(typeof(ViewMode)).Length);
        }
        UpdateVisualizers();
    }

    public void ResetPositions()
    {
        currentIndex = 0;
        meshParent = FindObjectOfType<MLSpatialMapper>().meshParent;
        anchoredPositions = new Vector3[TrackerBehaviours.Length];
        anchoredLookAts = new Vector3[TrackerBehaviours.Length];

        totalPositions = new List<Vector3>[TrackerBehaviours.Length];
        totalLookAts = new List<Vector3>[TrackerBehaviours.Length];
        calibrating = new bool[TrackerBehaviours.Length];
        for (var i = 0; i < calibrating.Length; i++) startCalibrating(i);
        foreach (GameObject obj in AnchoredObjects)
        {
            obj.transform.position = Vector3.zero;
        }
    }
    #endregion
}
