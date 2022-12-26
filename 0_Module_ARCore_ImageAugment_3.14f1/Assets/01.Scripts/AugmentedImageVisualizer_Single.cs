using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GoogleARCore;
using GoogleARCoreInternal;
using UnityEngine;

public class AugmentedImageVisualizer_Single : MonoBehaviour
{
    [HideInInspector] public AugmentedImage Image;
    [HideInInspector] public Anchor Anchor;
    [HideInInspector] public GameObject Dummy;

    //private string dummyName;

    public void Update()
    {
        if (Image.TrackingMethod == AugmentedImageTrackingMethod.FullTracking)
        {
            transform.parent = Anchor.transform;
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = new Vector3(0, 0, 0);
        }

        else if (Image.TrackingMethod != AugmentedImageTrackingMethod.FullTracking)
        {
            transform.parent = Dummy.transform;
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = new Vector3(0, 0, 0);
        }
    }
}

