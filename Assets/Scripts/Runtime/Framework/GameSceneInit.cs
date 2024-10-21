using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Framework
{
	public class GameSceneInit : SceneInit
	{

		ISystemReference<MonoBehaviour>[] _systems = new ISystemReference<MonoBehaviour>[]
		{
			/*new SystemReference<Gameplay.TestPersistentSystem>(),

			new SystemReference<HazardArt.HazardArtLoadSystem>(),

			new SystemReference<HazardInfo.HazardInfoLoadSystem>(),*/

			new SystemReference<Input.InputSystem>(),


            new SystemReference<UI.UIStateSystem>(),

			/*new SystemReference<Levels.LevelLoadSystem>(),
			new SystemReference<Levels.LevelSystem>(),
			new SystemReference<Levels.ProjectileSystem>(),

			new SystemReference<Player.PlayerSystem>(),

			new SystemReference<Save.GameSaveSystem>(),

			new SystemReference<Session.SessionSystem>(),

			new SystemReference<UI.BestiaryUISystem>(),
			new SystemReference<UI.LevelCompleteUISystem>(),
			new SystemReference<UI.LevelFailureUISystem>(),
			new SystemReference<UI.LevelPauseUISystem>(),
			new SystemReference<UI.LevelUISystem>(),
			new SystemReference<UI.LevelSelectUISystem>(),
			new SystemReference<UI.MainMenuUISystem>(),
			new SystemReference<UI.PressStartUISystem>(),
			new SystemReference<UI.TestLevelSelectUISystem>(),
			,*/

		};

		[SerializeField] private bool _useDebugSeed = false;
		[SerializeField] private int _debugSeed;
		private int _currentSeed;
		public int CurrentSeed => _currentSeed;

		public void Start()
		{
			Setup(_systems);

			InitialiseSeed();
		}

		private void Update()
		{
			UpdateScene();
		}

		void InitialiseSeed()
		{
			_currentSeed = _useDebugSeed ? _debugSeed : System.DateTime.Now.Millisecond;
			UnityEngine.Random.InitState(_currentSeed);
		}

#if UNITY_EDITOR
		public void OnApplicationQuit()
		{
			
		}
#endif
	}
}