using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using System;

public class SwitchModes : MonoBehaviour
{
    private MLInputController controller;
    private string[] modes;
    private int modeIndex;
    private HydrogenPOI[] hydrogenPOIs;

    private Vector3 prevPosition;
    private bool dragging;
    // Start is called before the first frame update
    void Start()
    {
        controller = FindObjectOfType<ControllerConnectionHandler>().ConnectedController;
        MLInput.OnControllerTouchpadGestureEnd += switchMode;
        MLInput.OnControllerButtonDown += resetTank;
        MLInput.OnTriggerDown += triggerDown;
        MLInput.OnTriggerUp += triggerUp;

        modeIndex = 0;
        modes = new string[] {"label", "time-series", "clear"};

        hydrogenPOIs = FindObjectsOfType<HydrogenPOI>();
        SetMode("label", true);
        SetMode("time-series", false);

        dragging = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (dragging)
        {
            FindObjectOfType<CarFillup>().moveCar(FindObjectOfType<ControllerConnectionHandler>().transform.position - prevPosition);
            prevPosition = FindObjectOfType<ControllerConnectionHandler>().transform.position;
        }
    }

    private void switchMode(byte controllerId, MLInputControllerTouchpadGesture touchpad)
    {
        SetMode(modes[modeIndex], false);
        if (touchpad.Direction == MLInputControllerTouchpadGestureDirection.Up)
        {
            modeIndex = (modeIndex + 1) % modes.Length;
            GameObject.Find("DebugText").GetComponent<Text>().text = char.ToUpper(modes[modeIndex][0]) + modes[modeIndex].Substring(1);
            Invoke("ClearHUDText", 5);
        }
        else if (touchpad.Direction == MLInputControllerTouchpadGestureDirection.Down)
        {
            modeIndex = (modeIndex + modes.Length - 1) % modes.Length;
            GameObject.Find("DebugText").GetComponent<Text>().text = char.ToUpper(modes[modeIndex][0]) + modes[modeIndex].Substring(1);
            Invoke("ClearHUDText", 5);
        }
        SetMode(modes[modeIndex], true);
    }

    private void triggerDown(byte controllerId, float value)
    {
        prevPosition = FindObjectOfType<ControllerConnectionHandler>().transform.position;
        dragging = true;
    }
    private void triggerUp(byte controllerId, float value)
    {
        dragging = false;
    }

    private void resetTank(byte controllerId, MLInputControllerButton button)
    {
        if (button == MLInputControllerButton.HomeTap)
        {
            FindObjectOfType<CarFillup>().resetTank();
            GameObject.Find("DebugText").GetComponent<Text>().text = "New Tank";
            Invoke("ClearHUDText", 3);
        }
    }

    private void SetMode(string modeString, bool activate)
    {
        if (modeString == "label")
        {
            foreach (HydrogenPOI poi in hydrogenPOIs) poi.metricObject.SetActive(activate);
        }
        else if (modeString == "time-series")
        {
            foreach (HydrogenPOI poi in hydrogenPOIs) poi.timeSeriesObject.SetActive(activate);
        }
    }
    void ClearHUDText()
    {
        GameObject.Find("DebugText").GetComponent<Text>().text = "";
    }
}
