using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using JZK.Serialization;

namespace JZK.Save
{
    public enum EIOState
    {
        Idle,
        SaveInProgress,
        LoadInProgress
    }


    public class SaveSystemModule
    {
        public delegate string GetFilePathFunction();
        [SerializeField] private EIOState _currentState;
        private static bool _isAnyIOOperationInProgress = false;
        public bool IsAnyIOOperationInProgress => _isAnyIOOperationInProgress;
        public const string FILE_EXTENSION = "jzk";

        #region Save

        public async Task<bool> WriteSaveFileAsync(object data, string filePath)
        {
            if (_isAnyIOOperationInProgress)
            {
                return false;
            }

            _currentState = EIOState.SaveInProgress;

            await SerializationManager.ProcessWriteAsync(data, filePath);

            Debug.Log("Save file written to " + filePath);
            _currentState = EIOState.Idle;

            Debug.Log("Save removed from pending");

            return true;
        }

        #endregion //Save

        #region Load

        [System.Serializable]
        public struct LoadResult
        {
            public bool Success;
            public object LoadedData;
        }

        public async Task<LoadResult> LoadGameAsync(GetFilePathFunction getPathFunction, GetFilePathFunction getBackupPathFunction)
        {
            LoadResult result = new LoadResult { Success = false, LoadedData = null };

            if (_isAnyIOOperationInProgress)
            {
                return result;
            }

            string filePath = getPathFunction.Invoke();

            _currentState = EIOState.LoadInProgress;

            SerializationManager.SerializationResult serializationResult = await SerializationManager.ProcessReadAsync(filePath);

            _currentState = EIOState.Idle;

            object saveData;

            if (serializationResult.ResultType != SerializationManager.EResultType.Success)
            {
                LoadResult backupLoadResult = await LoadBackupGameAsync(getBackupPathFunction);

                if (!backupLoadResult.Success)
                {
                    return result;
                }

                saveData = backupLoadResult.LoadedData;
            }

            else
            {
                saveData = serializationResult.Object;
            }

            result.Success = true;
            result.LoadedData = saveData;

            return result;
        }

        private async Task<LoadResult> LoadBackupGameAsync(GetFilePathFunction getBackupFilePath)
        {
            LoadResult result = new LoadResult { Success = false, LoadedData = null };

            if (_isAnyIOOperationInProgress)
            {
                return result;
            }

            string filePath = getBackupFilePath.Invoke();

            _currentState = EIOState.LoadInProgress;

            SerializationManager.SerializationResult serializationResult = await SerializationManager.ProcessReadAsync(filePath);

            _currentState = EIOState.Idle;

            if (serializationResult.ResultType == SerializationManager.EResultType.Success)
            {
                Debug.Log("Save module - backup load successful");
                result.Success = true;
                result.LoadedData = serializationResult.Object;
            }
            else
            {
                Debug.Log("Save module - backup load failed");
            }

            return result;

        }

        #endregion //Load
    }
}