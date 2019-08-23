using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

public class MLVisTransformController : MonoBehaviour
{
    #region Private Variables
    private ControllerConnectionHandler _controllerConnectionHandler;

    private int _lastLEDindex = -1;
    #endregion

    #region Const Variables
    private const float TRIGGER_DOWN_MIN_VALUE = 0.2f;

    // UpdateLED - Constants
    private const float HALF_HOUR_IN_DEGREES = 15.0f;
    private const float DEGREES_PER_HOUR = 12.0f / 360.0f;

    private const int MIN_LED_INDEX = (int)(MLInputControllerFeedbackPatternLED.Clock12);
    private const int MAX_LED_INDEX = (int)(MLInputControllerFeedbackPatternLED.Clock6And12);
    private const int LED_INDEX_DELTA = MAX_LED_INDEX - MIN_LED_INDEX;

    private bool triggerDown;
    private bool touchpadDown;
    private GameObject laser;
    private Vector3 hitPoint;
    private GameObject hoveringObject;
    private float draggingDistance;
    private Vector3 dragOffset;
    private float touchPosition;
    private MLInputController controller;

    public enum ControllerLaserMode { PickAndPlace, PullUpDetails };
    public ControllerLaserMode mode = ControllerLaserMode.PullUpDetails;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        triggerDown = false;
        touchpadDown = false;
        touchPosition = Mathf.NegativeInfinity; 
        laser = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        laser.transform.name = "Laser";
        laser.transform.GetComponent<Collider>().enabled = false;
        laser.transform.parent = transform;
        laser.transform.localEulerAngles = new Vector3(90, 0, 0);
        _controllerConnectionHandler = GetComponent<ControllerConnectionHandler>();

        MLInput.OnTriggerDown += HandleOnTriggerDown;
        MLInput.OnTriggerUp += HandleOnTriggerUp;

        controller = GetComponent<ControllerConnectionHandler>().ConnectedController;
    }

    // Update is called once per frame
    void Update()
    {
        GameObject.Find("DebugText").GetComponent<Text>().text = "";
        if (triggerDown && mode == ControllerLaserMode.PickAndPlace)
        {
            UpdateDragging();
        }
        else UpdateRaycast();
    }

    private void HandleOnTriggerDown(byte controllerId, float value)
    {
        if (mode == ControllerLaserMode.PickAndPlace && hoveringObject != null)
        {
            triggerDown = true;
            draggingDistance = 2 * laser.transform.localScale.y;
        }

        // Manually invoke pulling up details
        else if (mode == ControllerLaserMode.PullUpDetails && hoveringObject != null && hoveringObject.GetComponentInParent<GazeFocusable>() != null)
        {
            hoveringObject.GetComponentInParent<GazeFocusable>().InvokeGaze(transform.position + 2 * laser.transform.localScale.y * transform.forward, -transform.forward); 
        }
    }
    private void HandleOnTriggerUp(byte controllerId, float value)
    {
        //triggerDown = false;
    }
    private void UpdateDragging()
    {
        if (controller == null) controller = GetComponent<ControllerConnectionHandler>().ConnectedController;
        float proposedDraggingDistance = draggingDistance;
        // Check if the user is adjusting the drag distance;
        if (controller.Touch1PosAndForce.z > 0.05f)
        {
            touchpadDown = true;
            if (!float.IsNegativeInfinity(touchPosition))
            {
                proposedDraggingDistance += (controller.Touch1PosAndForce.y - touchPosition);
            }
            touchPosition = controller.Touch1PosAndForce.y;
        }
        else touchPosition = Mathf.NegativeInfinity;

        Vector3 deltaPos = transform.position + proposedDraggingDistance * transform.forward + dragOffset - hoveringObject.transform.position;
        RaycastHit testHit, hit;
        if (!hoveringObject.GetComponent<Rigidbody>().SweepTest(deltaPos, out testHit, Vector3.Magnitude(deltaPos)))
        {

            hoveringObject.transform.Translate(deltaPos, Space.World);
            hoveringObject.transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
            draggingDistance = proposedDraggingDistance;
            laser.transform.localScale = new Vector3(0.005f, draggingDistance / 2, 0.005f);
            laser.transform.localPosition = new Vector3(0, 0, draggingDistance / 2);
        }
        else
        {
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 20))
            {
                dragOffset = hit.transform.GetComponentInParent<MLVisTransform>().transform.position - hit.point;
            }
            else triggerDown = false;   
        }
    }
    private void UpdateRaycast()
    {
        GameObject.Find("HostText").GetComponent<Text>().text = "";
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 20))
        {
            laser.transform.localScale = new Vector3(0.005f, Vector3.Distance(transform.position, hit.point) / 2, 0.005f);
            laser.transform.position = (transform.position + hit.point) / 2;
            hoveringObject = hit.transform.gameObject;
            if (hit.transform.GetComponentInParent<MLVisTransform>())
            {
                laser.GetComponent<Renderer>().material.color = Color.blue;
                hoveringObject = hit.transform.GetComponentInParent<MLVisTransform>().gameObject;

                if (hit.transform.GetComponent<DetailHover>() != null)
                {
                    hit.transform.GetComponent<DetailHover>().showDetail();
                }

                if (!triggerDown) dragOffset = hit.transform.GetComponentInParent<MLVisTransform>().transform.position - hit.point;
            }
            else
            {
                hoveringObject = null;
                laser.GetComponent<Renderer>().material.color = Color.green;
            }
        }
        else
        {
            hoveringObject = null;
            laser.transform.localScale = new Vector3(0.005f, 20, 0.005f);
            laser.transform.localPosition = new Vector3(0, 0, 20);
            laser.GetComponent<Renderer>().material.color = Color.red;
        }
    }
}
