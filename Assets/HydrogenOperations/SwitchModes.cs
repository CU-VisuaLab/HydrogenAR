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
        MLInput.OnTriggerDown += triggerDown;
        MLInput.OnTriggerUp += triggerUp;

        modeIndex = 0;
        modes = new string[] {"label", "PDF-graphs", "clear"};

        hydrogenPOIs = FindObjectsOfType<HydrogenPOI>();
        SetMode("label", true);
        SetMode("PDF-graphs", false);

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
            controller.StartFeedbackPatternVibe(MLInputControllerFeedbackPatternVibe.Buzz, MLInputControllerFeedbackIntensity.Medium);
            modeIndex = (modeIndex + 1) % modes.Length;
            GameObject.Find("StatusText").GetComponent<Text>().text = char.ToUpper(modes[modeIndex][0]) + modes[modeIndex].Substring(1);
            Invoke("ClearHUDText", 5);
            Invoke("StopVibrating", 0.5f);
        }
        else if (touchpad.Direction == MLInputControllerTouchpadGestureDirection.Down)
        {
            controller.StartFeedbackPatternVibe(MLInputControllerFeedbackPatternVibe.Buzz, MLInputControllerFeedbackIntensity.Medium);
            modeIndex = (modeIndex + modes.Length - 1) % modes.Length;
            GameObject.Find("StatusText").GetComponent<Text>().text = char.ToUpper(modes[modeIndex][0]) + modes[modeIndex].Substring(1);
            Invoke("ClearHUDText", 5);
            Invoke("StopVibrating", 0.5f);
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

    private void SetMode(string modeString, bool activate)
    {
        if (modeString == "label")
        {
            foreach (HydrogenPOI poi in hydrogenPOIs) poi.metricObject.SetActive(activate);
        }
        else if (modeString == "PDF-graphs")
        {
            foreach (HydrogenPOI poi in hydrogenPOIs) poi.timeSeriesObject.SetActive(activate);
        }
    }
    void ClearHUDText()
    {
        GameObject.Find("StatusText").GetComponent<Text>().text = "";
    }
}
