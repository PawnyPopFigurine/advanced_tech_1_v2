using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

            { ESpeechInputType.Game_FaceEast, "Gamepad East" },
            { ESpeechInputType.Game_FaceNorth, "Gamepad North" },
            { ESpeechInputType.Game_FaceSouth, "Gamepad South" },
            { ESpeechInputType.Game_FaceWest, "Gamepad West" },

            { ESpeechInputType.UI_Confirm, "UI Confirm" },
            { ESpeechInputType.UI_Back, "UI Back" },

            { ESpeechInputType.None, "None" },
        };
    }
}