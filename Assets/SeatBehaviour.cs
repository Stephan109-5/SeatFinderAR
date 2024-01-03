using System;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;


namespace SeatFinder
{
    public class SeatBehaviour : MonoBehaviour
    {
        public float ClosestAcDist;
        public float ClosestNoiseSrcDist;
        public float ClosestWindowDist;
        public bool OutletsPresent;

        public Vector3 ClosestAcPos;
        public Vector3 ClosestWindowPos;
        public Vector3 ClosestNoiseSrcPos;
        public Vector3 ClosestOutletPos;

        public float closeThreshold = 2f;
        public float mediumThreshold = 4f;

        private Camera _mainCam;

        private GameObject _seatIcons;
        private Transform _seatAvailableUI;
        private Transform _seatReservedUI;
        private GameObject _textContainerUI;
        private Transform _seatTitleUI;
        private GameObject _arrow;

        private PreferencePanelBehaviour _prefPanel;
        private Main _mainScript;

        // firebase stuff
        private DatabaseReference reference;
        private string seatOccupant;
        private float timer;


        private void Start()
        {
            ClosestNoiseSrcDist = float.MaxValue;
            ClosestAcDist = float.MaxValue;
            ClosestWindowDist = float.MaxValue;
            OutletsPresent = false;
            _mainCam = Camera.main;
            _seatIcons = transform.GetChild(0).GetChild(0).GetChild(0).Find("Icons").gameObject;


            _textContainerUI = transform.GetChild(0).GetChild(0).Find("Text").gameObject;

            _seatAvailableUI = _textContainerUI.transform.Find("Available");
            _seatReservedUI = _textContainerUI.transform.Find("Reserved");
            _seatTitleUI = _textContainerUI.transform.Find("Title");
            _seatTitleUI.GetComponent<TextMeshProUGUI>().text = gameObject.name.ToUpper();

            _mainScript = GameObject.Find("AreaTarget").GetComponent<Main>();

            //hide seat 3d model
            Destroy(GetComponent<MeshRenderer>());

            _arrow = transform.GetChild(1).GetChild(0).gameObject;
            _arrow.SetActive(false);
            timer = 0;
            _prefPanel = GameObject.Find("PreferenceCanvas").GetComponent<PreferencePanelBehaviour>();

            // hide seat ui on start
            _seatReservedUI.gameObject.SetActive(false);
            _seatAvailableUI.gameObject.SetActive(false);
            _textContainerUI.gameObject.SetActive(false);
            HideArrow();

            _seatAvailableUI.GetChild(0).GetComponent<Button>().onClick
                .AddListener(() => ReserveSeat(_prefPanel.UserName));
            _seatReservedUI.GetChild(2).GetComponent<Button>().onClick
                .AddListener(() => UnReserveSeat(_prefPanel.UserName));

            updateIcons();
        }

        private void Update()
        {
            Vector3 seat_pos = transform.position;
            Vector3 cam_pos = _mainCam.transform.position;
            float distance = (cam_pos - seat_pos).magnitude;

            // show/hide Seat UI based on distance to user
            if (distance > 5)
            {
                _seatIcons.SetActive(false);
                _textContainerUI.SetActive(false);
            }
            else if (distance > 3)
            {
                _seatIcons.SetActive(true);
                _textContainerUI.SetActive(false);
            }
            else
            {
                _seatIcons.SetActive(true);
                _textContainerUI.SetActive(true);
            }

            // refresh booking status every 2s
            if (timer < 2)
            {
                timer += Time.deltaTime;
            }
            else
            {
                timer = 0;
                GetSeatOccupant(_prefPanel.UserName);
                /*updateIcons();*/
            }
        }

        public void updateIcons()
        {
            Transform icons = transform.GetChild(0).GetChild(0).GetChild(0).Find("Icons");
            Transform light = icons.Find("light").GetChild(1);
            Transform noise = icons.Find("noise").GetChild(1);
            Transform thermal = icons.Find("thermal").GetChild(1);
            Transform outlet = icons.Find("outlet").GetChild(1).GetChild(0);

            Image[] light_bar = light.GetComponentsInChildren<Image>();
            Image[] noise_bar = noise.GetComponentsInChildren<Image>();
            Image[] thermal_bar = thermal.GetComponentsInChildren<Image>();
            Image outletIcon = outlet.GetComponent<Image>();

            Array[] loading_bar = {light_bar, noise_bar, thermal_bar};

            float[] distances = { ClosestNoiseSrcDist, ClosestAcDist, ClosestWindowDist };

            Color32[] color_l_n_t = { new Color32(253, 255, 92, 255), new Color32(92, 255, 248, 255), new Color32(92, 255, 166, 255) };
            Color32 blank_color = new Color32(217, 217, 217, 255);

            int[] light_noise_thermal = { 0, 0, 0 };

            for (int i = 0; i < 3; i++)
            {
                if (distances[i] < closeThreshold)
                {
                    light_noise_thermal[i] = 1;
                }
                else if (distances[i] < mediumThreshold)
                {
                    light_noise_thermal[i] = 2;
                }
                else
                {
                    light_noise_thermal[i] = 3;
                }

                /*Debug.Log("Number " + i + ", val: " + light_noise_thermal[i])*/;
                
                foreach (Image img in loading_bar[i])
                {
                    if (light_noise_thermal[i] > 0)
                    {
                        img.color = color_l_n_t[i];
                        light_noise_thermal[i]--;
                    }
                    else { img.color = blank_color; }
                }

            }

            /*Debug.Log("outlet " + OutletsPresent);*/
            if (OutletsPresent)
            {
                outletIcon.sprite = Resources.Load<Sprite>("Checked_Checkbox");
            }
            else
            {
                outletIcon.sprite = null;
            }
        }

        public void showSuggestionArrow()
        {
            _arrow.GetComponent<MeshRenderer>().material = Resources.Load<Material>("suggestionMaterial");
            _arrow.SetActive(true);
            // Animation arrowAnimation = _arrow.AddComponent<Animation>();
            // arrowAnimation.AddClip(Resources.Load<AnimationClip>("ArrowBounce"), "ArrowBounce");
            // arrowAnimation.Play();
        }

        private void showReservedArrow()
        {
            _arrow.GetComponent<MeshRenderer>().material = Resources.Load<Material>("reservedMaterial");
            _arrow.SetActive(true);
        }

        public void HideArrow()
        {
            _arrow.SetActive(false);
        }

        private void GetSeatOccupant(string userName)
        {
            this.reference = FirebaseDatabase.DefaultInstance.GetReference("seats/" + gameObject.name);

            this.reference
                .GetValueAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        // Handle the error...
                        Debug.Log("Fault");
                    }
                    else if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;
                        seatOccupant = (string)snapshot.Child("Occupant").GetValue(true);
                        // available
                        if (seatOccupant == "")
                        {

                            _seatReservedUI.gameObject.SetActive(false);
                            _seatAvailableUI.gameObject.SetActive(true);
                        }
                        // reserved
                        else
                        {
                            _seatReservedUI.gameObject.SetActive(true);
                            _seatAvailableUI.gameObject.SetActive(false);

                            _seatReservedUI.GetChild(1).GetComponent<TextMeshProUGUI>().text = seatOccupant;

                            if (seatOccupant == userName)
                            {
                                // show unreserve button
                                _seatReservedUI.GetChild(2).gameObject.SetActive(true);
                            }
                            else
                            {
                                _seatReservedUI.GetChild(2).gameObject.SetActive(false);
                            }
                        }
                    }
                });
        }

        public void ReserveSeat(string userName)
        {
            reference = FirebaseDatabase.DefaultInstance.GetReference("seats/" + gameObject.name);
            /*Debug.Log(name.username);*/

            if (userName != "")
            {
                reference.Child("Occupant").SetValueAsync(userName);
                GetSeatOccupant(userName);
                _mainScript.hideBestSeats();
                showReservedArrow();
            }
        }

        public void UnReserveSeat(string userName)
        {
            reference = FirebaseDatabase.DefaultInstance.GetReference("seats/" + gameObject.name);
            /*Debug.Log(name.username);*/

            if (userName != "")
            {
                reference.Child("Occupant").SetValueAsync("");
                GetSeatOccupant(userName);
                HideArrow();
            }
        }
    }
}