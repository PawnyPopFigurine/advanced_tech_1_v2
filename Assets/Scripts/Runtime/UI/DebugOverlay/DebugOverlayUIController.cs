using JZK.Input;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JZK.UI
{
    public class DebugOverlayUIController : UIController
    {
        [SerializeField] TMP_Text _latestSpeechText;
        [SerializeField] TMP_Text _latestInputText;

        [SerializeField] TMP_Text _north;
        [SerializeField] TMP_Text _south;
        [SerializeField] TMP_Text _east;
        [SerializeField] TMP_Text _west;

        [SerializeField] TMP_Text _up;
        [SerializeField] TMP_Text _down;
        [SerializeField] TMP_Text _left;
        [SerializeField] TMP_Text _right;

        [SerializeField] TMP_Text _leftShoulder;
        [SerializeField] TMP_Text _leftTrigger;
        [SerializeField] TMP_Text _rightShoulder;
        [SerializeField] TMP_Text _rightTrigger;

        [SerializeField] TMP_Text _confirm;
        [SerializeField] TMP_Text _back;

        [SerializeField] TMP_Text _voiceControlEnabledText;

        [SerializeField] TMP_Text _isRecordingTrue;
        [SerializeField] TMP_Text _isRecordingFalse;


        public override void SetActive(bool active)
        {
            base.SetActive(active);
            if (active)
            {
                RefreshOnActive();
            }
        }

        void RefreshOnActive()
        {

        }

        public override void UpdateController()
        {
            base.UpdateController();

            RefreshTerms();
            if (SpeechInputSystem.Instance.VoiceControlEnabled)
            {
                if (SpeechRecognitionSystem.Instance.RecordedThisFrame)
                {
                    _latestSpeechText.text = SpeechRecognitionSystem.Instance.LatestRecordedSpeech;
                }
            
                UpdateInputTextForSpeechInput();
            }

            UpdateInputTextForGamepad();

            string voiceEnabledText = SpeechInputSystem.Instance.VoiceControlEnabled ? "ON" : "OFF";

            _voiceControlEnabledText.text = voiceEnabledText;

            bool isRecording = SpeechRecognitionSystem.Instance.IsRecording;
            _isRecordingTrue.gameObject.SetActive(isRecording);
            _isRecordingFalse.gameObject.SetActive(!isRecording);
        }

        public void RefreshTerms()
        {
            Dictionary<ESpeechInputType, string> currentTerms = new(SpeechInputSystem.Instance.SpeechInputTerm_LUT);
            foreach (ESpeechInputType validType in currentTerms.Keys)
            {
                string term = currentTerms[validType];
                switch(validType)
                {
                    case ESpeechInputType.Game_DPadUp:
                        _up.SetText(term); break;
                    case ESpeechInputType.Game_DPadDown:
                        _down.SetText(term); break;
                    case ESpeechInputType.Game_DPadLeft:
                        _left.SetText(term); break;
                    case ESpeechInputType.Game_DPadRight:
                        _right.SetText(term); break;
                    case ESpeechInputType.Game_FaceNorth:
                        _north.SetText(term); break;
                    case ESpeechInputType.Game_FaceSouth:
                        _south.SetText(term); break;
                    case ESpeechInputType.Game_FaceEast:
                        _east.SetText(term); break;
                    case ESpeechInputType.Game_FaceWest:
                        _west.SetText(term); break;
                    case ESpeechInputType.UI_Confirm:
                        _confirm.SetText(term); break;
                    case ESpeechInputType.UI_Back:
                        _back.SetText(term); break;
                    case ESpeechInputType.Game_LeftShoulder:
                        _leftShoulder.SetText(term); break;
                    case ESpeechInputType.Game_LeftTrigger:
                        _leftTrigger.SetText(term); break;
                    case ESpeechInputType.Game_RightShoulder:
                        _rightShoulder.SetText(term); break;
                    case ESpeechInputType.Game_RightTrigger:
                        _rightTrigger.SetText(term); break;
                    default:
                        break;
                }
            }
        }

        public void SetLatestInputText(ESpeechInputType type)
        {
            if(!SpeechHelper.INPUT_NAMES.TryGetValue(type, out string inputName))
            {
                return;
            }

            _latestInputText.text = inputName;
        }

        public void SetLatestInputText(ESpeechInputType_Flag type)
        {
            _latestInputText.text = type.ToString();
        }

        void UpdateInputTextForSpeechInput()
        {
            if (SpeechInputSystem.Instance.PressedThisFrame)
            {
                SetLatestInputText(SpeechInputSystem.Instance.DebugLatestInputFlag);
            }
        }

        void UpdateInputTextForGamepad()
        {
            if(InputSystem.Instance.AnyButtonPressed)
            {
                ESpeechInputType inputSystemDebugType = InputSystem.Instance.DebugLatestInput;
                if (inputSystemDebugType != ESpeechInputType.None)
                {
                    SetLatestInputText(inputSystemDebugType);
                }
            }
        }

        public void Input_ToggleVoiceControl()
        {
            SpeechInputSystem.Instance.ToggleVoiceInputEnabled();
        }
    }
}