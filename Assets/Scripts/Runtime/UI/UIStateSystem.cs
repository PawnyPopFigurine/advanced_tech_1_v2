using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;
using JZK.Input;

namespace JZK.UI
{
	public class UIStateSystem : GameSystem<UIStateSystem>
	{

		#region PersistentSystem

		public SystemLoadData _loadData = new SystemLoadData()
		{
			LoadStates = SystemLoadState.NoLoadingNeeded,
			UpdateAfterLoadingState = ELoadingState.FrontEnd
		};

		public override SystemLoadData LoadData => _loadData;

		public override void Initialise()
		{
			base.Initialise();

			for(int uiIndex = 0; uiIndex < (int)EUIState.Max; uiIndex++)
			{
				EUIState state = (EUIState)uiIndex;
				IUISystem system = GetSystemForUIState(state);

				if(system == null)
				{
					continue;
				}

				system.SetActive(false);
			}


		}

		public override void UpdateSystem()
		{
			base.UpdateSystem();

			if(!HasLoadedAll())
			{
				return;
			}

			if(_quitTriggered)
			{
				if(!SpeechRecognitionSystem.Instance.IsRecording)
				{
					_canQuitSafely = true;
					QuitToDesktop();
				}
			}

			UpdateState();
		}

		public override void LateUpdateSystem()
		{
			base.LateUpdateSystem();

			LateUpdateState();
		}

		public override void SetCallbacks()
		{
			base.SetCallbacks();

			SceneInit.CurrentSceneInit.OnLoadingStateComplete -= OnLoadingStateComplete;
			SceneInit.CurrentSceneInit.OnLoadingStateComplete += OnLoadingStateComplete;
		}

		#endregion

		public enum EUIState
		{
			None,
			MainMenu,
			Gameplay,
			ControlSettings,

			Max
		}

		[SerializeField] EUIState _previousState = EUIState.None;
		[SerializeField] EUIState _currentState = EUIState.None;

		public EUIState PreviousState => _previousState;
		public EUIState CurrentState => _currentState;

		[SerializeField] IUISystem _activeUISystem;

		[SerializeField] bool _hasActiveUI;

		bool _quitTriggered;
		public bool QuitTriggered => _quitTriggered;

		bool _canQuitSafely;

		private void UpdateState()
		{
			if(_hasActiveUI)
			{
				_activeUISystem.UpdateSystem();
			}

			DebugOverlayUISystem.Instance.UpdateSystem();
		}

		private void LateUpdateState()
		{
			if(_hasActiveUI)
			{
				_activeUISystem.LateUpdateSystem();
			}

            DebugOverlayUISystem.Instance.LateUpdateSystem();
        }

		IUISystem GetSystemForUIState(EUIState state)
		{
			switch(state)
			{
				case EUIState.MainMenu:
					return MainMenuUISystem.Instance;
				case EUIState.ControlSettings:
					return ControlSettingsUISystem.Instance;
				case EUIState.Gameplay:
					return GameplayUISystem.Instance;
				default:
					Debug.LogWarning("[UI] No UI System found for UI State " + state.ToString());
					return null;
			}
		}

		public void TransitionToState(EUIState state)
		{
			if(state == _currentState)
			{
				return;
			}

			//Must clear InputSystem here
			Input.InputSystem.Instance.Clear();

			_previousState = _currentState;
			_currentState = state;

			bool hasActiveUIBeforeTransition = _activeUISystem != null;

			if(hasActiveUIBeforeTransition)
            {
				_activeUISystem.SetActive(false);
            }

			_activeUISystem = GetSystemForUIState(state);
			_hasActiveUI = _activeUISystem != null;

			if(_hasActiveUI)
			{
				_activeUISystem.SetActive(true);
			}

			DebugOverlayUISystem.Instance.SetActive(true);
		}

		public void HideAllUI()
		{
			EnterScreen(EUIState.None);
		}

		public void EnterScreen(EUIState state)
		{
			//functionality for changing audio goes here

			TransitionToState(state);
		}

		public void EnterPreviousScreen()
		{
			EnterScreen(_previousState);
		}

		public void TriggerQuit()
		{
			_quitTriggered = true;
			SpeechRecognitionSystem.Instance.OnQuitTriggered();
		}

		private void QuitToDesktop()
		{
			if(!_canQuitSafely)
			{
				Debug.Log(this.name + " - tried to trigger quit when not safe - aborting action");
				return;
			}
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
			Application.Quit();
#endif
        }

        private EUIState _pauseLastState;

		/*public void Pause()
		{
			if(_currentState == EUIState.LevelPause)
			{
				return;
			}

			_pauseLastState = CurrentState;
			EnterScreen(EUIState.LevelPause);
		}

		public void UnPause()
		{
			EnterScreen(_pauseLastState);
			_pauseLastState = EUIState.None;
		}*/

		#region Callbacks

		private void OnLoadingStateComplete(ELoadingState state)
		{
			if(state != ELoadingState.FrontEnd)
			{
				return;
			}

			if(SceneInit.IsTestScene)
			{
				return;
			}

			EnterScreen(EUIState.MainMenu);
		}

		#endregion //Callbacks



	}
}