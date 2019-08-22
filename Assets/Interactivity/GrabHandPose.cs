using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class GrabHandPose : MonoBehaviour
{
    private GameObject handPoseCylinder;
    public enum Handed { Left, Right};
    public Handed hand;

    private List<bool> holding;
    // Start is called before the first frame update
    void Start()
    {
        holding = new List<bool>();
        handPoseCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        handPoseCylinder.GetComponent<Renderer>().enabled = false;
        handPoseCylinder.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        handPoseCylinder.transform.parent = transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (MLHands.IsStarted)
        {
            if (hand == Handed.Right && MLHands.Right.IsVisible && MLHands.Right.Center != Vector3.zero)
            {
                handPoseCylinder.transform.position = MLHands.Right.Center;
                handPoseCylinder.transform.forward = MLHands.Right.Center - MLHands.Right.Wrist.Center.Position;
                holding.Add(true);
            }
            else if (hand == Handed.Left && MLHands.Left.IsVisible && MLHands.Left.Center != Vector3.zero)
            {
                handPoseCylinder.transform.position = MLHands.Left.Center;
                handPoseCylinder.transform.forward = MLHands.Left.Center - MLHands.Left.Wrist.Center.Position;
                holding.Add(true);
            }
            else
            {
                holding.Add(false);
            }
            if (holding.Count > 40) holding.RemoveAt(0);
        }
    }
    public Transform GetTransform()
    {
        return handPoseCylinder.transform;
    }

    public bool isHolding()
    {
        int holdCount = 0;
        foreach (bool hold in holding)
        {
            if (hold) holdCount++;
        }
       
        if (holdCount > 20) return true;
        return false;
    }
}
