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

        bool _popupOpen;

        [SerializeField] GameObject _recordingPopup;
        [SerializeField] GameObject _popupHeader;
        [SerializeField] GameObject _recordedTerm;
        [SerializeField] TMP_Text _recordedTermText;

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
        }

        public override void UpdateController()
        {
            base.UpdateController();
        }

        public void UpdateInput()
        {

        }

        void TogglePopup(bool active)
        {
            _recordingPopup.SetActive(active);
            _popupOpen = active;

            if(active)
            {
                RefreshPopupOnOpen();
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
        }
    }
}