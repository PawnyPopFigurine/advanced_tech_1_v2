using JZK.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.UI
{
    public class ScrollItemTermText : MonoBehaviour
    {
        [SerializeField] ESpeechInputType _inputType;
        public ESpeechInputType InputType => _inputType;
    }
}