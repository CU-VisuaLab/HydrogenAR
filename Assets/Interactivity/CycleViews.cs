using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class CycleViews : MonoBehaviour
{
    private ControllerConnectionHandler _controllerConnectionHandler;
    private bool bumperDown;
    // Start is called before the first frame update
    void Start()
    {
        _controllerConnectionHandler = GetComponent<ControllerConnectionHandler>();
        MLInput.OnControllerButtonUp += HandleOnButtonUp;
        MLInput.OnControllerButtonDown += HandleOnButtonDown;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void HandleOnButtonDown(byte controllerId, MLInputControllerButton button)
    {
        MLInputController controller = _controllerConnectionHandler.ConnectedController;
        if (controller != null && controller.Id == controllerId &&
            button == MLInputControllerButton.Bumper)
        {
            bumperDown = true;
            foreach (ViewManagement viewManagement in FindObjectsOfType<ViewManagement>())
            {
                viewManagement.nextView();
            }
        }
    }
    private void HandleOnButtonUp(byte controllerId, MLInputControllerButton button)
    {
        MLInputController controller = _controllerConnectionHandler.ConnectedController;
        if (controller != null && controller.Id == controllerId &&
            button == MLInputControllerButton.Bumper)
        {
            bumperDown = false;
        }
    }
}
