using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;
using System.Text.RegularExpressions;

namespace JZK.Input
{
    public enum ESpeechInputType
    {
        None = 0,

        Game_DPadUp,
        Game_DPadDown,
        Game_DPadLeft,
        Game_DPadRight,

        Game_FaceNorth,
        Game_FaceSouth,
        Game_FaceWest,
        Game_FaceEast,

        Game_LeftShoulder,
        Game_LeftTrigger,
        Game_RightShoulder,
        Game_RightTrigger,

        UI_Up,
        UI_Down,
        UI_Left,
        UI_Right,

        UI_Confirm,
        UI_Cancel,

        Max,
        Invalid
    }

    //Takes in recognised speech and matches it to game input.
    public class SpeechInputSystem : PersistentSystem<SpeechInputSystem>
    {
        public SystemLoadData _loadData = new SystemLoadData()
        {
            LoadStates = SystemLoadState.NoLoadingNeeded,
            UpdateAfterLoadingState = ELoadingState.Game,
        };

        public override SystemLoadData LoadData => _loadData;

        Dictionary<string, ESpeechInputType> _speechTermInput_LUT = new()
        {
            {"green", ESpeechInputType.Game_FaceNorth },
            {"blue", ESpeechInputType.Game_FaceSouth },
            {"red", ESpeechInputType.Game_FaceEast },
            {"pink", ESpeechInputType.Game_FaceWest },

            {"up", ESpeechInputType.Game_DPadUp },
            {"down", ESpeechInputType.Game_DPadDown },
            {"left", ESpeechInputType.Game_DPadLeft },
            {"right", ESpeechInputType.Game_DPadRight },
        };

        public bool NorthFacePressed { get; private set; }
        public bool SouthFacePressed { get; private set; }
        public bool EastFacePressed { get; private set; }
        public bool WestFacePressed { get; private set; }
        public bool DPadUpPressed { get; private set; }
        public bool DPadDownPressed { get; private set; }
        public bool DPadLeftPressed { get; private set; }
        public bool DPadRightPressed { get; private set; }


        public override void UpdateSystem()
        {
            base.UpdateSystem();
            ClearSpeechInput();
        }

        ESpeechInputType GetInputForRecognisedSpeech(string speech)
        {
            foreach(string keyString in _speechTermInput_LUT.Keys)
            {
                if(keyString == speech)
                {
                    return _speechTermInput_LUT[keyString];
                }
            }

            return ESpeechInputType.None;
        }

        public void OnSpeechRecognized(string speechTerm)
        {
            //Debug.Log("[HELLO] recognized speech " + speechTerm);
            
            string processedTerm = speechTerm.ToLower();

            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            processedTerm = rgx.Replace(processedTerm, "");

            ESpeechInputType inputType = GetInputForRecognisedSpeech(processedTerm);

            switch(inputType)
            {
                case ESpeechInputType.Game_FaceNorth:
                    NorthFacePressed = true;
                    break;
                case ESpeechInputType.Game_FaceSouth:
                    SouthFacePressed = true;
                    break;
                case ESpeechInputType.Game_FaceWest:
                    WestFacePressed = true;
                    break;
                case ESpeechInputType.Game_FaceEast:
                    EastFacePressed = true;
                    break;
                case ESpeechInputType.Game_DPadUp:
                    DPadUpPressed = true;
                    break;
                case ESpeechInputType.Game_DPadDown:
                    DPadDownPressed = true;
                    break;
                case ESpeechInputType.Game_DPadLeft:
                    DPadLeftPressed = true;
                    break;
                case ESpeechInputType.Game_DPadRight:
                    DPadRightPressed = true;
                    break;
                default:
                    Debug.Log(this.name + " - no recognised input type for speech " + processedTerm);
                    break;
            }
        }

        void ClearSpeechInput()
        {
            NorthFacePressed = false;
            SouthFacePressed = false;
            EastFacePressed = false;
            WestFacePressed = false;

            DPadUpPressed = false;
            DPadDownPressed = false;
            DPadLeftPressed = false;
            DPadRightPressed = false;
        }
    }
}