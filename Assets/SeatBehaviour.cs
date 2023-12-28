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
            Image[] light_bar = light.GetComponentsInChildren<Image>();
            Image[] noise_bar = noise.GetComponentsInChildren<Image>();
            Image[] thermal_bar = thermal.GetComponentsInChildren<Image>();

            float[] distances = { ClosestNoiseSrcDist, ClosestAcDist, ClosestWindowDist };

            
            int light_level = 0;
            if (distances[2] < closeThreshold)
            {
                light_level = 1;
            }
            else if (distances[2] < mediumThreshold)
            {
                light_level = 2;
            }
            else
            {
                light_level = 3;
            }

            int noise_level = 0;
            if (distances[0] < closeThreshold)
            {
                noise_level = 1;
            }
            else if (distances[0] < mediumThreshold)
            {
                noise_level = 2;
            }
            else
            {
                noise_level = 3;
            }

            int thermal_level = 0;
            if (distances[1] < closeThreshold)
            {
                thermal_level = 1;
            }
            else if (distances[1] < mediumThreshold)
            {
                thermal_level = 2;
            }
            else
            {
                thermal_level = 3;
            }



            for (int i = 0; i < distances.Length; i++)
            {
                if (light_level > 0)
                {
                    light_bar[i].color = new Color32(92, 255, 166, 255);
                    light_level--;
                }
                else
                {
                    light_bar[i].color = new Color32(217, 217, 217, 255);
                }


                if (noise_level > 0)
                {
                    noise_bar[i].color = new Color32(92, 255, 166, 255);
                    noise_level--;
                }
                else
                {
                    noise_bar[i].color = new Color32(217, 217, 217, 255);
                }


                if (thermal_level > 0)
                {
                    thermal_bar[i].color = new Color32(92, 255, 166, 255);
                    thermal_level--;
                }
                else
                {
                    thermal_bar[i].color = new Color32(217, 217, 217, 255);
                }


            }


            /*Debug.Log("outlet " + OutletsPresent);*/
            /*            if (OutletsPresent)
                        {
                            outletIcon.sprite = Resources.Load<Sprite>("Icons/o0");
                        }
                        else
                        {
                            outletIcon.sprite = Resources.Load<Sprite>("Icons/o1");
                        }*/
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