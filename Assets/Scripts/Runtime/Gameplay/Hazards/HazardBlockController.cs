using JZK.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace JZK.Gameplay
{
    public class HazardBlockController : MonoBehaviour
    {
        [SerializeField] HazardBlockHitbox _hitbox;

        ESpeechInputType_Flag _inputType;
        public ESpeechInputType_Flag InputType => _inputType;

        [SerializeField] List<HazardButtonPrompt> _promptVisuals;

        [SerializeField] TMP_Text _termText;

        public void Initialise(ESpeechInputType_Flag inputType)
        {
            _inputType = inputType;
            RefreshPromptForInputType();
            RefreshTermTextForInputType();
        }

        void RefreshPromptForInputType()
        {
            foreach(HazardButtonPrompt prompt in _promptVisuals)
            {
                if(prompt.InputType == _inputType)
                {
                    prompt.gameObject.SetActive(true);
                }
                else
                {
                    prompt.gameObject.SetActive(false);
                }
            }
        }

        void RefreshTermTextForInputType()
        {
            ESpeechInputType typeEnum = ESpeechInputType.None;

            switch(_inputType)
            {
                case ESpeechInputType_Flag.Game_DPadUp:
                    typeEnum = ESpeechInputType.Game_DPadUp;
                    break;
                case ESpeechInputType_Flag.Game_DPadDown:
                    typeEnum = ESpeechInputType.Game_DPadDown;
                    break;
                case ESpeechInputType_Flag.Game_DPadLeft:
                    typeEnum = ESpeechInputType.Game_DPadLeft;
                    break;
                case ESpeechInputType_Flag.Game_DPadRight:
                    typeEnum = ESpeechInputType.Game_DPadRight;
                    break;
                case ESpeechInputType_Flag.Game_FaceNorth:
                    typeEnum = ESpeechInputType.Game_FaceNorth;
                    break;
                case ESpeechInputType_Flag.Game_FaceSouth:
                    typeEnum = ESpeechInputType.Game_FaceSouth;
                    break;
                case ESpeechInputType_Flag.Game_FaceWest:
                    typeEnum = ESpeechInputType.Game_FaceWest;
                    break;
                case ESpeechInputType_Flag.Game_FaceEast:
                    typeEnum = ESpeechInputType.Game_FaceEast;
                    break;
            }

            if(typeEnum != ESpeechInputType.None)
            {
                string speechTerm = SpeechInputSystem.Instance.GetTermForType(typeEnum);
                _termText.text = speechTerm;
            }
        }

        public void OnCollide(GameObject collision)
        {
            if(collision.transform.tag != "Player")
            {
                return;
            }

            GameplaySystem.Instance.OnPlayerHitHazard();
        }
    }
}