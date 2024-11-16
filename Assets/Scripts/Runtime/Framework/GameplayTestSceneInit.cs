using JZK.Gameplay;
using JZK.Input;
using JZK.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Framework
{
    public class GameplayTestSceneInit : SceneInit
    {

        bool _hasPerformedLoadCompleteAction = false;

        ISystemReference<MonoBehaviour>[] _systems = new ISystemReference<MonoBehaviour>[]
        {
            new SystemReference<Gameplay.GameplaySystem>(),

            new SystemReference<Input.InputSystem>(),
            new SystemReference<Input.OptionsDataSystem>(),
            new SystemReference<Input.SpeechInputSystem>(),
            new SystemReference<Input.SpeechRecognitionSystem>(),

            new SystemReference<UI.ControlSettingsUISystem>(),
            new SystemReference<UI.DebugOverlayUISystem>(),
            new SystemReference<UI.MainMenuUISystem>(),
            new SystemReference<UI.UIStateSystem>(),



        };

        public void Start()
        {
            _isTestScene = true;

            Setup(_systems);
        }

        private void Update()
        {
            UpdateScene();

            if(CurrentLoadingState == ELoadingState.Finished)
            {
                OnAllLoadingComplete();
            }
        }

        void OnAllLoadingComplete()
        {
            if(_hasPerformedLoadCompleteAction)
            {
                return;
            }

            SpeechRecognitionSystem.Instance.StartContinuousRecognition();
            UIStateSystem.Instance.EnterScreen(UIStateSystem.EUIState.Gameplay);
            GameplaySystem.Instance.StartGameplay();

            _hasPerformedLoadCompleteAction = true;
        }

        


#if UNITY_EDITOR
        public void OnApplicationQuit()
        {

        }
#endif
    }
}