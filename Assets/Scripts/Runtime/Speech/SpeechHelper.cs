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

        };
    }
}