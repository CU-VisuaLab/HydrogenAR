using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using System;

public class OneDimensionalMenu : MonoBehaviour
{
    public GameObject MenuPrefab;
    private MLControllerRadialMenu previousMenu;
    private GameObject menuObject;
    private bool active;
    private float originalThumbY;
    private float initialButtonY;
    private bool thumbDown;
    private ControllerConnectionHandler _controllerConnectionHandler;
    private Button[] buttons;
    private Button hoveredButton;

    // Start is called before the first frame update
    void Start()
    {
        active = false;
        thumbDown = false;
        _controllerConnectionHandler = FindObjectOfType<ControllerConnectionHandler>(); // Assumes there is only one
        //Activate(FindObjectOfType<MLControllerRadialMenu>());
        MLInput.OnTriggerDown += HandleOnTriggerDown;
        MLInput.OnTriggerUp += HandleOnTriggerUp;
        MLInput.OnControllerButtonDown += HandleOnButtonDown;
        MLInput.OnControllerButtonUp += HandleOnButtonUp;
    }

    // Update is called once per frame
    void Update()
    {
        MLInputController controller = _controllerConnectionHandler.ConnectedController;

        if (active && controller.Touch1Active)
        {
            GetButton();
        }
    }

    public void Activate(MLControllerRadialMenu menu)
    {
        previousMenu = menu; 
        if (previousMenu != null) previousMenu.usingSubMenu(true);
        active = true;
        thumbDown = false;

        if (_controllerConnectionHandler.ConnectedController != null)
            initialButtonY = _controllerConnectionHandler.ConnectedController.Touch1PosAndForce.y;
        menuObject = Instantiate(MenuPrefab);
        menuObject.GetComponent<Canvas>().worldCamera = Camera.main;
        buttons = menuObject.GetComponentsInChildren<Button>();

        if (menuObject.transform.name.ToLower().Contains("metric")) FindObjectOfType<UpdateMetric>().initializeButtons(menuObject);
        else if (menuObject.transform.name.ToLower().Contains("timewindow")) FindObjectOfType<UpdateTimeWindow>().initializeButtons(menuObject);
    }

    private void HandleOnTriggerDown(byte controllerId, float value)
    {
        try
        {
            if (!active || hoveredButton == null) return;
            //string metric = hoveredButton.transform.name;
            if (hoveredButton != null && hoveredButton.transform.name.ToLower() != "back")
            {
                hoveredButton.onClick.Invoke();
                if (menuObject.transform.name.ToLower().Contains("metric"))
                {
                    HostMetrics[] metrics = FindObjectsOfType<HostMetrics>();
                    foreach (HostMetrics metric in metrics)
                    {
                        metric.UpdateMetric(hoveredButton.transform.name);
                    }
                }
            }
            if (previousMenu != null) previousMenu.usingSubMenu(false);
            Destroy(menuObject);
            if (hoveredButton.transform.name.ToLower() == "back") previousMenu.SetActiveMenu(true);
            hoveredButton = null;
            active = false;
        }
        catch (Exception e)
        {
            GameObject.Find("DebugText").GetComponent<Text>().text = "1D Menu: " + e.Message;
        }
    }
    private void HandleOnTriggerUp(byte controllerId, float value)
    {
    }

    private void HandleOnButtonDown(byte controllerId, MLInputControllerButton button)
    {
        if (active && button == MLInputControllerButton.HomeTap)
        {
            Destroy(menuObject);
            active = false;
            hoveredButton = null;
            if (previousMenu != null) previousMenu.usingSubMenu(false);
        }
    }
    private void HandleOnButtonUp(byte controllerId, MLInputControllerButton button)
    {
    }

    private void GetButton()
    {
        float min_y = Mathf.Infinity;
        float max_y = -Mathf.Infinity;
        float average = 0;

        foreach (Button button in buttons)
        {
            min_y = Mathf.Min(min_y, button.transform.localPosition.y);
            max_y = Mathf.Max(max_y, button.transform.localPosition.y);
            average += button.transform.localPosition.y / buttons.Length;
        }
        float menuMappedThumbPos = 1.25f * _controllerConnectionHandler.ConnectedController.Touch1PosAndForce.y / 2 * (max_y - min_y) + average;
        float closestDist = Mathf.Infinity;
        foreach (Button button in buttons)
        {
            button.transform.GetComponent<Image>().color = Color.white;
            if (Mathf.Abs(button.transform.localPosition.y - menuMappedThumbPos) < closestDist)
            {
                closestDist = Mathf.Abs(button.transform.localPosition.y - menuMappedThumbPos);
                hoveredButton = button;
            }
        }
        hoveredButton.transform.GetComponent<Image>().color = Color.green;
    }
}
