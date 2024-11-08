using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;
using JZK.Gameplay;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace JZK.UI
{
    public class GameplayUISystem : GameSystem<GameplayUISystem>, IUISystem
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

        private GameplayUIController _controller;
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
            }
        }

        #endregion //PersistentSystem

        #region Load

        void LoadUIPrefab()
        {
            Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/UI/Gameplay/GameplayUI.prefab").Completed += LoadCompleted;
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
            _controller = uiRoot.GetComponent<GameplayUIController>();
            _controller.Initialise();
            _controller.SetActive(false);

            _hasLoaded = true;
            FinishLoading(ELoadingState.FrontEnd);

            float endTime = Time.realtimeSinceStartup - startTime;
            _initPerfMarker.End();
            Debug.Log("INIT: " + GetType() + " LoadCompleted " + endTime.ToString("F2") + " seconds.)");
        }

        #endregion //Load

        public void OnGameplayStart()
        {
            _controller.OnGameplayStart();
        }

        public void OnPlayerDeath()
        {
            _controller.OnPlayerDeath();
        }

        public void Input_RestartButtonPressed()
        {
            _controller.OnRestart();
            GameplaySystem.Instance.RestartGameplay();
        }

        public void Input_MainMenuButtonPressed()
        {
            _controller.OnMainMenu();
            GameplaySystem.Instance.DestroyAllHazardBlocks();
            UIStateSystem.Instance.EnterScreen(UIStateSystem.EUIState.MainMenu);
        }

        public void Input_QuitButtonPressed()
        {
            UIStateSystem.Instance.TriggerQuit();
        }
    }
}