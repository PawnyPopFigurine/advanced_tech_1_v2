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

        UI_Up,
        UI_Down,
        UI_Left,
        UI_Right,

        UI_Confirm,
        UI_Back,

        Max,
        Invalid
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
            string processedTerm = SpeechHelper.ProcessSpeechTerm(speechTerm);

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

            DebugLatestInput = inputType;
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
    }
}