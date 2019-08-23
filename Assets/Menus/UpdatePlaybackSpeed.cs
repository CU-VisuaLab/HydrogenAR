using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using UnityEngine.UI;
using System.Globalization;
using System;

public class UpdatePlaybackSpeed : MonoBehaviour
{
    MLControllerRadialMenu previousMenu;
    GameObject playbackSlider;
    private bool active;
    private float previousThumbX;
    private bool thumbDown;
    private ControllerConnectionHandler _controllerConnectionHandler;
    private float timeThreshold;
    private float playbackSpeed;
    private float sliderPosition;
    private float initiatedTime;
    // Start is called before the first frame update
    void Awake()
    {
        active = false;
        thumbDown = false;
        playbackSpeed = 1;
        sliderPosition = 75;
        previousThumbX = Mathf.NegativeInfinity;
        _controllerConnectionHandler = FindObjectOfType<ControllerConnectionHandler>(); // Assumes there is only one
        MLInput.OnTriggerDown += HandleOnTriggerDown;
        MLInput.OnTriggerUp += HandleOnTriggerUp;
    }

    void UpdateIt()
    {
        HostMetrics[] hosts = FindObjectsOfType<HostMetrics>();
        foreach (HostMetrics host in hosts)
        {
            host.UpdatePlaybackSpeed(1f);
        }

        WIMFocusObject[] wimFocusObjects = Resources.FindObjectsOfTypeAll(typeof(WIMFocusObject)) as WIMFocusObject[];
        foreach (WIMFocusObject wimFocus in wimFocusObjects)
        {
            wimFocus.UpdatePlaybackSpeed(1f);
        }
        /*previousMenu.usingSubMenu(false);
        Destroy(playbackSlider);*/
        active = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            MLInputController controller = _controllerConnectionHandler.ConnectedController;

            float sliderBarWidth = playbackSlider.transform.Find("BaseBar").GetComponent<RectTransform>().rect.width * playbackSlider.transform.Find("BaseBar").localScale.x;
            if (!controller.Touch1Active)
            {
                thumbDown = false;
            }
            else if (thumbDown)
            {
                float delta = controller.Touch1PosAndForce.x - previousThumbX;
                previousThumbX = controller.Touch1PosAndForce.x;
                playbackSlider.transform.Find("Slider").localPosition = playbackSlider.transform.Find("Slider").localPosition + new Vector3(200 * delta, 0, 0);
                if (playbackSlider.transform.Find("Slider").localPosition.x > sliderBarWidth / 2)
                {
                    playbackSlider.transform.Find("Slider").localPosition = new Vector3(375, 0, 0);
                }
                else if (playbackSlider.transform.Find("Slider").localPosition.x < -sliderBarWidth / 2)
                {
                    playbackSlider.transform.Find("Slider").localPosition = new Vector3(-375, 0, 0);
                }
                sliderPosition = playbackSlider.transform.Find("Slider").transform.localPosition.x;
                //UpdateSliderValueLogarithm(playbackSlider.transform.Find("Slider").localPosition.x / sliderBarWidth + 0.5f);
                UpdateSliderValueLinear(playbackSlider.transform.Find("Slider").localPosition.x / sliderBarWidth + 0.5f);
            }
            else if (_controllerConnectionHandler.ConnectedController.Touch1Active)
            {
                thumbDown = true;
                previousThumbX = controller.Touch1PosAndForce.x;
            }
        }
    }

    private void UpdateSliderValueLinear(float percentage)
    {

        string leftText = playbackSlider.transform.Find("LeftLabel").GetComponent<Text>().text.Trim(new char[] { 'x', ' ' });
        string rightText = playbackSlider.transform.Find("RightLabel").GetComponent<Text>().text.Trim(new char[] { 'x', ' ' });
        float minVal = float.Parse(leftText);
        float maxVal = float.Parse(rightText);
        playbackSpeed = minVal + percentage * (maxVal - minVal);
        playbackSlider.transform.Find("Slider/SliderLabel").GetComponent<Text>().text = string.Format("{0:N2}", playbackSpeed);
    }

    private void UpdateSliderValueLogarithm(float percentage)
    {
        string leftText = playbackSlider.transform.Find("LeftLabel").GetComponent<Text>().text.Trim(new char[] { 'x', ' ' });
        string rightText = playbackSlider.transform.Find("RightLabel").GetComponent<Text>().text.Trim(new char[] { 'x', ' ' });
        float minExponent = float.Parse(leftText);
        float maxExponent = float.Parse(rightText.Remove(rightText.Length - 1)); // For some reason there's a whitespace at the end

        float minLog = Mathf.Log10(minExponent);
        float maxLog = Mathf.Log10(maxExponent);

        float logValue = percentage * (maxLog - minLog) + minLog;
        playbackSpeed = Mathf.Pow(10f, logValue);
        playbackSlider.transform.Find("Slider/SliderLabel").GetComponent<Text>().text = string.Format("{0:N2}",playbackSpeed);
    }

    public void Activate(MLControllerRadialMenu menu)
    {
        previousMenu = menu;
        previousMenu.usingSubMenu(true);
        active = true;
        thumbDown = false;

        playbackSlider = Instantiate(Resources.Load("Prefabs/PlaybackSpeed") as GameObject);
        playbackSlider.GetComponent<Canvas>().worldCamera = Camera.main;

        playbackSlider.transform.Find("Slider").localPosition = new Vector3(sliderPosition, 0, 0);
        playbackSlider.transform.Find("Slider/SliderLabel").GetComponent<Text>().text = string.Format("{0:N2}", playbackSpeed);
         
        initiatedTime = Time.time;
    }
    private void HandleOnTriggerDown(byte controllerId, float value)
    {
        if (!active || Mathf.Abs(Time.time - initiatedTime) < 0.2f) return;
        HostMetrics[] hosts = FindObjectsOfType<HostMetrics>();
        foreach (HostMetrics host in hosts)
        {
            host.UpdatePlaybackSpeed(playbackSpeed);
        }
        WIMFocusObject[] wimFocusObjects = Resources.FindObjectsOfTypeAll(typeof(WIMFocusObject)) as WIMFocusObject[];
        foreach(WIMFocusObject wimFocus in wimFocusObjects)
        {
            wimFocus.UpdatePlaybackSpeed(playbackSpeed);
        }

        previousMenu.usingSubMenu(false);
        Destroy(playbackSlider);
        active = false;
    }
    private void HandleOnTriggerUp(byte controllerId, float value)
    {
    }
}
