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
    }
}