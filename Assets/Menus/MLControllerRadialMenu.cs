using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

public class MLControllerRadialMenu : MonoBehaviour
{

    #region Private Variables
    private ControllerConnectionHandler _controllerConnectionHandler;
    private bool pressedDown;
    private float pressThresholdTime;

    private GameObject hoveredButton;
    private GameObject pressedButton;

    private float[] angles;
    private const float SCALE_FACTOR = 100f;
    #endregion

    public GameObject centerButton;
    public GameObject[] radialButtons;

    public Color defaultColor;
    public Color hoverColor;

    public float dwellThreshold = 0.5f;

    private bool subMenuInUse;

    public enum MenuClickStyle { Hover, Trigger };
    public MenuClickStyle clickStyle;
    // Start is called before the first frame update
    void Start()
    {
        pressedDown = false;
        _controllerConnectionHandler = FindObjectOfType<ControllerConnectionHandler>(); // Assumes there is only one
        centerButton.transform.localPosition = Vector3.zero;
        angles = new float[radialButtons.Length];
        for (var i = 0; i < radialButtons.Length; i++)
        {
            Vector3 buttonPos = 1.25f * new Vector3(Mathf.Sin(2 * Mathf.PI * i / radialButtons.Length), Mathf.Cos(2 * Mathf.PI * i / radialButtons.Length), 0) * SCALE_FACTOR;
            radialButtons[i].transform.localPosition = buttonPos;
        }


        MLInput.OnControllerButtonUp += HandleOnButtonUp;
        MLInput.OnControllerButtonDown += HandleOnButtonDown;
        MLInput.OnTriggerDown += HandleOnTriggerDown;
        MLInput.OnTriggerUp += HandleOnTriggerUp;

        centerButton.GetComponent<Button>().onClick.AddListener(() => SetActiveMenu(false));
        centerButton.GetComponent<Button>().onClick.Invoke();

        subMenuInUse = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_controllerConnectionHandler.IsControllerValid() || subMenuInUse)
        {
            return;
        }
        if (!centerButton.activeSelf && clickStyle == MenuClickStyle.Trigger) return;
        GameObject currentButton = getHoveredButton();

        // If hover style, check if there is a button to confirm
        if (clickStyle == MenuClickStyle.Hover && currentButton == null)
        {
            if (pressedButton != null)
            {
                if (Time.time > pressThresholdTime)
                {
                    // Press that button
                    pressedButton.GetComponent<Button>().onClick.Invoke();
                }
            }
            pressedDown = false;
            hoveredButton = null;
            SetActiveMenu(false);
        }
        else if (currentButton != null)
        {
            hoveredButton = currentButton;
        }
    }

    private GameObject getHoveredButton()
    {
        MLInputController controller = _controllerConnectionHandler.ConnectedController;
        GameObject currentButton = null;
        if (controller.Touch1Active)
        {
            if (clickStyle == MenuClickStyle.Hover)
            {
                if (!pressedDown) pressThresholdTime = Time.time + dwellThreshold;
                pressedDown = true;
                SetActiveMenu(true);
            }
            if (Vector2.SqrMagnitude(new Vector2(controller.Touch1PosAndForce.x, controller.Touch1PosAndForce.y)) < 0.25f)
            {
                currentButton = centerButton;
            }
            else
            {
                float closestDistance = Mathf.Infinity;
                Vector2 thumbPos = new Vector2(controller.Touch1PosAndForce.x, controller.Touch1PosAndForce.y);
                foreach (GameObject button in radialButtons)
                {
                    float distance = Vector2.Distance(new Vector2(button.transform.localPosition.x, button.transform.localPosition.y), thumbPos);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        currentButton = button;
                    }
                }
            }
            foreach (GameObject button in radialButtons)
            {
                if (button == currentButton) button.GetComponent<Image>().color = hoverColor;
                else button.GetComponent<Image>().color = defaultColor;
            }
            centerButton.GetComponent<Image>().color = centerButton == currentButton ? hoverColor : defaultColor;
            return currentButton;
        }
        else
        {
            pressedButton = hoveredButton;
            return null;
        }
    }

    public void usingSubMenu(bool usage)
    {
        subMenuInUse = usage;
        if (!subMenuInUse) hoveredButton = centerButton;
    }

    public void SetActiveMenu(bool active)
    {
        foreach (GameObject button in radialButtons) button.SetActive(active);
        centerButton.SetActive(active);
        hoveredButton = null;

        foreach (GameObject button in radialButtons) button.GetComponent<Image>().color = defaultColor;
        centerButton.GetComponent<Image>().color = defaultColor;
    }

    private void HandleOnButtonDown(byte controllerId, MLInputControllerButton button)
    {
        if (subMenuInUse) return;
        if (_controllerConnectionHandler.IsControllerValid() && _controllerConnectionHandler.ConnectedController.Id == controllerId &&
            button == MLInputControllerButton.HomeTap && clickStyle == MenuClickStyle.Trigger)
        {
            hoveredButton = centerButton;
            SetActiveMenu(!centerButton.activeSelf);
        }
    }
    private void HandleOnButtonUp(byte controllerId, MLInputControllerButton button)
    {
    }
    private void HandleOnTriggerDown(byte controllerId, float value)
    {
        try
        {
            if (subMenuInUse || hoveredButton == null) return;

            if (clickStyle == MenuClickStyle.Trigger)
            {
                hoveredButton.GetComponent<Button>().onClick.Invoke();
                SetActiveMenu(false);
                hoveredButton = centerButton;
            }
        }
        catch(Exception e)
        {
            GameObject.Find("DebugText").GetComponent<Text>().text = "MainMenu: " + e.Message;
        }
    }
    private void HandleOnTriggerUp(byte controllerId, float value)
    {
    }
}
