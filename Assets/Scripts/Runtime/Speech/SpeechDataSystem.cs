using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using JZK.Framework;
using System;
using Unity.VisualScripting;
using JZK.Save;
using JetBrains.Annotations;


namespace JZK.Input
{
    [Serializable]
    public class SpeechSaveData
    {
        [Serializable]
        public class SpeechSaveDataTerm
        {
            public ESpeechInputType Type;
            public string Term;
        }

        public List<SpeechSaveDataTerm> Terms = new();

    }

    public class SpeechDataSystem : GameSystem<SpeechDataSystem>
    {
        private SystemLoadData _loadData = new SystemLoadData()
        {
            LoadStates = new SystemLoadState[] { new SystemLoadState { LoadStartState = ELoadingState.FrontEndData, BlockStateUntilFinished = ELoadingState.FrontEndData } },
            UpdateAfterLoadingState = ELoadingState.FrontEndData,
        };

        public override SystemLoadData LoadData
        {
            get { return _loadData; }
        }

        private SpeechSaveData _savedSpeechData;

        public delegate void DataLoadedEvent(object loadedData);
        public DataLoadedEvent OnSystemDataLoaded;

        private SaveSystemModule _saveModule = new SaveSystemModule();

        private bool _hasSavedGame = false;

        public override void StartLoading(ELoadingState state)
        {
            base.StartLoading(state);

            _ = InitialLoad();
        }

        private async Task InitialLoad()
        {
            await LoadGameFile();

            if (_hasSavedGame)
            {
                OnSystemDataLoaded?.Invoke(_savedSpeechData);
            }
            else
            {
                _savedSpeechData = CreateDefaultSaveData();
                OnSystemDataLoaded?.Invoke(_savedSpeechData);
            }

            FinishLoading(ELoadingState.FrontEndData);
        }

        #region Load

        private async Task LoadGameFile()
        {
            SaveSystemModule.LoadResult loadResult = await _saveModule.LoadGameAsync(
                delegate { return GetMainFilePath(); },
                delegate { return GetBackupFilePath(); }
                );

            if (loadResult.Success)
            {
                SpeechSaveData saveData = (SpeechSaveData)loadResult.LoadedData;

                //update save data for different versions here

                _hasSavedGame = true;
                _savedSpeechData = saveData;
                await WriteSaveFileAsync(saveData, true);
            }
        }

        private string GetMainFilePath()
        {
            string fileName = GetMainFileName();

            //TODO: include steam Id when steam package instealled
            string filePath = Serialization.SerializationManager.GetPersistentSaveDataPath() + "/" + GetUserIdString() + "/" + fileName;
            return filePath;
        }

        private string GetMainFileName()
        {
            string fileName = "GameSaveData";
            return $"{fileName}.{SaveSystemModule.FILE_EXTENSION}";
        }

        private string GetBackupFilePath()
        {
            string fileName = GetBackupFileName();

            string filePath = Serialization.SerializationManager.GetPersistentSaveDataPath() + "/" + GetUserIdString() + "/" + fileName;
            return filePath;
        }

        private string GetBackupFileName()
        {
            string fileName = "GameSaveData_Backup";
            return $"{fileName}.{SaveSystemModule.FILE_EXTENSION}";
        }

        private string GetUserIdString()
        {
#if UNITY_EDITOR
            return "EDITOR";
#endif
            return "Steam_ID_Here";
        }

        public SpeechSaveData CreateDefaultSaveData()
        {
            return new()
            {
                Terms = new()
                {
                    new(){Type = ESpeechInputType.Game_DPadUp, Term = "up"},
                    new(){Type = ESpeechInputType.Game_DPadDown, Term = "down"},
                    new(){Type = ESpeechInputType.Game_DPadLeft, Term = "left"},
                    new(){Type = ESpeechInputType.Game_DPadRight, Term = "right"},

                    new(){Type = ESpeechInputType.Game_FaceEast, Term = "east"},
                    new(){Type = ESpeechInputType.Game_FaceWest, Term = "west"},
                    new(){Type = ESpeechInputType.Game_FaceNorth, Term = "north"},
                    new(){Type = ESpeechInputType.Game_FaceSouth, Term = "south"},
                }
            };
        }


        #endregion //Load

        #region Save

        private SpeechSaveData _pendingBackupSaveData;
        private SpeechSaveData _pendingSaveData;

        public delegate void SaveSystemEvent();

        public event SaveSystemEvent OnDataCollectionStarted;


        public event SaveSystemEvent OnGameResumeReady;

        public event SaveSystemEvent OnSystemSaveComplete;

        private EIOState _currentState;

        public void SaveGameData(SpeechSaveData saveData)
        {
            _ = WriteSaveFileAsync(saveData, false);
        }

        public async Task<bool> WriteSaveFileAsync(SpeechSaveData data, bool saveAsBackup)
        {
            string filePath = saveAsBackup ? GetBackupFilePath() : GetMainFilePath();

            _currentState = EIOState.SaveInProgress;

            await Serialization.SerializationManager.ProcessWriteAsync(data, filePath);

            Debug.Log("Game save file written to " + filePath);

            _currentState = EIOState.Idle;

            if (saveAsBackup)
            {
                _pendingBackupSaveData = null;
            }
            else
            {
                _pendingSaveData = null;
            }

            Debug.Log("Game save removed from pending");

            return true;
        }

        private bool _isRecordingSaveData = false;

        private SpeechSaveData _recordingSaveData = null;

        public void SubmitSystemSaveData(SpeechSaveData saveData)
        {
            if (!_isRecordingSaveData)
            {
                return;
            }

            Debug.Log("Save data submitted for options data");

            _recordingSaveData = saveData;
        }

        #endregion //Save
    }
}