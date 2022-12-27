using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

/// This component listens for images detected by the <c>XRImageTrackingSubsystem</c>
/// and overlays some information as well as the source Texture2D on top of the
/// detected image.
/// </summary>
[RequireComponent(typeof(ARTrackedImageManager))]
public class TrackedImageInfoManager : MonoBehaviour
{
    [HideInInspector] public int currentIndex;

    [SerializeField] private GameObject[] dummy;
    [SerializeField] private Visualizer[] contents;

    [SerializeField] private GameObject scanningEffect;
    [SerializeField] private GameObject markerTitle;

    private ARTrackedImage TrackedImage = null;
    private Visualizer visualizer = null;
    private bool markerFlag = false;
    private float scanningTime = 0;
    Camera m_WorldSpaceCanvasCamera;

    /// <summary>
    /// The prefab has a world space UI canvas,
    /// which requires a camera to function properly.
    /// </summary>
    public Camera worldSpaceCanvasCamera
    {
        get { return m_WorldSpaceCanvasCamera; }
        set { m_WorldSpaceCanvasCamera = value; }
    }

    [SerializeField]
    Texture2D m_DefaultTexture;

    /// <summary>
    /// If an image is detected but no source texture can be found,
    /// this texture is used instead.
    /// </summary>
    public Texture2D defaultTexture
    {
        get { return m_DefaultTexture; }
        set { m_DefaultTexture = value; }
    }

    ARTrackedImageManager m_TrackedImageManager;

    void Awake()
    {
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    public void ResetButton()
    {
        if (visualizer != null)
        {
            GameObject.Destroy(visualizer.gameObject);
        }

        TrackedImage = null;
        scanningEffect.SetActive(false);
        markerTitle.SetActive(true);
        markerFlag = false;
        scanningTime = 0;
    }

    void UpdateInfo(ARTrackedImage trackedImage)
    {
        // Set canvas camera
        var canvas = trackedImage.GetComponentInChildren<Canvas>();
        canvas.worldCamera = worldSpaceCanvasCamera;

        // Update information about the tracked image
        var text = canvas.GetComponentInChildren<Text>();
        text.text = string.Format(
            "{0}\ntrackingState: {1}\nGUID: {2}\nReference size: {3} cm\nDetected size: {4} cm",
            trackedImage.referenceImage.name,
            trackedImage.trackingState,
            trackedImage.referenceImage.guid,
            trackedImage.referenceImage.size * 100f,
            trackedImage.size * 100f);

        var planeParentGo = trackedImage.transform.GetChild(0).gameObject;
        var planeGo = planeParentGo.transform.GetChild(0).gameObject;

        // Disable the visual plane if it is not being tracked
        if (trackedImage.trackingState != TrackingState.None)
        {
            planeGo.SetActive(true);

            // The image extents is only valid when the image is being tracked
            trackedImage.transform.localScale = new Vector3(trackedImage.size.x, 1f, trackedImage.size.y);

            // Set the texture
            var material = planeGo.GetComponentInChildren<MeshRenderer>().material;
            material.mainTexture = (trackedImage.referenceImage.texture == null) ? defaultTexture : trackedImage.referenceImage.texture;
        }
        else
        {
            planeGo.SetActive(false);
        }
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            if (TrackedImage == null)
            {
                if (trackedImage.trackingState == TrackingState.Tracking)
                {
                    TrackedImage = trackedImage;
                    markerFlag = true;
                }
            }
        }
    }

    private void Update()
    {
        if (scanningEffect.activeSelf)
        {
            markerTitle.SetActive(false);
            scanningTime += Time.deltaTime;
        }
        else if (!scanningEffect.activeSelf)
        {
            scanningTime = 0;
        }

        if (markerFlag) {
            scanningEffect.SetActive(true);
            scanningEffect.transform.parent = TrackedImage.transform;
            scanningEffect.transform.localPosition = Vector3.zero;
            scanningEffect.transform.localEulerAngles = new Vector3(0, 0, 0);

            markerTitle.SetActive(false);

            if (scanningTime > 3)
            {
                Debug.Log(TrackedImage.referenceImage.name);

                for (int i = 0; i < dummy.Length; i++)
                {
                    if (TrackedImage.referenceImage.name == dummy[i].name)
                    {
                        currentIndex = i;
                    }
                }

                visualizer = (Visualizer)Instantiate(contents[currentIndex], TrackedImage.transform);

                visualizer.Image = TrackedImage;
                visualizer.Dummy = dummy[currentIndex];
                visualizer.Anchor = TrackedImage.transform;
                scanningEffect.SetActive(false);
                scanningTime = 0;
                markerFlag = false;
            }

            if (TrackedImage.trackingState != TrackingState.Tracking)
            {
                ResetButton();
            }
        }

    }
}
