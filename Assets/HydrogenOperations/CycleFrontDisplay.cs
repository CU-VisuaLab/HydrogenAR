using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class CycleFrontDisplay : MonoBehaviour
{
    private int currentIndex;
    [Tooltip("If left empty, it will initialize to every child object")]
    public GameObject[] FrontDisplayViews;

    private MLInputController controller;
    // Start is called before the first frame update
    void Start()
    {
        controller = FindObjectOfType<ControllerConnectionHandler>().ConnectedController;
        MLInput.OnControllerTouchpadGestureEnd += ChangeView;
        foreach(GameObject obj in FrontDisplayViews)
        {
            obj.SetActive(false);
        }
        if (FrontDisplayViews.Length == 0)
        {
            FrontDisplayViews = new GameObject[transform.childCount];
            for (var i = 0; i < FrontDisplayViews.Length; i++)
            {
                FrontDisplayViews[i] = transform.GetChild(i).gameObject;
            }
        }
        if (FrontDisplayViews.Length > 0)
        {
            FrontDisplayViews[0].SetActive(true);
            for (var i = 1; i < FrontDisplayViews.Length; i++)
            {
                FrontDisplayViews[i].SetActive(false);
            }
        }
        currentIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void ChangeView(byte controllerId, MLInputControllerTouchpadGesture touchpad)
    {
        if (touchpad.Direction == MLInputControllerTouchpadGestureDirection.Right)
        {
            cycleView(1);
        }
        else if (touchpad.Direction == MLInputControllerTouchpadGestureDirection.Left)
        {
            cycleView(-1);
        }
    }
    private void cycleView(int indexIncrementer)
    {
        if (FrontDisplayViews.Length == 0) return;
        FrontDisplayViews[currentIndex].SetActive(false);
        currentIndex = (currentIndex + FrontDisplayViews.Length + indexIncrementer) % FrontDisplayViews.Length;
        FrontDisplayViews[currentIndex].SetActive(true);
        int patternNumber = (int)(12 * (float)currentIndex / FrontDisplayViews.Length);
        if (currentIndex % 2 == 0)
            controller.StartFeedbackPatternLED((MLInputControllerFeedbackPatternLED)patternNumber, MLInputControllerFeedbackColorLED.PastelMysticBlue, 60);
        else
            controller.StartFeedbackPatternLED((MLInputControllerFeedbackPatternLED)patternNumber, MLInputControllerFeedbackColorLED.BrightLunaYellow, 60);
    }
}
