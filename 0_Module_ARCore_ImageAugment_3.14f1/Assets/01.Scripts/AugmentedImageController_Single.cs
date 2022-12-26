
using GoogleARCore;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

public class AugmentedImageController_Single : MonoBehaviour
{
    //GameObject
    [SerializeField] private GameObject scanningEffect = null;
    [SerializeField] private GameObject resetButton = null;
    [SerializeField] private GameObject markerTitle = null;
    [SerializeField] private GameObject[] dummy = null;
    [SerializeField] private AugmentedImageVisualizer_Single[] AugmentedImageVisualizerPrefab = null;

    //Variables
    [Tooltip("몇초동안 스캔할것인가")]
    [SerializeField] private float scanTime = 3.0f;
    private List<AugmentedImage> m_TempAugmentedImages = new List<AugmentedImage>();
    private bool markerFlag = false;
    private int currentIndex = 0;
    private float scanningTime = 0;
    private AugmentedImage trackedImage = null;
    private AugmentedImageVisualizer_Single visualizer = null;
    private Anchor anchor = null;

    private void GetContentsIndex(string imageName)
    {
        for (int i = 0; i < dummy.Length; i++)
        {
            if (imageName == dummy[i].name)
            {
                currentIndex = i;
            }
        }
    }

    public void ResetButton()
    {
        if (visualizer != null)
        {
            Destroy(visualizer.Anchor);
            Destroy(visualizer.gameObject);
        }
        Destroy(anchor);

        trackedImage = null;
        resetButton.SetActive(false);
        scanningEffect.SetActive(false);
        markerTitle.SetActive(true);
        markerFlag = false;
        scanningTime = 0;
    }

    public void Update()
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

        // Only allow the screen to sleep when not tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        Session.GetTrackables<AugmentedImage>(m_TempAugmentedImages, TrackableQueryFilter.Updated);

        foreach (var image in m_TempAugmentedImages)
        {
            if (image.TrackingState == TrackingState.Tracking)
            {
                if (image.TrackingMethod == AugmentedImageTrackingMethod.FullTracking && trackedImage == null) // tracking된 마커가 없을때 마커를 fulltracking 하면 anchor를 생성하고 trackedImage에 마커를 넣는다
                {
                    anchor = image.CreateAnchor(image.CenterPose);
                    trackedImage = image;
                    GetContentsIndex(image.Name);
                    markerFlag = true;
                    break;
                }
                else if (image.TrackingMethod != AugmentedImageTrackingMethod.FullTracking && trackedImage == null && scanningEffect.activeSelf) // 첫 마커 인식중(scanning)에 fulltracking을 놓치면
                {
                    ResetButton();
                    break;
                }
            }
        }

        if (markerFlag) // 스캔중 effect 시작
        {
            if(!scanningEffect.activeSelf)
            {
                scanningEffect.SetActive(true);
                scanningEffect.transform.parent = anchor.transform;
                scanningEffect.transform.localPosition = Vector3.zero;
                scanningEffect.transform.localEulerAngles = new Vector3(0, 0, 0);
                markerTitle.SetActive(false);
            }

            if (scanningTime > scanTime) // 스캔 시간이 3초가 되면 prefab으로 instance 생성
            {
                if (trackedImage.DatabaseIndex == currentIndex)
                {
                    visualizer = (AugmentedImageVisualizer_Single)Instantiate(AugmentedImageVisualizerPrefab[currentIndex], anchor.transform);
                }

                visualizer.Image = trackedImage;
                visualizer.Anchor = anchor;
                visualizer.Dummy = dummy[currentIndex]; // 인스턴스로 생성한 visualizer에 trackedImage, anchor, dummy를 넘겨준다

                resetButton.SetActive(true);
                scanningEffect.SetActive(false);
                scanningTime = 0;
                markerFlag = false;
            }

            if (trackedImage.TrackingMethod != AugmentedImageTrackingMethod.FullTracking) //스캔중 effect가 나오는중에 마커를 놓쳤을때
            {
                ResetButton();
            }

            if (trackedImage.TrackingState == TrackingState.Stopped && visualizer != null)
            {
                GameObject.Destroy(visualizer.gameObject);
            } //오류시
        }

        if (visualizer != null) { // 카메라가 가려졌다가 다시 컨텐츠를 붙힐때 생기는 scanningEffect 제거
            scanningEffect.SetActive(false);
        }
    }
}

