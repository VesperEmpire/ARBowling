namespace GoogleARCore.Examples.BowlingAR
{
    using System.Collections.Generic;
    using GoogleARCore;
    using GoogleARCore.Examples.Common;
    using UnityEngine;
    using UnityEngine.UI;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = InstantPreviewInput;
#endif

    public class BowlingController : MonoBehaviour
    {
        /// The first-person camera being used to render the passthrough camera image (i.e. AR background).
        public Camera FirstPersonCamera;

        /// A prefab for tracking and visualizing detected planes.
        public GameObject DetectedPlanePrefab;
        public GameObject PinPrefab;
        public GameObject BowlPrefab;
        public GameObject StartInfotext;
        public GameObject SearchingForPlaneUI;
        public GameObject StartGameButton;
        private GameObject[] PinInstance = new GameObject[10];
        public Text RealtimeSizeOfPlane;

        public GameObject TestCube;
        //g = GameObject.Find("BowlingBall"); To save the result of finding
        private GameObject g;

        private Anchor[] anchor = new Anchor[10];
        /// A list to hold all planes ARCore is tracking in the current frame. This object is used across
        /// the application to avoid per-frame allocations.
        private List<DetectedPlane> m_AllPlanes = new List<DetectedPlane>();

        // True if the app is in the process of quitting due to an ARCore connection error, otherwise false.
        private bool m_IsQuitting = false;
        //judge whether the game starts
        private bool isStart = false;
        //Whether there is a ping in the game
        private bool isPin = false;
        //Whether UI objs have been hidden or have appeared
        private bool isHidOrApear = false;
        
        public void Start()
        {
            //At the beginning, some game information and status bars should be hidden or appear
            SearchingForPlaneUI.SetActive(true);
            StartInfotext.SetActive(false);
            StartGameButton.SetActive(false);
            g = GameObject.Find("BowlingBall");
            g.SetActive(false);
            RealtimeSizeOfPlane.text = "Plane Size: (x: 0, z: 0)\nPlane is being detected";
        }

        public void Update()
        {
            _UpdateApplicationLifecycle();

            //Detect plane, if you find a plane, hide the search bar and display information that can start the game.
            Session.GetTrackables<DetectedPlane>(m_AllPlanes);
            for (int i = 0; i < m_AllPlanes.Count; i++)
            {
                if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
                {
                    //print("x: " + m_AllPlanes[i].ExtentX + " z: " + m_AllPlanes[i].ExtentZ);
                    RealtimeSizeOfPlane.text = "Plane Size: (x:" 
                                             + m_AllPlanes[i].ExtentX 
                                             + ",  z:" 
                                             + m_AllPlanes[i].ExtentZ 
                                             + ")\n"
                                             + "You need at least x = 5, z = 5 to start the game";
                    //Only when the plane reaches a certain size can the game be played.
                    if (m_AllPlanes[i].ExtentX >= 2.0f && m_AllPlanes[i].ExtentZ >= 2.0f) {
                        //If there is no ping, instantiate it
                        if (!isPin) {
                            Vector3 centerPos = m_AllPlanes[i].CenterPose.position;
                            TestCube.GetComponent<Transform>().position = m_AllPlanes[i].CenterPose.position;
                            print(TestCube.GetComponent<Transform>().position);
                            float z = centerPos.z + 0.25f;
                            float z2 = z + 0.25f;
                            Pose centerP = m_AllPlanes[i].CenterPose;
                            //Instantiate 10 bowling pins to form a triangle array
                            for (int j = 0; j < 10; j++)
                            {
                                if (j < 4)
                                {
                                    centerPos.x += 0.25f;
                                    PinInstance[j] = Instantiate(PinPrefab,
                                                                 centerPos,
                                                                 m_AllPlanes[i].CenterPose.rotation);
                                }
                                else if (j >= 4 && j < 7)
                                {
                                    centerPos = m_AllPlanes[i].CenterPose.position;
                                    centerPos.x +=(0.3f * (j-3));
                                    centerPos.z = z;
                                    PinInstance[j] = Instantiate(PinPrefab,
                                                                 centerPos,
                                                                 m_AllPlanes[i].CenterPose.rotation);
                                }
                                else if (j >= 7 && j < 9)
                                {
                                    centerPos = m_AllPlanes[i].CenterPose.position;
                                    centerPos.x += (0.35f * (j - 6));
                                    centerPos.z = z2;
                                    PinInstance[j] = Instantiate(PinPrefab,
                                                                 centerPos,
                                                                 m_AllPlanes[i].CenterPose.rotation);
                                }
                                else
                                {
                                    centerPos = m_AllPlanes[i].CenterPose.position;
                                    centerPos.x += 0.45f;
                                    centerPos.z = z2 + 0.25f;
                                    PinInstance[j] = Instantiate(PinPrefab,
                                                                 centerPos,
                                                                 m_AllPlanes[i].CenterPose.rotation);
                                }
                                //Instantiate one ping at the center of the suitable plane

                                //Fix the bowling pin by setting an anchor on it
                                centerP.position = centerPos;
                                anchor[j] = m_AllPlanes[i].CreateAnchor(centerP);
                                PinInstance[j].transform.parent = anchor[j].transform;
                            }
                            isPin = true;
                        }
                        if (!isHidOrApear) {
                            SearchingForPlaneUI.SetActive(false);
                            StartInfotext.SetActive(true);
                            StartGameButton.SetActive(true);
                            isHidOrApear = true;
                            break;
                        }
                    }
                }
            }


            /* If the appropriate plane is detected, 
            you can wait for the user to press the start button to start the game.*/
            //isStart == true when ButtonClicked() is invoked
            if (isHidOrApear && isStart)
            {
                Touch touch;
                if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
                {
                    return;
                }

                /*When the user is holding the bowling ball, 
                the bowling ball should not have gravity, otherwise it will fall.*/
                BowlPrefab.GetComponent<Rigidbody>().useGravity = false;
                    

                // Raycast against the location the player touched to search for planes.
                //TrackableHit hit;
                //TrackableHitFlags raycastFilter = TrackableHitFlags.FeaturePointWithSurfaceNormal | TrackableHitFlags.PlaneWithinInfinity;
                ////print(touch.position.x + " ~~" + touch.position.y);
                //if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
                //{
                //    //print("Shoot the plane successfully!！");
                //    // Use hit pose and camera pose to check if hittest is from the
                //    // back of the plane, if it is, no need to create the anchor.
                //    if ((hit.Trackable is DetectedPlane) &&
                //        Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
                //            hit.Pose.rotation * Vector3.up) < 0)
                //    {
                //        Debug.Log("Hit at back of the current DetectedPlane");
                //    }
                //    else
                //    {
                //        // Instantiate model at the hit pose.
                //        //var bowlObject = Instantiate(BowlPrefab, hit.Pose.position, hit.Pose.rotation);

                //        // Compensate for the hitPose rotation facing away from the raycast (i.e. camera).
                //        //bowlObject.transform.Rotate(0, k_ModelRotation, 0, Space.Self);

                //        // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                //        // world evolves.
                //        //var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                //        // Make model a child of the anchor.
                //        //bowlObject.transform.parent = anchor.transform;
                //    }
                //}
            }
        }

        //点击开始游戏，信息栏和按钮隐藏，游戏开始，允许点击屏幕生成保龄球
        public void ButtonClicked() {
            isStart = true;
            StartInfotext.SetActive(false);
            StartGameButton.SetActive(false);
            g.SetActive(true);
        }


        // Check and update the application lifecycle.
        private void _UpdateApplicationLifecycle()
        {
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                const int lostTrackingSleepTimeout = 15;
                Screen.sleepTimeout = lostTrackingSleepTimeout;
            }
            else
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            if (m_IsQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
            else if (Session.Status.IsError())
            {
                _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
        }

        // Actually quit the application.
        private void _DoQuit()
        {
            Application.Quit();
        }

        /// Show an Android toast message.
        private void _ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                        message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }

}
