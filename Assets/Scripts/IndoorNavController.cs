    using System.Collections.Generic;
    using GoogleARCore;
    using GoogleARCore.Examples.Common;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>
    /// Controls the IndoorLocalisation example.
    /// </summary>
    public class IndoorNavController : MonoBehaviour
    {
        /// <summary>
        /// The first-person camera being used to render the passthrough camera image (i.e. AR
        /// background).
        /// </summary>
        public Camera FirstPersonCamera;

        /// <summary>
        /// The sphere representing the person
        /// </summary>
        public GameObject CameraTarget;

        /// <summary>
        /// Text to show error messages
        /// </summary>
        public Text ErrorText;

        /// <summary>
        /// True if the app is in the process of quitting due to an ARCore connection error,
        /// otherwise false.
        /// </summary>
        private bool IsQuitting = false;

        /// <summary>
        /// Remember previous position in order to calculate rotations and translations
        /// </summary>
        private Vector3 PrevARPosePosition;

        /// <summary>
        /// Saying AR is tracking ot not
        /// </summary>
        private bool Tracking = false;

        public bool start = true;

        /// <summary>
        /// The Unity Start() method.
        /// </summary>
        public void Start()
        {
            //set initial position
            PrevARPosePosition = Vector3.zero;
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            UpdateApplicationLifecycle();

            //move the person indicator according to position
            Vector3 currentARPosition = Frame.Pose.position;
            if (!Tracking)
            {
                Tracking = true;
                PrevARPosePosition = Frame.Pose.position;
            }
            //Remember the previous position so we can apply deltas
            Vector3 deltaPosition = currentARPosition - PrevARPosePosition;
            PrevARPosePosition = currentARPosition;
            if (CameraTarget != null)
            {
            // The initial forward vector of the sphere must be aligned with the initial camera direction in the XZ plane.
            // We apply translation only in the XZ plane.
            CameraTarget.transform.Translate(deltaPosition.x, 0.0f, deltaPosition.z);
            // Set the pose rotation to be used in the CameraFollow script
            FirstPersonCamera.GetComponent<FollowTarget>().targetRot = Frame.Pose.rotation;
            }
        }

        /// <summary>
        /// Check and update the application lifecycle.
        /// </summary>
        private void UpdateApplicationLifecycle()
        {
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                Tracking = false;
                ErrorText.text = "Lost tracking, wait ...";
                const int lostTrackingSleepTimeout = 15;
                Screen.sleepTimeout = lostTrackingSleepTimeout;
                return;
            }
            else
            {
                if(ErrorText.text.Equals("Lost tracking, wait ..."))
                {
                    ErrorText.text = "";
                }
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            if (IsQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to
            // appear.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                ShowAndroidToastMessage("Camera permission is needed to run this application.");
                IsQuitting = true;
                Invoke("DoQuit", 0.5f);
            }
            else if (Session.Status.IsError())
            {
                ShowAndroidToastMessage(
                    "ARCore encountered a problem connecting.  Please start the app again.");
                IsQuitting = true;
                Invoke("DoQuit", 0.5f);
            }
        }

        /// <summary>
        /// Actually quit the application.
        /// </summary>
        private void DoQuit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        private void ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity =
                unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject =
                        toastClass.CallStatic<AndroidJavaObject>(
                            "makeText", unityActivity, message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }