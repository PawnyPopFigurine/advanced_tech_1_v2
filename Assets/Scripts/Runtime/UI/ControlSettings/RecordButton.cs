using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Input;

namespace JZK.UI
{
    public class RecordButton : MonoBehaviour
    {
        [SerializeField] ESpeechInputType _type;
        public ESpeechInputType Type => _type;

        public void Input_ButtonPressed()
        {
            ControlSettingsUISystem.Instance.Input_RecordButtonPressed(_type);
        }
    }
}