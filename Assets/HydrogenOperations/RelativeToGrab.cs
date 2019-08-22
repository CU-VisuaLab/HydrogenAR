using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.XR.MagicLeap;
using UnityEngine.UI;

public class RelativeToGrab : MonoBehaviour
{
    public float offsetForward;
    public float offsetRight;
    public float offsetUp;

    public int movingAverageLength = 50;
    private List<Vector3> positions;

    public GrabHandPose grab;
    public enum Handed { Left, Right};
    public Handed hand;

    private ControllerConnectionHandler _controllerConnectionHandler;
    private MLInputController controller;
    private Vector3 controllerStats;
    // Start is called before the first frame update
    void Start()
    {
        positions = new List<Vector3>();
        _controllerConnectionHandler = FindObjectOfType<ControllerConnectionHandler>();
        controller = _controllerConnectionHandler.ConnectedController;
        controllerStats = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (grab == null) setGrab();
        Transform baseTransform = grab.GetTransform();
        Vector3 positionMeasure = baseTransform.position + offsetForward * baseTransform.forward + offsetUp * baseTransform.up + offsetRight * baseTransform.right;
        positions.Add(positionMeasure);
        if (positions.Count > movingAverageLength) positions.RemoveAt(0);
        transform.position = average(positions);
        /*if (hand == Handed.Right)
        {
            if (controller.Touch1Active)
            {
                if (controllerStats == Vector3.zero)
                {
                    controllerStats = controller.Touch1PosAndForce;
                    return;
                }
                else
                {
                    offsetForward += (controller.Touch1PosAndForce.x - controllerStats.x);
                    offsetUp += (controller.Touch1PosAndForce.y - controllerStats.y);
                    controllerStats = controller.Touch1PosAndForce;
                }
            }
            GameObject.Find("DebugText").GetComponent<Text>().text = offsetForward + ", " + offsetRight + ", " + offsetUp;
        }*/
    }

    private Vector3 average(List<Vector3> vectors)
    {
        Vector3 sum = Vector3.zero;
        if (vectors.Count == 0) return sum;
        foreach (Vector3 vector in vectors)
        {
            sum += vector;
        }
        return sum / vectors.Count;
    }
    public void setGrab()
    {
        GrabHandPose[] hands = FindObjectsOfType<GrabHandPose>();
        foreach (GrabHandPose handGrab in hands)
        {
            if (handGrab.hand.ToString() == hand.ToString())
            {
                grab = handGrab;
            }
        }
    }
}
