using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using NatSuite.Recorders;
using NatCorder.Examples;
using UnityEngine.Video;

public class CameraManager : MonoBehaviour
{
#if UNITY_ANDROID
    private static string ANDROID_PATH;
#elif UNITY_IOS
	[System.Runtime.InteropServices.DllImport( "__Internal" )]
	private static extern void _ScreenshotWriteToAlbum(string path);

	[System.Runtime.InteropServices.DllImport( "__Internal" )]
	private static extern void _VideoWriteToAlbum(string path);
#endif

    [SerializeField] private VideoCapture videoCapture;
    public GameObject videoCaptureSign;
    public GameObject videoStopBtn;
    public Text timerText;

    private float videoRecTimeCount;
    private bool isVideoCapturing;

    public GameObject canvas;
    public RawImage captureImg;

    private string savedPath;
    [HideInInspector] public string currentFileName = "";
    private Texture2D tex = null;

    private bool isPictureCapturing;
    private bool Pictures = true;
    private bool Videos = true;

    private void Awake()
    {
#if UNITY_ANDROID
        ANDROID_PATH = Path.Combine(Application.persistentDataPath, "NGallery");
        DirectoryInfo directName = new DirectoryInfo(ANDROID_PATH);
        if (directName.Exists == false)
            directName.Create();
#endif        
    }
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f;
    }


    // Update is called once per frame
    void Update()
    {
        if (UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageWrite))
        {
            DirectoryInfo directName = new DirectoryInfo(ANDROID_PATH);
            if (directName.Exists == false)
            {
                directName.Create();
            }
        }

        if (isVideoCapturing)
        {
            VideoRecordingTimeCount(); //Record Count Start
        }
    }
#if UNITY_ANDROID
    private void OnApplicationFocus(bool Focused)
    {

        if (Focused)
        {
            if (Pictures)
            {
                if (UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.WRITE_EXTERNAL_STORAGE"))
                {
                    if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.WRITE_EXTERNAL_STORAGE"))
                    {
                        ShowAndroidToastMessage("저장소 권한 허용 완료");
                        Pictures = false;
                    }
                }

                else if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.WRITE_EXTERNAL_STORAGE"))
                {
                    ShowAndroidToastMessage("저장소 권한 허용이 필요합니다");
                }
            }

            else if (Videos)
            {
                if (UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.RECORD_AUDIO") &&
                    UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.WRITE_EXTERNAL_STORAGE"))
                {
                    if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.RECORD_AUDIO") || UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.WRITE_EXTERNAL_STORAGE"))
                    {
                        ShowAndroidToastMessage("저장소, 마이크 권한 허용 완료");
                        Videos = false;
                    }
                }
                else if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.RECORD_AUDIO") &&
                    UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.WRITE_EXTERNAL_STORAGE"))
                {
                    ShowAndroidToastMessage("마이크 권한 허용이 필요합니다");
                }
                else if (UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.RECORD_AUDIO") &&
                    !UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.WRITE_EXTERNAL_STORAGE"))
                {
                    ShowAndroidToastMessage("저장소 권한 허용이 필요합니다");
                }
                else if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.RECORD_AUDIO") &&
                    !UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.WRITE_EXTERNAL_STORAGE"))
                {
                    ShowAndroidToastMessage("저장소, 마이크 권한 허용이 필요합니다");
                }
            }
        }
    }
#endif

    public void OnClickSNS()
    {
#if UNITY_ANDROID
        new NativeShare().AddFile(Path.Combine(ANDROID_PATH, currentFileName)).Share();
#elif UNITY_IOS
        new NativeShare().AddFile(filePath).Share();
#endif
    }

    public void OnClickVideoCaptureStart()
    {
        Videos = true;
#if UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone) ||
            !UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageWrite))
        {
            UnityEngine.XR.ARCore.ARCorePermissionManager.RequestPermission("android.permission.WRITE_EXTERNAL_STORAGE", (text, check) => { });
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
            OnApplicationFocus(Application.isFocused);
        }
        else
        {
            canvas.SetActive(false);
            videoCaptureSign.SetActive(true);
            videoStopBtn.SetActive(true);
            videoCapture.StartRecording();

            videoRecTimeCount = 0;
            isVideoCapturing = true;
            timerText.gameObject.SetActive(true);
        }
#elif UNITY_IOS
        {
            canvas.SetActive(false);
            videoCaptureSign.SetActive(true);
            videoStopBtn.SetActive(true);
            videoCapture.StartRecording();

            videoRecTimeCount = 0;
            isVideoCapturing = true;
            timerText.gameObject.SetActive(true);
        }
#endif
    }

    public void OnClickVideoCaptureStop()
    {
        videoCapture.StopRecording();
        isVideoCapturing = false;

        timerText.gameObject.SetActive(false);
        videoCaptureSign.SetActive(false);
        videoStopBtn.SetActive(false);
    }

    public IEnumerator SaveVideoCapture(string path, string currentFile)
    {
        if (File.Exists(path))
        {
            currentFileName = currentFile;
            Rect captureRect = Camera.main.pixelRect;

            tex = new Texture2D((int)captureRect.width, (int)captureRect.height, TextureFormat.RGB24, true);
            yield return new WaitForEndOfFrame();
            tex.ReadPixels(captureRect, 0, 0, true);
            tex.Apply();

            byte[] fileData = NativeGallery.GetTextureBytes(tex, true);
            tex = new Texture2D((int)captureRect.width, (int)captureRect.height);
            tex.LoadImage(fileData);
            captureImg.texture = tex;

            captureImg.gameObject.SetActive(true);

#if UNITY_IOS
            _VideoWriteToAlbum (newPath);
#endif
            OnSaveSignImg(1);
        }
        else
        {
            Debug.Log("Not Found Mp4 File");
        }
    }

    private void VideoRecordingTimeCount()
    {
        videoRecTimeCount += Time.deltaTime;
        int m = (int)videoRecTimeCount / 60;
        int s = (int)videoRecTimeCount % 60;
        timerText.text = m.ToString("00") + ":" + s.ToString("00");
    }

    public void OnClickPictureCatpure()
    {
        Pictures = true;
#if UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageWrite))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageWrite);
            OnApplicationFocus(Application.isFocused);
        }
        else
        {
            if (!isPictureCapturing)
            {
                isPictureCapturing = true;

                Debug.Log("디폴트 UI 지우기");
                canvas.SetActive(false);
                StartCoroutine(PictureCapture());
            }
        }
#elif UNITY_IOS
         if (!isPictureCapturing)
            {
                isPictureCapturing = true;

                if (saveSignImg.activeSelf)
                {
                    saveSignImg.SetActive(false);
                }
                Debug.Log("디폴트 UI 지우기");
                canvas.SetActive(false);
                StartCoroutine(PictureCapture());
            }
#endif
    }

    private IEnumerator PictureCapture()
    {
        currentFileName = "Suseongmot_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpeg";
#if UNITY_ANDROID
        savedPath = ANDROID_PATH + currentFileName;
#endif

        Rect captureRect = Camera.main.pixelRect;

        tex = new Texture2D((int)captureRect.width, (int)captureRect.height, TextureFormat.RGB24, true);
        yield return new WaitForEndOfFrame();
        tex.ReadPixels(captureRect, 0, 0, true);
        tex.Apply();

        NativeGallery.SaveImageToGallery(tex, "SusungMotAR", "Suseongmot_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpeg");
        OnSaveSignImg(0);

        byte[] fileData = NativeGallery.GetTextureBytes(tex, true);
        tex = new Texture2D((int)captureRect.width, (int)captureRect.height);
        tex.LoadImage(fileData);
        captureImg.texture = tex; //..this will auto-resize the texture dimensions.
        captureImg.gameObject.SetActive(true);
        //canvas.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForEndOfFrame();
#if UNITY_IOS
        filePath = Path.Combine(Application.persistentDataPath, currentFileName);
        imageByte = tex.EncodeToPNG();
        File.WriteAllBytes(filePath, imageByte);
#endif
        isPictureCapturing = false;

    }

    public void OnClickCaptureBackButton()
    {
        canvas.SetActive(true);
        captureImg.gameObject.SetActive(false);
        Directory.Delete(Application.persistentDataPath, true);
    }

    private void OnSaveSignImg(int type)
    {
        // 0: photo 1: video
        if (type == 0) {
            ShowAndroidToastMessage("사진이 저장되었습니다.");
        }
        else {
            ShowAndroidToastMessage("동영상이 저장되었습니다.");
        }
    }

    private void ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }
}
