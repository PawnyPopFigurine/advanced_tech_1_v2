using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;
using System.Text.RegularExpressions;
using JZK.Save;
using System;
using UnityEngine.UIElements;
using System.Linq;
using JZK.UI;

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

        UI_Confirm,
        UI_Back,

        Max,
        Invalid
    }

    [Flags]
    public enum ESpeechInputType_Flag
    {
        None = 0,

        Game_DPadUp = 1,
        Game_DPadDown = 2,
        Game_DPadLeft = 4,
        Game_DPadRight = 8,

        Game_FaceNorth = 16,
        Game_FaceSouth = 32,
        Game_FaceWest = 64,
        Game_FaceEast = 128,

        Game_LeftShoulder = 256,
        Game_LeftTrigger = 512,
        Game_RightShoulder = 1024,
        Game_RightTrigger = 2048,

        UI_Confirm = 4096,
        UI_Back = 8192,

        Max = 16384,
        Invalid = 32768,
    }

    public enum ESpeechRegion
    {
        English_GB, //british english
        English_AU, //aussie engilsh
        English_CA, //canada english
        English_GH, //ghana english
        English_HK, //hong kong english
        English_IE, //ireland english
        English_IN, //indian english
        English_KE, //kenyan english
        English_NG, //nigerian english
        English_NZ, //new zealand english
        English_PH, //phillipines english
        English_SG, //singapore english
        English_TZ, //tanzania english
        English_ZA, //south africa english
        English_US, //american english
        Afrikaans,  //afrikaans
        Amharic,    //amharic (ethiopia)
        Arabic_UAE, //UAE arabic
        Arabic_BH,  //Bahrain arabic
        Arabic_AL,  //Algeria arabic
        Arabic_EG,  //Egypt arabic
        Arabic_IL,  //Israel arabic
        Arabic_IQ,  //Iraq arabic
        Arabic_JO,  //Jordan arabic
        Arabic_KW,  //Kuwait arabic
        Arabic_LB,  //Lebanon arabic
        Arabic_LY,  //Lybian arabic
        Arabic_MA,  //Morocco arabic
        Arabic_OM,  //Oman arabic
        Arabic_PS,  //Palestine arabic
        Arabic_QA,  //Qatar arabic
        Arabic_SA,  //Saudi arabic
        Arabic_SY,  //Syrian arabic
        Arabic_TN,  //Tunisian arabic
        Arabic_YE,  //Yemen arabic
        Azerbaijani,
        Bulgarian,
        Bengali,
        Bosnian,
        Catalan,
        Czech,
        Welsh,
        Danish,
        German_AU,  //Austrian german
        German_SZ,  //Swiss german
        German_GE,  //German German
        Greek,
        Spanish_AR, //Argentinian spanish
        Spanish_BO, //Bolivian spanish
        Spanish_CL, //Chilean spanish
        Spanish_CO, //Colombian spanish
        Spanish_CR, //Costa Rica spanish
        Spanish_CU, //Cuban spanish
        Spanish_DO, //Dominican spanish
        Spanish_EC, //Ecuador spanish
        Spanish_SP, //Spanish spanish
        Spanish_EQ, //Equatorial guinea spanish
        Spanish_GU, //Guatemalan spanish
        Spanish_HN, //Honduras spanish
        Spanish_MX, //Mexican spanish
        Spanish_NI, //Nicaraguan spanish
        Spanish_PA, //Panama spanish
        Spanish_PE, //Peru spanish
        Spanish_PR, //Puerto rico spanish
        Spanish_PG, //Paraguay spanish
        Spanish_SV, //El Salvador spanish
        Spanish_US, //American spanish
        Spanish_UG, //Uruguay spanish
        Spanish_VE, //Venezuelan spanish
        Estonian,   
        Basque,
        Persian,
        Finnish,
        Filipino,
        French_BE, //Belgian french
        French_CA, //Canadian french
        French_SW, //Swiss french
        French_FR, //French french
        Irish,     //Irish
        Galician,  //Galician
        Gujarati,  //Gujarati (indian)
        Hebrew,
        Hindi,     //Hindi
        Croatian,
        Hungarian,
        Armenian,
        Indonesian,
        Icelandic,
        Italian_SW, //Swiss italian
        Italian_IT, //Italian italian
        Japanese,
        Javanese,
        Georgian,
        Kazakh,
        Khmer,
        Kannada,    //Kannada (indian)
        Korean,
        Lao,
        Lithuanian,
        Latvian,
        Macedonian,
        Malaylam,  //Malaylan (indian)
        Mongolian,
        Marathi,
        Malay,
        Maltese,
        Burmese,
        Bokmal,    //Norwegian bokmal
        Nepali,
        Dutch_BE,  //Belgian dutch
        Dutch_NL,  //Netherlands dutch
        Punjabi,
        Polish,
        Pashto_AF,  //Afghan pashto
        Portuguese_BR, //Brazilian
        Portuguese_PT, //Portuguese portuguese
        Romanian,
        Russian,
        Sinhala,    //Sinhala (sri lankan)
        Slovak,
        Slovenian,
        Somali,
        Albanian,
        Serbian,
        Swedish,
        Kiswahili_KE, //Kenyan Kiswahili
        Kiswahili_TZ, //Tanzanian Kiswahili
        Tamil,
        Telugu,
        Thai,
        Turkish,
        Ukrainian,
        Urdu,
        Uzbek,
        Vietnamese,
        Chinese_WU,  //simplified wu chinese
        Cantonese_Simp,  //simplified cantonese
        Cantonese_Trad,  //traditional cantonese
        Mandarin,    //simplified mandarin
        Mandarin_JL, //jilu mandarin
        Mandarin_SW, //simplified southwest mandarin
        Mandarin_TW, //traditional taiwanese mandarin
        IsiZulu,
    }

    public class SpeechInputData
    {
        public float TimeToNextPress;
        public List<ESpeechInputType> InputQueue = new List<ESpeechInputType>();
    }

    //Takes in recognised speech and matches it to game input.
    public class SpeechInputSystem : GameSystem<SpeechInputSystem>
    {
        public SystemLoadData _loadData = new SystemLoadData()
        {
            LoadStates = SystemLoadState.NoLoadingNeeded,
            UpdateAfterLoadingState = ELoadingState.Game,
        };

        public override SystemLoadData LoadData => _loadData;

        Dictionary<ESpeechInputType, string> _speechInputTerm_LUT = new();
        public Dictionary<ESpeechInputType, string> SpeechInputTerm_LUT => _speechInputTerm_LUT;

        public bool NorthFacePressed { get; private set; }
        public bool SouthFacePressed { get; private set; }
        public bool EastFacePressed { get; private set; }
        public bool WestFacePressed { get; private set; }
        public bool DPadUpPressed { get; private set; }
        public bool DPadDownPressed { get; private set; }
        public bool DPadLeftPressed { get; private set; }
        public bool DPadRightPressed { get; private set; }

        public bool UIConfirmPressed { get; private set; }
        public bool UIBackPressed { get; private set; }

        public bool LeftShoulderPressed { get; private set; }
        public bool RightShoulderPressed { get; private set; }
        public bool LeftTriggerPressed { get; private set; }
        public bool RightTriggerPressed { get; private set; }

        public ESpeechInputType DebugLatestInput { get; private set; }
        public ESpeechInputType_Flag DebugLatestInputFlag { get; private set; }

        public bool VoiceControlEnabled { get; private set; }

        SpeechInputData _latestSpeechInputData;

        static float SPEECH_INPUT_DELAY = 0.2f;

        public bool PressedThisFrame
        {
            get
            {
                return (NorthFacePressed ||
                    SouthFacePressed ||
                    EastFacePressed ||
                    WestFacePressed ||
                    DPadUpPressed ||
                    DPadDownPressed ||
                    DPadLeftPressed ||
                    DPadRightPressed ||
                    UIConfirmPressed ||
                    UIBackPressed ||
                    LeftShoulderPressed ||
                    LeftTriggerPressed ||
                    RightShoulderPressed ||
                    RightTriggerPressed);
            }
        }

        public override void Initialise()
        {
            base.Initialise();

            VoiceControlEnabled = true;
        }

        public override void UpdateSystem()
        {
            base.UpdateSystem();
            ClearSpeechInput();

            if(SpeechRecognitionSystem.Instance.RecordedThisFrame)
            {
                OnSpeechRecognized(SpeechRecognitionSystem.Instance.LatestRecordedSpeech);
            }

            UpdateInputForLatestSpeechData();
        }

        public override void SetCallbacks()
        {
            base.SetCallbacks();

            SpeechDataSystem.Instance.OnSystemDataLoaded -= OnSystemDataLoaded;
            SpeechDataSystem.Instance.OnSystemDataLoaded += OnSystemDataLoaded;

        }

        SpeechInputData GetInputDataForRecognisedSpeech(string speech)
        {
            SpeechInputData speechInputData = new();

            string[] words = speech.Split(" ");
            foreach(string word in words)
            {
                foreach(ESpeechInputType termKey in _speechInputTerm_LUT.Keys)
                {
                    string keyString = _speechInputTerm_LUT[termKey];
                    if (keyString == word)
                    {
                        speechInputData.InputQueue.Add(termKey);
                    }
                }
            }

            return speechInputData;
        }

        ESpeechInputType_Flag GetInputForRecognisedSpeech(string speech)
        {
            ESpeechInputType_Flag typeFlag = ESpeechInputType_Flag.None;

            string[] words = speech.Split(" ");
            foreach(string word in words)
            {
                foreach (ESpeechInputType termKey in _speechInputTerm_LUT.Keys)
                {
                    string keyString = _speechInputTerm_LUT[termKey];
                    if (keyString == word)
                    {
                        ESpeechInputType_Flag enumFlag = SpeechHelper.FlagFromEnum(termKey);
                        typeFlag = typeFlag | enumFlag;
                    }
                }
            }

            return typeFlag;
        }

        public string GetTermForType(ESpeechInputType type)
        {
            if(_speechInputTerm_LUT.TryGetValue(type, out string term))
            {
                return term;
            }

            Debug.LogWarning(this.name + " - failed to find term for type " + type.ToString() + " - returning empty string");
            return string.Empty;
        }

        public void SetTermForType(ESpeechInputType type, string newTerm)
        {
            Dictionary<ESpeechInputType, string> _speechInputTerm_LUT_Cache = new(_speechInputTerm_LUT);

            if(_speechInputTerm_LUT.ContainsKey(type))
            {
                _speechInputTerm_LUT[type] = newTerm;

                Debug.Log(this.name + " - set new term - " + newTerm + " - for input type - " + type.ToString());

                return;
            }

            Debug.LogWarning(this.name + " - failed to set new term - " + newTerm + " - for type " + type.ToString());
        }

        public void ResetTermsToDefault()
        {
            _speechInputTerm_LUT.Clear();

            foreach (ESpeechInputType type in Enum.GetValues(typeof(ESpeechInputType)))
            {
                if(!SpeechHelper.DEFAULT_TERMS.ContainsKey(type))
                {
                    continue;
                }

                string defaultTerm = SpeechHelper.DEFAULT_TERMS[type];
                _speechInputTerm_LUT.Add(type, defaultTerm);
            }
        }

        void UpdateInputForLatestSpeechData()
        {
            if(null == _latestSpeechInputData)
            {
                return;
            }

            if (_latestSpeechInputData.InputQueue.Count == 0)
            {
                return;
            }

            bool hasPressed = false;

            if(!hasPressed)
            {
                _latestSpeechInputData.TimeToNextPress -= Time.deltaTime;
                if(_latestSpeechInputData.TimeToNextPress <= 0)
                {
                    _latestSpeechInputData.TimeToNextPress = SPEECH_INPUT_DELAY;
                    ESpeechInputType type = _latestSpeechInputData.InputQueue[0];

                    switch (type)
                    {
                        case ESpeechInputType.Game_DPadUp:
                            DPadUpPressed = true;
                            hasPressed = true;
                            break;
                        case ESpeechInputType.Game_DPadDown:
                            DPadDownPressed = true;
                            hasPressed = true;
                            break;
                        case ESpeechInputType.Game_DPadLeft:
                            DPadLeftPressed = true;
                            hasPressed = true;
                            break;
                        case ESpeechInputType.Game_DPadRight:
                            DPadRightPressed = true;
                            hasPressed = true;
                            break;
                        case ESpeechInputType.Game_FaceNorth:
                            NorthFacePressed = true;
                            hasPressed = true;
                            break;
                        case ESpeechInputType.Game_FaceSouth:
                            SouthFacePressed = true;
                            hasPressed = true;
                            break;
                        case ESpeechInputType.Game_FaceWest:
                            WestFacePressed = true;
                            hasPressed = true;
                            break;
                        case ESpeechInputType.Game_FaceEast:
                            EastFacePressed = true;
                            hasPressed = true;
                            break;
                        case ESpeechInputType.UI_Confirm:
                            UIConfirmPressed = true;
                            hasPressed = true;
                            break;
                        case ESpeechInputType.UI_Back:
                            UIBackPressed = true;
                            hasPressed = true;
                            break;
                        case ESpeechInputType.Game_LeftShoulder:
                            LeftShoulderPressed = true;
                            hasPressed = true;
                            break;
                        case ESpeechInputType.Game_RightShoulder:
                            RightShoulderPressed = true;
                            hasPressed = true;
                            break;
                        case ESpeechInputType.Game_LeftTrigger:
                            LeftTriggerPressed = true;
                            hasPressed = true;
                            break;
                        case ESpeechInputType.Game_RightTrigger:
                            RightTriggerPressed = true;
                            hasPressed = true;
                            break;
                    }

                    _latestSpeechInputData.InputQueue.RemoveAt(0);
                }
            }
        }

        public void OnSpeechRecognized(string speechTerm)
        {            
            if(!VoiceControlEnabled)
            {
                Debug.Log(this.name + " - recognised term " + speechTerm + " , but voice control is disabled - aborting input");
                return;
            }

            string processedTerm = SpeechHelper.ProcessSpeechTerm(speechTerm);

            ESpeechInputType_Flag inputType = GetInputForRecognisedSpeech(processedTerm);
            SpeechInputData speechInputData = GetInputDataForRecognisedSpeech(processedTerm);
            _latestSpeechInputData = speechInputData;

            DebugLatestInputFlag = inputType;
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

            LeftShoulderPressed = false;
            RightShoulderPressed = false;
            LeftTriggerPressed = false;
            RightShoulderPressed = false;

            UIConfirmPressed = false;
            UIBackPressed = false;
        }

        public void OnSystemDataLoaded(object loadedData)
        {
            SpeechSaveData saveData = (SpeechSaveData)loadedData;

            SpeechRecognitionSystem.Instance.SetRegionString(saveData.LanguageCode, (ESpeechRegion)saveData.LanguageCodeIndex);
            //ControlSettingsUISystem.Instance.SetSavedSpeechString(saveData.LanguageCode);

            if(saveData.Terms.Count == 0)
            {
                Debug.LogError(this.name + " - has recieved save data with 0 terms");
                return;
            }

            _speechInputTerm_LUT.Clear();

            foreach(SpeechSaveData.SpeechSaveDataTerm term in saveData.Terms)
            {
                if(_speechInputTerm_LUT.ContainsKey(term.Type))
                {
                    Debug.LogError(this.name + " - has recieved duplicate term for type " + term.Type.ToString() + " - skipping addition for Term" + term.Term);
                    continue;
                }

                _speechInputTerm_LUT.Add(term.Type, term.Term);
                Debug.Log(this.name + " - adding term " + term.Term + " - for input type " + term.Type.ToString());
            }
        }

        public void SaveCurrentSpeechData()
        {
            SpeechSaveData saveData = new();

            saveData.Terms = new();

            foreach(ESpeechInputType termKey in _speechInputTerm_LUT.Keys)
            {
                SpeechSaveData.SpeechSaveDataTerm term = new()
                {
                    Term = _speechInputTerm_LUT[termKey],
                    Type = termKey,
                };

                saveData.Terms.Add(term);
                saveData.LanguageCode = SpeechRecognitionSystem.Instance.CurrentLanguageString;
                saveData.LanguageCodeIndex = (int)SpeechRecognitionSystem.Instance.CurrentRegion;
            }

            SpeechDataSystem.Instance.SaveGameData(saveData);
        }

        public void ToggleVoiceInputEnabled()
        {
            VoiceControlEnabled = !VoiceControlEnabled;
        }
    }
}