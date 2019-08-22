using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

public class WIMOperations : MonoBehaviour
{

    private ControllerConnectionHandler _controllerConnectionHandler;
    private bool holdingWIM;
    private const float WIM_PICKUP_THRESHOLD = 0.02f;
    private Vector3 previousControllerPosition;
    private GameObject focusObjectHeld;

    // Start is called before the first frame update
    void Start()
    {
        holdingWIM = false;
        focusObjectHeld = null;
        _controllerConnectionHandler = FindObjectOfType<ControllerConnectionHandler>();
        MLInput.OnControllerButtonUp += HandleOnButtonUp;
        MLInput.OnControllerButtonDown += HandleOnButtonDown;
        MLInput.OnTriggerDown += HandleOnTriggerDown;
        MLInput.OnTriggerUp += HandleOnTriggerUp;
    }

    // Update is called once per frame
    void Update()
    {
        GameObject.Find("DebugText").GetComponent<Text>().text = "";
        if (holdingWIM)
        {
            GameObject.Find("WIM_Model").transform.position += (transform.position - previousControllerPosition);
            previousControllerPosition = transform.position;
        }
        else if (focusObjectHeld)
        {
            focusObjectHeld.transform.position += (transform.position - previousControllerPosition);
            previousControllerPosition = transform.position;
        }
    }

    public void cycleAllObjects()
    {
        WIMFocusObject[] focusObjects = FindObjectsOfType<WIMFocusObject>();
        foreach (WIMFocusObject focusObject in focusObjects)
        {
            focusObject.cycleFocusObject();
        }
    }
    private void HandleOnButtonDown(byte controllerId, MLInputControllerButton button)
    {
        MLInputController controller = _controllerConnectionHandler.ConnectedController;
        if (button != MLInputControllerButton.Bumper) return;
        if (controller != null && controller.Id == controllerId && 
            (Vector3.Distance(Camera.main.transform.position, GameObject.Find("WIM_Model").transform.position) < 0.5f) ||
            (Vector3.Angle(GameObject.Find("WIM_Model").transform.position - Camera.main.transform.position, Camera.main.transform.forward) < 35 &&
            Vector3.Distance(Camera.main.transform.position, GameObject.Find("WIM_Model").transform.position) < 1.5f))
        {
            cycleAllObjects();
        }
    }

    private void HandleOnButtonUp(byte controllerId, MLInputControllerButton button)
    {
    }

    private void HandleOnTriggerDown(byte controllerId, float value)
    {
        if (GameObject.Find("WIM_Model/[Pieces]") == null) Debug.LogError("Need a subobject called 'WIM_Model/[Pieces]'");
        //MLInputController controller = _controllerConnectionHandler.ConnectedController;
        foreach (Transform piece in GameObject.Find("WIM_Model/[Pieces]").transform)
        {
            if (piece.GetComponent<Collider>() != null && Vector3.Distance(piece.GetComponent<Collider>().ClosestPoint(transform.position), transform.position) < WIM_PICKUP_THRESHOLD)
            {
                holdingWIM = true;
                previousControllerPosition = transform.position;
                return;
            }
            foreach (Transform grandchild in piece)
            {
                if (grandchild.GetComponent<Collider>() != null && Vector3.Distance(grandchild.GetComponent<Collider>().ClosestPoint(transform.position), transform.position) < WIM_PICKUP_THRESHOLD)
                {
                    holdingWIM = true;
                    previousControllerPosition = transform.position;
                    return;
                }
            }
        }

        WIMFocusObject[] WIMFocusObjects = FindObjectsOfType<WIMFocusObject>();
        foreach (WIMFocusObject focusObject in WIMFocusObjects)
        {
            Collider colliderInUse = null;
            foreach (GameObject candidateObject in focusObject.focusObjects)
            {
                if (candidateObject.activeSelf) colliderInUse = candidateObject.GetComponent<Collider>();
            }
            if (Vector3.Distance(colliderInUse.ClosestPoint(transform.position), transform.position) < WIM_PICKUP_THRESHOLD)
            {
                focusObjectHeld = colliderInUse.gameObject;
                previousControllerPosition = transform.position;
                return;
            }
        }
    }
    private void HandleOnTriggerUp(byte controllerId, float value)
    {
        holdingWIM = false;
        focusObjectHeld = null;
    }
}
