using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

namespace JZK.Input
{
    public static class SpeechHelper
    {
        public static string ProcessSpeechTerm(string rawSpeech)
        {
            string processedSpeech = rawSpeech.ToLower();
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            processedSpeech = rgx.Replace(processedSpeech, "");
            return processedSpeech;
        }


        public static Dictionary<ESpeechInputType, string> DEFAULT_TERMS = new()
        {
            { ESpeechInputType.Game_DPadDown, "down"},
            { ESpeechInputType.Game_DPadUp, "up" },
            { ESpeechInputType.Game_DPadLeft, "left" },
            { ESpeechInputType.Game_DPadRight, "right" },

            { ESpeechInputType.Game_FaceEast, "east" },
            { ESpeechInputType.Game_FaceNorth, "north" },
            { ESpeechInputType.Game_FaceSouth, "south" },
            { ESpeechInputType.Game_FaceWest, "west" },

            { ESpeechInputType.UI_Back, "back" },
            { ESpeechInputType.UI_Confirm, "confirm" },

        };

        public static Dictionary<ESpeechInputType, string> INPUT_NAMES = new()
        {
            { ESpeechInputType.Game_DPadDown, "D-Pad Down"},
            { ESpeechInputType.Game_DPadUp, "D-Pad Up" },
            { ESpeechInputType.Game_DPadLeft, "D-Pad Left" },
            { ESpeechInputType.Game_DPadRight, "D-Pad Right" },

            { ESpeechInputType.Game_FaceEast, "Face East" },
            { ESpeechInputType.Game_FaceNorth, "Face North" },
            { ESpeechInputType.Game_FaceSouth, "Face South" },
            { ESpeechInputType.Game_FaceWest, "Face West" },

            { ESpeechInputType.UI_Confirm, "UI Confirm" },
            { ESpeechInputType.UI_Back, "UI Back" },

            { ESpeechInputType.None, "None" },
        };

        public static ESpeechInputType_Flag FlagFromEnum(ESpeechInputType enumType)
        {
            switch(enumType)
            {
                case ESpeechInputType.None:
                    return ESpeechInputType_Flag.None;
                case ESpeechInputType.Game_DPadUp:
                    return ESpeechInputType_Flag.Game_DPadUp;
                case ESpeechInputType.Game_DPadDown:
                    return ESpeechInputType_Flag.Game_DPadDown;
                case ESpeechInputType.Game_DPadLeft:
                    return ESpeechInputType_Flag.Game_DPadLeft;
                case ESpeechInputType.Game_DPadRight:
                    return ESpeechInputType_Flag.Game_DPadRight;
                case ESpeechInputType.Game_FaceNorth:
                    return ESpeechInputType_Flag.Game_FaceNorth;
                case ESpeechInputType.Game_FaceSouth:
                    return ESpeechInputType_Flag.Game_FaceSouth;
                case ESpeechInputType.Game_FaceWest:
                    return ESpeechInputType_Flag.Game_FaceWest;
                case ESpeechInputType.Game_FaceEast:
                    return ESpeechInputType_Flag.Game_FaceEast;
                case ESpeechInputType.Game_LeftShoulder:
                    return ESpeechInputType_Flag.Game_LeftShoulder;
                case ESpeechInputType.Game_LeftTrigger:
                    return ESpeechInputType_Flag.Game_LeftTrigger;
                case ESpeechInputType.Game_RightShoulder:
                    return ESpeechInputType_Flag.Game_RightShoulder;
                case ESpeechInputType.Game_RightTrigger:
                    return ESpeechInputType_Flag.Game_RightTrigger;
                case ESpeechInputType.UI_Confirm:
                    return ESpeechInputType_Flag.UI_Confirm;
                case ESpeechInputType.UI_Back:
                    return ESpeechInputType_Flag.UI_Back;
                case ESpeechInputType.Max:
                    return ESpeechInputType_Flag.Max;
                case ESpeechInputType.Invalid:
                    return ESpeechInputType_Flag.Invalid;
                default:
                    Debug.Log("SPEECHHELPER - type " + enumType.ToString() + " - not supported - returning None");
                    return ESpeechInputType_Flag.None;
            }
        }
    }
}