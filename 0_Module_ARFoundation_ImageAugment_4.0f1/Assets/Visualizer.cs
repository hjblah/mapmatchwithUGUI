using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine;

public class Visualizer : MonoBehaviour
{
    [HideInInspector] public ARTrackedImage Image;
    [HideInInspector] public Transform Anchor;
    [HideInInspector] public GameObject Dummy;


    private bool check = false;
    // Start is called before the first frame update
    void Start()
    {
        transform.localEulerAngles = new Vector3(0, 180, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Image.trackingState == TrackingState.Tracking)
        {
            transform.parent = Anchor;
            transform.localPosition = Vector3.zero;
            if (check)
            {
                transform.localEulerAngles = new Vector3(0, 180, 0);
                check = false;
            }
        }

        else if (Image.trackingState != TrackingState.Tracking)
        {
            transform.parent = Dummy.transform;
            transform.localPosition = Vector3.zero;
            if (!check)
            {
                transform.localEulerAngles = new Vector3(0, 0, 0);
                check = true;
            }
        }
    }
}

