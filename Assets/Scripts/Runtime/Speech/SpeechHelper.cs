using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

namespace JZK.Input
{
    public static class SpeechHelper
    {

        public static List<ESpeechRegion> ALL_LANGUAGE_ENUMS => Enum.GetValues(typeof(ESpeechRegion)).Cast<ESpeechRegion>().ToList();
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

            { ESpeechInputType.Game_LeftShoulder, "bumper" },
            { ESpeechInputType.Game_LeftTrigger, "trigger" },

            { ESpeechInputType.Game_RightShoulder, "shoulder" },
            { ESpeechInputType.Game_RightTrigger, "guard" },

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

            { ESpeechInputType.Game_LeftShoulder, "Left Shoulder" },
            { ESpeechInputType.Game_LeftTrigger, "Left Trigger" },

            { ESpeechInputType.Game_RightShoulder, "Right Shoulder" },
            { ESpeechInputType.Game_RightTrigger, "Right Trigger" },

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

        public static string RegionStringFromEnum(ESpeechRegion regionEnum)
        {
            switch(regionEnum)
            {
                case ESpeechRegion.English_GB:
                    return "en-GB";
                case ESpeechRegion.English_AU:
                    return "en-AU";
                case ESpeechRegion.English_CA:
                    return "en-CA";
                case ESpeechRegion.English_GH:
                    return "en-GH";
                case ESpeechRegion.English_HK:
                    return "en-HK";
                case ESpeechRegion.English_IE:
                    return "en-IE";
                case ESpeechRegion.English_IN:
                    return "en-IN";
                case ESpeechRegion.English_KE:
                    return "en-KE";
                case ESpeechRegion.English_NG:
                    return "en-NG";
                case ESpeechRegion.English_NZ:
                    return "en-NZ";
                case ESpeechRegion.English_PH:
                    return "en-PH";
                case ESpeechRegion.English_US:
                    return "en-US";
                case ESpeechRegion.English_SG:
                    return "en-SG";
                case ESpeechRegion.English_TZ:
                    return "en-TZ";
                case ESpeechRegion.English_ZA:
                    return "en-ZA";
                case ESpeechRegion.Afrikaans:
                    return "af-ZA";
                case ESpeechRegion.Amharic:
                    return "am-ET";
                case ESpeechRegion.Arabic_UAE:
                    return "ar-AE";
                case ESpeechRegion.Arabic_BH:
                    return "ar-BH";
                case ESpeechRegion.Arabic_EG:
                    return "ar-EG";
                case ESpeechRegion.Arabic_IL:
                    return "ar-IL";
                case ESpeechRegion.Arabic_IQ:
                    return "ar-IQ";
                case ESpeechRegion.Arabic_JO:
                    return "ar-JO";
                case ESpeechRegion.Arabic_KW:
                    return "ar-KW";
                case ESpeechRegion.Arabic_LB:
                    return "ar-LB";
                case ESpeechRegion.Arabic_LY:
                    return "ar-LY";
                case ESpeechRegion.Arabic_MA:
                    return "ar-MA";
                case ESpeechRegion.Arabic_PS:
                    return "ar-PS";
                case ESpeechRegion.Arabic_QA:
                    return "ar-QA";
                case ESpeechRegion.Arabic_SA:
                    return "ar-SA";
                case ESpeechRegion.Arabic_SY:
                    return "ar-SY";
                case ESpeechRegion.Arabic_TN:
                    return "ar-TN";
                case ESpeechRegion.Arabic_YE:
                    return "ar-YE";
                case ESpeechRegion.Azerbaijani:
                    return "az-AZ";
                case ESpeechRegion.Bulgarian:
                    return "bg-BG";
                case ESpeechRegion.Bengali:
                    return "bn-IN";
                case ESpeechRegion.Bosnian:
                    return "bs_BA";
                case ESpeechRegion.Catalan:
                    return "ca-ES";
                case ESpeechRegion.Czech:
                    return "cs-CZ";
                case ESpeechRegion.Welsh:
                    return "cy-GB";
                case ESpeechRegion.Danish:
                    return "da-DK";
                case ESpeechRegion.German_AU:
                    return "de-AT";
                case ESpeechRegion.German_GE:
                    return "de-DE";
                case ESpeechRegion.German_SZ:
                    return "de-CH";
                case ESpeechRegion.Greek:
                    return "el-GR";
                case ESpeechRegion.Spanish_AR:
                    return "es-AR";
                case ESpeechRegion.Spanish_BO:
                    return "es-BO";
                case ESpeechRegion.Spanish_CL:
                    return "es-CL";
                case ESpeechRegion.Spanish_CO:
                    return "es-CO";
                case ESpeechRegion.Spanish_CR:
                    return "es-CR";
                case ESpeechRegion.Spanish_CU:
                    return "es-CU";
                case ESpeechRegion.Spanish_DO:
                    return "es-DO";
                case ESpeechRegion.Spanish_EC:
                    return "es-EC";
                case ESpeechRegion.Spanish_SP:
                    return "es-ES";
                case ESpeechRegion.Spanish_EQ:
                    return "es-GQ";
                case ESpeechRegion.Spanish_GU:
                    return "es-GT";
                case ESpeechRegion.Spanish_HN:
                    return "es-HN";
                case ESpeechRegion.Spanish_MX:
                    return "es-MX";
                case ESpeechRegion.Spanish_NI:
                    return "es-NI";
                case ESpeechRegion.Spanish_PA:
                    return "es-PA";
                case ESpeechRegion.Spanish_PE:
                    return "es-PE";
                case ESpeechRegion.Spanish_PR:
                    return "es-PR";
                case ESpeechRegion.Spanish_PG:
                    return "es-PY";
                case ESpeechRegion.Spanish_SV:
                    return "es-SV";
                case ESpeechRegion.Spanish_US:
                    return "es-US";
                case ESpeechRegion.Spanish_UG:
                    return "es-UY";
                case ESpeechRegion.Spanish_VE:
                    return "es-VE";
                case ESpeechRegion.Estonian:
                    return "et-EE";
                case ESpeechRegion.Basque:
                    return "eu-ES";
                case ESpeechRegion.Persian:
                    return "fa-IR";
                case ESpeechRegion.Finnish:
                    return "fi-FI";
                case ESpeechRegion.Filipino:
                    return "fil-PH";
                case ESpeechRegion.French_BE:
                    return "fr-BE";
                case ESpeechRegion.French_CA:
                    return "fr-CA";
                case ESpeechRegion.French_SW:
                    return "fr-CH";
                case ESpeechRegion.French_FR:
                    return "fr-FR";
                case ESpeechRegion.Irish:
                    return "ga-IE";
                case ESpeechRegion.Galician:
                    return "gl-ES";
                case ESpeechRegion.Gujarati:
                    return "gu-IN";
                case ESpeechRegion.Hebrew:
                    return "he-IL";
                case ESpeechRegion.Hindi:
                    return "hi-IN";
                case ESpeechRegion.Croatian:
                    return "hr-HR";
                case ESpeechRegion.Hungarian:
                    return "hu-HU";
                case ESpeechRegion.Armenian:
                    return "hy-AM";
                case ESpeechRegion.Indonesian:
                    return "id-ID";
                case ESpeechRegion.Icelandic:
                    return "is-IS";
                case ESpeechRegion.Italian_SW:
                    return "it-CH";
                case ESpeechRegion.Italian_IT:
                    return "it-IT";
                case ESpeechRegion.Japanese:
                    return "ja-JP";
                case ESpeechRegion.Javanese:
                    return "jv-ID";
                case ESpeechRegion.Georgian:
                    return "ka-GE";
                case ESpeechRegion.Kazakh:
                    return "kk-KZ";
                case ESpeechRegion.Khmer:
                    return "km-KH";
                case ESpeechRegion.Kannada:
                    return "kn-IN";
                case ESpeechRegion.Korean:
                    return "ko-KR";
                case ESpeechRegion.Lao:
                    return "lo-LA";
                case ESpeechRegion.Latvian:
                    return "lv-LV";
                case ESpeechRegion.Macedonian:
                    return "mk-MK";
                case ESpeechRegion.Malaylam:
                    return "ml-IN";
                case ESpeechRegion.Mongolian:
                    return "mn-MN";
                case ESpeechRegion.Marathi:
                    return "mr-IN";
                case ESpeechRegion.Malay:
                    return "ms-MY";
                case ESpeechRegion.Maltese:
                    return "mt-MT";
                case ESpeechRegion.Burmese:
                    return "my-MM";
                case ESpeechRegion.Bokmal:
                    return "nb-NO";
                case ESpeechRegion.Nepali:
                    return "ne-NP";
                case ESpeechRegion.Dutch_BE:
                    return "nl-BE";
                case ESpeechRegion.Dutch_NL:
                    return "nl-NL";
                case ESpeechRegion.Punjabi:
                    return "pa-IN";
                case ESpeechRegion.Polish:
                    return "pl-PL";
                case ESpeechRegion.Pashto_AF:
                    return "ps-AF";
                case ESpeechRegion.Portuguese_BR:
                    return "pt-BR";
                case ESpeechRegion.Portuguese_PT:
                    return "pt-PT";
                case ESpeechRegion.Romanian:
                    return "ro-RO";
                case ESpeechRegion.Russian:
                    return "ru-RU";
                case ESpeechRegion.Sinhala:
                    return "si-LK";
                case ESpeechRegion.Slovak:
                    return "sk-SK";
                case ESpeechRegion.Slovenian:
                    return "sl-SI";
                case ESpeechRegion.Somali:
                    return "so-SO";
                case ESpeechRegion.Albanian:
                    return "sq-AL";
                case ESpeechRegion.Serbian:
                    return "sr-RS";
                case ESpeechRegion.Swedish:
                    return "sv-SE";
                case ESpeechRegion.Kiswahili_KE:
                    return "sw-KE";
                case ESpeechRegion.Kiswahili_TZ:
                    return "sw-TZ";
                case ESpeechRegion.Tamil:
                    return "ta-IN";
                case ESpeechRegion.Telugu:
                    return "te-IN";
                case ESpeechRegion.Thai:
                    return "th-TH";
                case ESpeechRegion.Turkish:
                    return "tr-TR";
                case ESpeechRegion.Ukrainian:
                    return "uk-UA";
                case ESpeechRegion.Urdu:
                    return "ur-IN";
                case ESpeechRegion.Uzbek:
                    return "uz-US";
                case ESpeechRegion.Vietnamese:
                    return "vi-VN";
                case ESpeechRegion.Chinese_WU:
                    return "wuu-CN";
                case ESpeechRegion.Cantonese_Simp:
                    return "yue-CN";
                case ESpeechRegion.Cantonese_Trad:
                    return "zh-HK";
                case ESpeechRegion.Mandarin:
                    return "zh-CN";
                case ESpeechRegion.Mandarin_JL:
                    return "zh-CN-shandong";
                case ESpeechRegion.Mandarin_SW:
                    return "zh-CN-sichuan";
                case ESpeechRegion.Mandarin_TW:
                    return "zh-TW";
                case ESpeechRegion.IsiZulu:
                    return "zu-ZA";
                default:
                    Debug.Log("SPEECHHELPER - type " + regionEnum.ToString() + " - not supported - returning en-GB");
                    return "en-GB";
            }
        }
    }
}