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

        public void Initialise(ESpeechInputType_Flag inputType)
        {
            _inputType = inputType;
            _hitbox.gameObject.transform.localPosition = Vector3.zero;
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