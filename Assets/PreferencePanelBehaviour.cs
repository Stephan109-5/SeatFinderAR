using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SeatFinder
{
    public class PreferencePanelBehaviour : MonoBehaviour
    {
        private Animation menuTransform;
        private bool showMenu;

        private Slider _temperatureSlider;
        private Slider _noiseSlider;
        private Slider _windowSlider;
        private Toggle _outletToggle;
        private TMP_InputField _userNameInput;
        private Button _findSeatButton; 
        private Button GoBackButton;
        public string UserName;
        /*public TMP_Text SliderBtnText;*/
        private Transform SliderBtn;

        private Main _mainScript;

        private void Start()
        {
            _temperatureSlider = transform.GetChild(0).Find("TempSlider").GetComponent<Slider>();
            _noiseSlider = transform.GetChild(0).Find("NoiseSlider").GetComponent<Slider>();
            _windowSlider = transform.GetChild(0).Find("WindowSlider").GetComponent<Slider>();
            _outletToggle = transform.GetChild(0).Find("OutletToggle").GetComponent<Toggle>();
            _userNameInput = transform.GetChild(0).Find("UserNameInput").GetComponent<TMP_InputField>();
            _findSeatButton = transform.GetChild(0).Find("FindSeatButton").GetComponent<Button>();

            _mainScript = GameObject.Find("AreaTarget").GetComponent<Main>();

            _findSeatButton.onClick.AddListener(FindSeat);

            showMenu = true;
            menuTransform = transform.GetChild(0).GetComponent<Animation>();
            GoBackButton = transform.GetChild(0).Find("GoBackBtn").GetComponent<Button>();
            GoBackButton.onClick.AddListener(GoBackScene);
            SliderBtn = transform.GetChild(0).Find("SliderBtn");

        }

        public void FindSeat()
        {
            int tempVal = (int)_temperatureSlider.value;
            int noiseVal = (int)_noiseSlider.value;
            int windowVal = (int)_windowSlider.value;
            bool outletVal = _outletToggle.isOn; 
            UserName = _userNameInput.text;
            
            if (UserName == "")
            {
                Debug.Log("No Username entered");
                return;
            }
            
            UserPreference userPreferences = new UserPreference(tempVal, noiseVal, windowVal, outletVal);
            _mainScript.showBestSeats(userPreferences);
            /*gameObject.SetActive(false);*/
            HidePanel();
        }

        public void HidePanel()
        {
            if (showMenu)
            {
                menuTransform.Play("SlideDown");
                SliderBtn.Rotate(0f, 0f, 180f);
                showMenu = false;
                /*SliderBtnText.text = "^";*/
            }
            else
            {
                menuTransform.Play("SlideUp");
                SliderBtn.Rotate(0f, 0f, 180f);
                showMenu = true;
                /*SliderBtnText.text = "v";*/
            }
            
        }

       void GoBackScene()
        {
            SceneManager.LoadScene("StartScene");
        }
    }
}