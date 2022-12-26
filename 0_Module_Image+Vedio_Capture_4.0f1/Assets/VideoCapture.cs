/* 
*   NatCorder
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatCorder.Examples {

    #if UNITY_EDITOR
	using UnityEditor;
	#endif
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;
    using NatSuite.Recorders;
    using NatSuite.Recorders.Clocks;
    using NatSuite.Recorders.Inputs;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using System.IO;

    public class VideoCapture : MonoBehaviour {

        /**
        * ReplayCam Example
        * -----------------
        * This example records the screen using a `CameraRecorder`.
        * When we want mic audio, we play the mic to an AudioSource and record the audio source using an `AudioRecorder`
        * -----------------
        * Note that UI canvases in Overlay mode cannot be recorded, so we use a different mode (this is a Unity issue)
        */

        [SerializeField] CameraManager photoManager;

        [Header("Recording")]
        public int videoWidth = 1280;
        public int videoHeight = 720;

        [Header("Microphone")]
        public bool recordMicrophone;
        public AudioSource microphoneSource;

        private MP4Recorder videoRecorder;
        private IClock recordingClock;
        private CameraInput cameraInput;
        private AudioInput audioInput;

        [HideInInspector] public string savedPath = "";
        [HideInInspector] public byte[] bytes;

        public void StartRecording () {
            // Start recording
            var frameRate = 30;
            var sampleRate = recordMicrophone ? AudioSettings.outputSampleRate : 0;
            var channelCount = recordMicrophone ? (int)AudioSettings.speakerMode : 0;
            recordingClock = new RealtimeClock();
            videoRecorder = new MP4Recorder(videoWidth,videoHeight,frameRate, sampleRate, channelCount);
            // Create recording inputs
            cameraInput = new CameraInput(videoRecorder, recordingClock, Camera.main);

            if (recordMicrophone) {
                StartMicrophone();
                audioInput = new AudioInput(videoRecorder, recordingClock, microphoneSource, true);
            }
        }

        private void StartMicrophone () {
            #if !UNITY_WEBGL || UNITY_EDITOR // No `Microphone` API on WebGL :(
            // Create a microphone clip
            microphoneSource.clip = Microphone.Start(null, true, 60, 48000);
            while (Microphone.GetPosition(null) <= 0) ;
            // Play through audio source
            microphoneSource.timeSamples = Microphone.GetPosition(null);
            microphoneSource.loop = true;
            microphoneSource.Play();
            #endif
        }

        public async void StopRecording () {
            // Stop the recording inputs
            if (recordMicrophone) {
                StopMicrophone();
                audioInput.Dispose();
            }
            cameraInput.Dispose();

            var path = await videoRecorder.FinishWriting();
            bytes = File.ReadAllBytes(path);

            string currentfileName = "Test_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".mp4";
            Handheld.PlayFullScreenMovie($"file://{path}");

            NativeGallery.SaveVideoToGallery(bytes, "Test", "Test_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".mp4");
            StartCoroutine(photoManager.SaveVideoCapture(path.ToString(), currentfileName));

        }

        private void StopMicrophone () {
            #if !UNITY_WEBGL || UNITY_EDITOR
            Microphone.End(null);
            microphoneSource.Stop();
            #endif
        }
    }
}