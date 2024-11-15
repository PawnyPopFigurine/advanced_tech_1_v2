using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using JetBrains.Annotations;
using JZK.Input;

namespace JZK.UI
{
    public class ControlSettingsUISystem : GameSystem<ControlSettingsUISystem>, IUISystem
    {
        #region IUISystem

        public UIController Controller => _controller;

        public void SetActive(bool active)
        {
            if (!_hasLoaded)
            {
                return;
            }

            _controller.SetActive(active);
            _active = active;
        }

        #endregion //IUISystem

        private ControlSettingsUIController _controller;
        bool _active = false;
        public bool Active => _active;

        private bool _hasLoaded;

        #region PersistentSystem

        private SystemLoadData _loadData = new SystemLoadData()
        {
            LoadStates = new SystemLoadState[] { new SystemLoadState { LoadStartState = ELoadingState.FrontEnd, BlockStateUntilFinished = ELoadingState.FrontEnd } },
            UpdateAfterLoadingState = ELoadingState.None,   //handled manually by IUISystem
        };

        public override SystemLoadData LoadData => _loadData;

        public override void StartLoading(ELoadingState state)
        {
            LoadUIPrefab();
            base.StartLoading(state);
        }

        public override void UpdateSystem()
        {
            base.UpdateSystem();
            if (_active)
            {
                _controller.UpdateController();

                if(SpeechRecognitionSystem.Instance.RecordedThisFrame)
                {
                    _controller.OnSpeechRecognised(SpeechRecognitionSystem.Instance.LatestRecordedSpeech);
                }
            }
        }

        public override void SetCallbacks()
        {


        }

        #endregion //PersistentSystem

        #region Load

        void LoadUIPrefab()
        {
            Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/UI/ControlSettings/ControlSettingsUI.prefab").Completed += LoadCompleted;
        }

        void LoadCompleted(AsyncOperationHandle<GameObject> op)
        {
            if (op.Result == null)
            {
                Debug.LogError(this.GetType().ToString() + " - failed to load addressable.");
                return;
            }

            _initPerfMarker.Begin(this);
            float startTime = Time.realtimeSinceStartup;

            GameObject uiRoot = Instantiate(op.Result);
            uiRoot.transform.SetParent(transform);
            uiRoot.transform.position = Vector3.zero;
            uiRoot.transform.rotation = Quaternion.identity;
            uiRoot.transform.localScale = Vector3.one;
            _controller = uiRoot.GetComponent<ControlSettingsUIController>();
            _controller.Initialise();
            _controller.SetActive(false);

            _hasLoaded = true;
            FinishLoading(ELoadingState.FrontEnd);

            float endTime = Time.realtimeSinceStartup - startTime;
            _initPerfMarker.End();
            Debug.Log("INIT: " + GetType() + " LoadCompleted " + endTime.ToString("F2") + " seconds.)");
        }

        #endregion //Load

        public void Input_BackButtonPressed()
        {
            UIStateSystem.Instance.EnterPreviousScreen();
        }

        public void Input_RecordButtonPressed(ESpeechInputType type)
        {
            _controller.Input_RecordButtonPressed(type);
        }

        public void Input_ConfirmButtonPressed()
        {

        }

        public void Input_CancelButtonPressed()
        {

        }

        public void OnSpeechRecognised(string speech)
        {
            _controller.OnSpeechRecognised(speech);
        }

        /*public void SetSavedSpeechString(string speechString)
        {
            //_controller.SetSavedSpeechString(speechString);
        }*/
    }
}