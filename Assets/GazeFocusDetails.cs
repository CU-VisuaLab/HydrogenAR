using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class GazeFocusDetails : MonoBehaviour
{
    private List<GameObject> detailObjects;
    private int currentIndex;
    MLInputController controller;
    // Start is called before the first frame update
    void Awake()
    {
        controller = FindObjectOfType<ControllerConnectionHandler>().ConnectedController;
        MLInput.OnControllerButtonDown += HandleOnButtonDown;
        MLInput.OnControllerTouchpadGestureEnd += HandleOnTouchpadGestureEnd;

        detailObjects = new List<GameObject>();
        foreach (Transform t in transform)
        {
            detailObjects.Add(t.gameObject);
            t.gameObject.SetActive(false);
        }
        currentIndex = -1;
        CycleObject(1);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject detailObject in detailObjects)
        {
            detailObject.transform.LookAt(Camera.main.transform);
            detailObject.transform.eulerAngles = new Vector3(0, detailObject.transform.eulerAngles.y, 0);
        }
    }

    private void OnDestroy()
    {
        MLInput.OnControllerButtonDown -= HandleOnButtonDown;
        MLInput.OnControllerTouchpadGestureEnd -= HandleOnTouchpadGestureEnd;
    }
    public void CycleObject(int delta)
    {
        if (currentIndex >= 0) detailObjects[currentIndex].SetActive(false);
        currentIndex = (currentIndex + delta) % detailObjects.Count;
        while (currentIndex < 0) currentIndex += detailObjects.Count;
        detailObjects[currentIndex].SetActive(true);
    }
    private void HandleOnButtonDown(byte controllerId, MLInputControllerButton button)
    {
        if (button == MLInputControllerButton.Bumper && Vector3.Angle(transform.position - Camera.main.transform.position, Camera.main.transform.forward) < 15)
        {
            transform.GetComponentInParent<GazeFocusable>().SetFocusObjectActive(false);
            Destroy(gameObject);
        }
    }
    private void HandleOnTouchpadGestureEnd(byte controllerId, MLInputControllerTouchpadGesture gesture)
    {
        if (Vector3.Angle(transform.position - Camera.main.transform.position, Camera.main.transform.forward) < 15)
        {
            if (gesture.Direction == MLInputControllerTouchpadGestureDirection.Right)
            {
                CycleObject(1);
            }
            else if (gesture.Direction == MLInputControllerTouchpadGestureDirection.Left)
            {
                CycleObject(-1);
            }
        }
    }
}
