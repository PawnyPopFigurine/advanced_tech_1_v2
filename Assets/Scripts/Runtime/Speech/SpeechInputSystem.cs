using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;
using System.Text.RegularExpressions;
using JZK.Save;
using System;
using UnityEngine.UIElements;
using System.Linq;

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

    public class SpeechInputData
    {
        public Dictionary<ESpeechInputType, int> Input_LUT = new();
        public float TimeToNextPress;
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
                    UIBackPressed);
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
                        if(speechInputData.Input_LUT.ContainsKey(termKey))
                        {
                            speechInputData.Input_LUT[termKey] += 1;
                        }
                        else
                        {
                            speechInputData.Input_LUT.Add(termKey, 1);
                        }
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

            Dictionary<ESpeechInputType, int> inputLUT_Cache = new(_latestSpeechInputData.Input_LUT);

            bool hasPressed = false;

            foreach (ESpeechInputType type in inputLUT_Cache.Keys)
            {
                if(hasPressed)
                {
                    continue;
                }
                _latestSpeechInputData.TimeToNextPress -= Time.deltaTime;
                if (_latestSpeechInputData.TimeToNextPress <= 0)
                {
                    _latestSpeechInputData.TimeToNextPress = SPEECH_INPUT_DELAY;
                    switch(type)
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
                    }

                    _latestSpeechInputData.Input_LUT[type] -= 1;
                    if (_latestSpeechInputData.Input_LUT[type] == 0)
                    {
                        _latestSpeechInputData.Input_LUT.Remove(type);
                    }
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

            UIConfirmPressed = false;
            UIBackPressed = false;
        }

        public void OnSystemDataLoaded(object loadedData)
        {
            SpeechSaveData saveData = (SpeechSaveData)loadedData;

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

        public void SaveCurrentTerms()
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
            }

            SpeechDataSystem.Instance.SaveGameData(saveData);
        }

        public void ToggleVoiceInputEnabled()
        {
            VoiceControlEnabled = !VoiceControlEnabled;
        }
    }
}