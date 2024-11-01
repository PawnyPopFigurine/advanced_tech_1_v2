using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;
using System.Text.RegularExpressions;
using JZK.Save;
using System;
using UnityEngine.UIElements;

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

    //Takes in recognised speech and matches it to game input.
    public class SpeechInputSystem : GameSystem<SpeechInputSystem>
    {
        public SystemLoadData _loadData = new SystemLoadData()
        {
            LoadStates = SystemLoadState.NoLoadingNeeded,
            UpdateAfterLoadingState = ELoadingState.Game,
        };

        public override SystemLoadData LoadData => _loadData;

        Dictionary<string, ESpeechInputType> _speechTermInput_LUT = new();
        public Dictionary<string, ESpeechInputType> SpeechTermInput_LUT => _speechTermInput_LUT;

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
        }

        public override void SetCallbacks()
        {
            base.SetCallbacks();

            SpeechDataSystem.Instance.OnSystemDataLoaded -= OnSystemDataLoaded;
            SpeechDataSystem.Instance.OnSystemDataLoaded += OnSystemDataLoaded;

        }

        ESpeechInputType_Flag GetInputForRecognisedSpeech(string speech)
        {
            ESpeechInputType_Flag typeFlag = ESpeechInputType_Flag.None;

            foreach(string keyString in _speechTermInput_LUT.Keys)
            {
                if(speech.Contains(keyString))
                {
                    ESpeechInputType_Flag enumFlag = SpeechHelper.FlagFromEnum(_speechTermInput_LUT[keyString]);
                    typeFlag = typeFlag | enumFlag;
                }
            }

            return typeFlag;
        }

        public string GetTermForType(ESpeechInputType type)
        {
            foreach(string keyString in _speechTermInput_LUT.Keys)
            {
                ESpeechInputType typeValue = _speechTermInput_LUT[keyString];
                if(typeValue != type)
                {
                    continue;
                }

                return keyString;
            }

            Debug.LogWarning(this.name + " - failed to find term for type " + type.ToString() + " - returning empty string");
            return string.Empty;
        }

        public void SetTermForType(ESpeechInputType type, string newTerm)
        {
            Dictionary<string, ESpeechInputType> _speechTermInput_LUT_Cache = new(_speechTermInput_LUT);

            foreach (string keyString in _speechTermInput_LUT_Cache.Keys)
            {
                ESpeechInputType typeValue = _speechTermInput_LUT_Cache[keyString];
                if (typeValue != type)
                {
                    continue;
                }

                _speechTermInput_LUT.Remove(keyString);
                _speechTermInput_LUT.Add(newTerm, type);

                Debug.Log(this.name + " - set new term - " + newTerm + " - for input type - " + type.ToString());

                return;
            }

            Debug.LogWarning(this.name + " - failed to set new term - " + newTerm + " - for type " + type.ToString());
        }

        public void ResetTermsToDefault()
        {
            _speechTermInput_LUT.Clear();

            foreach (ESpeechInputType type in Enum.GetValues(typeof(ESpeechInputType)))
            {
                if(!SpeechHelper.DEFAULT_TERMS.ContainsKey(type))
                {
                    continue;
                }

                string defaultTerm = SpeechHelper.DEFAULT_TERMS[type];
                _speechTermInput_LUT.Add(defaultTerm, type);
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

            if(inputType.HasFlag(ESpeechInputType_Flag.Game_FaceNorth))
            {
                NorthFacePressed = true;
            }

            if(inputType.HasFlag(ESpeechInputType_Flag.Game_FaceSouth))
            {
                SouthFacePressed = true;
            }

            if(inputType.HasFlag(ESpeechInputType_Flag.Game_FaceWest))
            {
                WestFacePressed = true;
            }

            if(inputType.HasFlag(ESpeechInputType_Flag.Game_FaceEast))
            {
                EastFacePressed = true;
            }

            if(inputType.HasFlag(ESpeechInputType_Flag.Game_DPadUp))
            {
                DPadUpPressed = true;
            }

            if(inputType.HasFlag(ESpeechInputType_Flag.Game_DPadDown))
            {
                DPadDownPressed = true;
            }

            if(inputType.HasFlag(ESpeechInputType_Flag.Game_DPadLeft))
            {
                DPadLeftPressed = true;
            }

            if(inputType.HasFlag(ESpeechInputType_Flag.Game_DPadRight))
            {
                DPadRightPressed = true;
            }

            if(inputType.HasFlag(ESpeechInputType_Flag.UI_Confirm))
            {
                UIConfirmPressed = true;
            }

            if(inputType.HasFlag(ESpeechInputType_Flag.UI_Back))
            {
                UIBackPressed = true;
            }

            DebugLatestInputFlag = inputType;
            //ESpeechInputType inputType = GetInputForRecognisedSpeech(processedTerm);

            /*switch(inputType)
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
                case ESpeechInputType.UI_Confirm:
                    UIConfirmPressed = true;
                    break;
                case ESpeechInputType.UI_Back:
                    UIBackPressed = true;
                    break;
                default:
                    Debug.Log(this.name + " - no recognised input type for speech " + processedTerm);
                    break;
            }

            DebugLatestInput = inputType;*/
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

            _speechTermInput_LUT.Clear();

            foreach(SpeechSaveData.SpeechSaveDataTerm term in saveData.Terms)
            {
                if(_speechTermInput_LUT.ContainsValue(term.Type))
                {
                    Debug.LogError(this.name + " - has recieved duplicate term for type " + term.Type.ToString() + " - skipping addition for Term" + term.Term);
                    continue;
                }

                _speechTermInput_LUT.Add(term.Term, term.Type);
                Debug.Log(this.name + " - adding term " + term.Term + " - for input type " + term.Type.ToString());
            }
        }

        public void SaveCurrentTerms()
        {
            SpeechSaveData saveData = new();

            saveData.Terms = new();

            foreach(string termString in _speechTermInput_LUT.Keys)
            {
                SpeechSaveData.SpeechSaveDataTerm term = new()
                {
                    Term = termString,
                    Type = _speechTermInput_LUT[termString]
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