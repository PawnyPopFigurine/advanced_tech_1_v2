using JZK.Input;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace JZK.UI
{
    public class ControlSettingsUIController : UIController
    {
        ESpeechInputType _recordForType;
        public ESpeechInputType RecordForType => _recordForType;

        bool _popupOpen;
        string _latestRecordedTerm;
        public string LatestRecordedTerm => _latestRecordedTerm;

        [SerializeField] GameObject _recordingPopup;
        [SerializeField] GameObject _popupHeader;
        [SerializeField] GameObject _recordedTerm;
        [SerializeField] TMP_Text _recordedTermText;
        [SerializeField] TMP_Text _settingInputText;

        [SerializeField] List<ScrollItemTermText> _scrollItemTermTexts;


        public override void SetActive(bool active)
        {
            base.SetActive(active);
            if(active)
            {
                RefreshOnActive();
            }
        }

        void RefreshOnActive()
        {
            _recordForType = ESpeechInputType.None;

            TogglePopup(false);
            RefreshDisplayedTerms();
        }

        void RefreshDisplayedTerms()
        {
            foreach(ScrollItemTermText item in _scrollItemTermTexts)
            {
                string currentTermForType = SpeechInputSystem.Instance.GetTermForType(item.InputType);
                TMP_Text itemText = item.gameObject.GetComponent<TMP_Text>();
                itemText.text = currentTermForType;
            }
        }

        public override void UpdateController()
        {
            base.UpdateController();

            UpdateInput();
        }

        public void UpdateInput()
        {
            if(SpeechInputSystem.Instance.UIBackPressed)
            {
                Input_BackButtonPressed();
                return;
            }

            if(_popupOpen)
            {
                if(SpeechInputSystem.Instance.UIConfirmPressed)
                {
                    Input_ConfirmButtonPressed();
                }
                if(SpeechInputSystem.Instance.UIBackPressed)
                {
                    Input_CancelButtonPressed();
                }
            }
        }

        void TogglePopup(bool active)
        {
            _recordingPopup.SetActive(active);
            _popupOpen = active;

            if(active)
            {
                RefreshPopupOnOpen();
                string inputName = SpeechHelper.INPUT_NAMES[_recordForType];
                _settingInputText.text = inputName;
                SpeechRecognitionSystem.Instance.IsSettingTerm = true;
            }
            else
            {
                SpeechRecognitionSystem.Instance.IsSettingTerm = false;
            }
        }

        void RefreshPopupOnOpen()
        {
            _popupHeader.SetActive(true);
            _recordedTerm.SetActive(false);
            _latestRecordedTerm = string.Empty;
            _recordedTermText.text = string.Empty;
        }

        public void Input_BackButtonPressed()
        {
            ControlSettingsUISystem.Instance.Input_BackButtonPressed();
        }

        public void Input_RecordButtonPressed(ESpeechInputType type)
        {
            _recordForType = type;
            TogglePopup(true);
        }

        public void Input_ConfirmButtonPressed()
        {
            if(_latestRecordedTerm == string.Empty)
            {
                Debug.Log(this.name + " - tried to confirm an empty recording. aborting action");
                return;
            }
            SpeechInputSystem.Instance.SetTermForType(_recordForType, _latestRecordedTerm);
            SpeechInputSystem.Instance.SaveCurrentTerms();
            RefreshDisplayedTerms();
            TogglePopup(false);
        }

        public void Input_CancelButtonPressed()
        {
            TogglePopup(false);
        }

        public void Input_ResetButtonPressed()
        {
            SpeechInputSystem.Instance.ResetTermsToDefault();
            SpeechInputSystem.Instance.SaveCurrentTerms();
            RefreshDisplayedTerms();
        }


        public void OnSpeechRecognised(string speech)
        {
            if(!_isActive)
            {
                return;
            }

            if(!_popupOpen)
            {
                return;
            }

            string processedSpeech = SpeechHelper.ProcessSpeechTerm(speech);

            _popupHeader.SetActive(false);
            _recordedTerm.SetActive(true);
            _recordedTermText.text = processedSpeech;
            _latestRecordedTerm = processedSpeech;
        }
    }
}