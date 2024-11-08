using JZK.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class HazardBlockController : MonoBehaviour
    {
        [SerializeField] HazardBlockHitbox _hitbox;

        ESpeechInputType_Flag _inputType;
        public ESpeechInputType_Flag InputType => _inputType;

        [SerializeField] List<HazardButtonPrompt> _promptVisuals;

        public void Initialise(ESpeechInputType_Flag inputType)
        {
            _inputType = inputType;
            RefreshPromptForInputType();
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