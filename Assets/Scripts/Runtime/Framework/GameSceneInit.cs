using JZK.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Framework
{
	public class GameSceneInit : SceneInit
	{

		ISystemReference<MonoBehaviour>[] _systems = new ISystemReference<MonoBehaviour>[]
		{
			new SystemReference<Gameplay.GameplaySystem>(),

			new SystemReference<Input.InputSystem>(),
			new SystemReference<Input.SpeechInputSystem>(),
			new SystemReference<Input.SpeechRecognitionSystem>(),

			new SystemReference<UI.ControlSettingsUISystem>(),
			new SystemReference<UI.MainMenuUISystem>(),
            new SystemReference<UI.UIStateSystem>(),



        };

		public void Start()
		{
			Setup(_systems);

			SpeechRecognitionSystem.Instance.StartContinuousRecognition();

		}

		private void Update()
		{
			UpdateScene();
		}


#if UNITY_EDITOR
		public void OnApplicationQuit()
		{
			
		}
#endif
	}
}