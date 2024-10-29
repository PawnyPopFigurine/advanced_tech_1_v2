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

        [SerializeField] TMP_Text _confirm;
        [SerializeField] TMP_Text _back;


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

            if(SpeechRecognitionSystem.Instance.RecordedThisFrame)
            {
                _latestSpeechText.text = SpeechRecognitionSystem.Instance.LatestRecordedSpeech;
            }

            if(SpeechInputSystem.Instance.PressedThisFrame)
            {
                SetLatestInputText(SpeechInputSystem.Instance.DebugLatestInput);
            }
            UpdateInputTextForGamepad();
        }

        public void RefreshTerms()
        {
            Dictionary<string, ESpeechInputType> currentTerms = new(SpeechInputSystem.Instance.SpeechTermInput_LUT);
            foreach (string validTerm in currentTerms.Keys)
            {
                ESpeechInputType type = currentTerms[validTerm];
                switch(type)
                {
                    case ESpeechInputType.Game_DPadUp:
                        _up.SetText(validTerm); break;
                    case ESpeechInputType.Game_DPadDown:
                        _down.SetText(validTerm); break;
                    case ESpeechInputType.Game_DPadLeft:
                        _left.SetText(validTerm); break;
                    case ESpeechInputType.Game_DPadRight:
                        _right.SetText(validTerm); break;
                    case ESpeechInputType.Game_FaceNorth:
                        _north.SetText(validTerm); break;
                    case ESpeechInputType.Game_FaceSouth:
                        _south.SetText(validTerm); break;
                    case ESpeechInputType.Game_FaceEast:
                        _east.SetText(validTerm); break;
                    case ESpeechInputType.Game_FaceWest:
                        _west.SetText(validTerm); break;
                    case ESpeechInputType.UI_Confirm:
                        _confirm.SetText(validTerm); break;
                    case ESpeechInputType.UI_Back:
                        _back.SetText(validTerm); break;
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
    }
}