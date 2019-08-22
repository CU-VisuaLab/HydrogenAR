// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%
//
// Modified by Matt Whitlock

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using MagicLeap;
using System.Collections.Generic;
using static MagicLeap.ImageTrackingExample;

//namespace MagicLeap
//{
/// <summary>
/// This provides an example of interacting with the image tracker visualizers using the controller
/// </summary>
[RequireComponent(typeof(PrivilegeRequester))]
public class VisImageTracking : MonoBehaviour
{
    public GameObject[] TrackerBehaviours;

    #region Private Variables
    private Vector3[] trackedObjectPositions;

    private ViewMode _viewMode = ViewMode.All;

    [SerializeField, Tooltip("Image Tracking Visualizers to control")]
    private ImageTrackingVisualizer[] _visualizers = null;

    [Space, SerializeField, Tooltip("ControllerConnectionHandler reference.")]
    private ControllerConnectionHandler _controllerConnectionHandler = null;

    private PrivilegeRequester _privilegeRequester = null;

    private bool _hasStarted = false;

    private MLWorldPlane[] _planes;
    private GameObject debugCube;
    #endregion

    #region Unity Methods

    // Using Awake so that Privileges is set before PrivilegeRequester Start
    void Awake()
    {
        if (_controllerConnectionHandler == null)
        {
            Debug.LogError("Error: ImageTrackingExample._controllerConnectionHandler is not set, disabling script.");
            enabled = false;
            return;
        }

        // If not listed here, the PrivilegeRequester assumes the request for
        // the privileges needed, CameraCapture in this case, are in the editor.
        _privilegeRequester = GetComponent<PrivilegeRequester>();

        // Before enabling the MLImageTrackerBehavior GameObjects, the scene must wait until the privilege has been granted.
        _privilegeRequester.OnPrivilegesDone += HandlePrivilegesDone;

        debugCube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        debugCube.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        trackedObjectPositions = new Vector3[TrackerBehaviours.Length];
        for (var i = 0; i < trackedObjectPositions.Length;  i++) 
        {
            trackedObjectPositions[i] = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        }
    }

    private void Update()
    {
        if (_planes.Length == 0) return;
        
        for (var i = 0; i < TrackerBehaviours.Length; i++) 
        {
            GameObject obj = TrackerBehaviours[i];
            MLWorldPlane closestPlane = _planes[0];
            foreach (MLWorldPlane plane in _planes)
            {
                if (Vector3.Distance(plane.Center, obj.transform.position) < Vector3.Distance(closestPlane.Center, obj.transform.position))
                {
                    closestPlane = plane;
                }
            }

            debugCube.transform.position = closestPlane.Center;
            debugCube.transform.rotation = closestPlane.Rotation;
            Vector3 proposedPosition = debugCube.transform.position - 0.25f * debugCube.transform.forward;
            if (Vector3.Distance(proposedPosition, trackedObjectPositions[i]) > 0.75f)
            {
                trackedObjectPositions[i] = proposedPosition;
            }
            Vector3 forwardPoint = debugCube.transform.position + new Vector3(debugCube.transform.forward.x, 0, debugCube.transform.forward.z);
            obj.transform.rotation = new Quaternion(0, 0, 0, 0);
            obj.GetComponent<ImageTrackingVisualizer>()._demo.transform.position = trackedObjectPositions[i];
            obj.SetActive(true);
        }
    }

    /// <summary>
    /// Unregister callbacks and stop input API.
    /// </summary>
    void OnDestroy()
    {
        MLInput.OnControllerButtonDown -= HandleOnButtonDown;
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
            MLInput.OnControllerButtonDown -= HandleOnButtonDown;

            UpdateImageTrackerBehaviours(false);

            _hasStarted = false;
        }
    }
    #endregion

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

            if (_visualizers.Length < 1)
            {
                Debug.LogError("Error: ImageTrackingExample._visualizers is not set, disabling script.");
                enabled = false;
                return;
            }

            MLInput.OnControllerButtonDown += HandleOnButtonDown;

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

        Debug.Log("Succeeded in requesting all privileges");
        StartCapture();
    }

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
    public void HandleOnPlanesUpdate(MLWorldPlane[] planes, MLWorldPlaneBoundaries[] boundaries)
    {
        _planes = planes;
        GameObject.Find("DebugText").GetComponent<Text>().text = "";
        foreach(MLWorldPlane plane in planes)
        {
            GameObject.Find("DebugText").GetComponent<Text>().text += (plane.Rotation.eulerAngles + "\n");
        }
    }
#endregion
}
