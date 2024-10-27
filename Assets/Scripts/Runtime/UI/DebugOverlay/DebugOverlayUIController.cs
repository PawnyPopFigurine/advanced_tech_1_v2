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

            if(SpeechRecognitionSystem.Instance.RecordedThisFrame)
            {
                _latestSpeechText.text = SpeechRecognitionSystem.Instance.LatestRecordedSpeech;
            }

            SetLatestInputText(SpeechInputSystem.Instance.DebugLatestInput);
        }

        public void SetLatestInputText(ESpeechInputType type)
        {
            if(!SpeechHelper.INPUT_NAMES.TryGetValue(type, out string inputName))
            {
                return;
            }

            _latestInputText.text = inputName;
        }
    }
}