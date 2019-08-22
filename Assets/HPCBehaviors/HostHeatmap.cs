using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.UI;
using System.Threading.Tasks;

public class HostHeatmap : MonoBehaviour
{

    //private float fastforwardRatio = 1;
    //[Tooltip("How large of a window to aggregate over")]
    //public float timestepDelta = 1;

    public float hostWidth = 0.02f;
    public float hostHeight = 0.08f;
    public float totalHeight;
    public float totalWidth;
    public TextAsset credentials;
    private int timestampIndex;
    private int incrementor;

    // Start is called before the first frame update
    void Awake()
    {
        timestampIndex = -1;
        incrementor = 1;
        //InvokeRepeating("CycleTimestamp", 0,timestepDelta / fastforwardRatio);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CycleTimestamp()
    {
#if UNITY_EDITOR
        UpdateValues();
        //UpdateWIM();
#else
        Task.Factory.StartNew(UpdateValues);
        //Task.Factory.StartNew(UpdateWIM);
#endif
    }

    void UpdateValues()
    {
        if (incrementor == 0) return;
        timestampIndex+=incrementor;
        HostMetrics metric = GetComponentInParent<HostMetrics>();
        metric.UpdateNodes(incrementor);
        List<GameObject> hostGrid = metric.GetHostGrid();
        string[] timestampComponents = metric.GetTimestamp(timestampIndex).Split('T');
        GameObject.Find("TimestampText").GetComponent<Text>().text = timestampComponents[0] + "\n" + timestampComponents[1].Split('-')[0];
        foreach (GameObject host in hostGrid)
        {
            float value = metric.GetIndividualValue(host.transform.name, timestampIndex);
            metric.UpdateHostColor(host, value);
            if (host.GetComponent<DetailHover>() != null)
            {
                if (metric.GetMetricType() == "Usage")
                    host.GetComponent<DetailHover>().updateDetail((Math.Round(value, 4) * 100).ToString() + "%");
                else host.GetComponent<DetailHover>().updateDetail(value.ToString());
            }
        }
    }
    public void UpdatePlaybackSpeed(float frequency)
    {
        CancelInvoke();
        incrementor = (int)Mathf.Sign(frequency);
        InvokeRepeating("CycleTimestamp", 0, Mathf.Abs(frequency));
    }

    public void updatePlaybackIncrementor(int incr)
    {
        incrementor = incr;
    }

    /*private void UpdateWIM()
    {
        GetComponentInParent<HostMetrics>().WIMPiece.GetComponentInParent
        WIMFocusObject[] focusObjects = FindObjectsOfType<WIMFocusObject>();
        foreach(WIMFocusObject focus in focusObjects)
        {
            focus.UpdateTimestepIndex(timestampIndex);
        }
    }*/
}
