using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.InputSystem;
using JZK.Framework;

namespace JZK.Input
{
	public enum EControllerType
	{
		Mouse,
		Keyboard,
		Gamepad,

	}

	public enum EControllerPlatformType
	{
		None,
		Xbox,
		Playstation,
		Switch,

	}

	public class InputSystem : PersistentSystem<InputSystem>
	{
		private static readonly string[] INPUT_ACTION_MAP_IDS =
		{
			"Gameplay",
		};

		public delegate void ControllerChangeEvent(EControllerType previous, EControllerType current);
		public delegate void InputEvent();
		public event ControllerChangeEvent OnControllerTypeChanged;
		public event InputEvent OnControllerPlatformTypeChanged;
		public bool FaceButtonSouth { get; private set; }
		public bool FaceButtonSouthPressed { get; private set; }
		public bool FaceButtonNorth { get; private set; }
		public bool FaceButtonNorthPressed { get; private set;}
		public bool FaceButtonWest { get; private set; }
		public bool FaceButtonWestPressed { get; private set; }
		public bool FaceButtonEast { get; private set; }
		public bool FaceButtonEastPressed { get; set; }


		EControllerType _lastControllerType;
		public EControllerType LastControllerType
		{
			get { return _lastControllerType; }
			set
			{
				EControllerType prevType = _lastControllerType;
				bool change = _lastControllerType != value;
				_lastControllerType = value;
				if (change)
				{
					OnControllerTypeChanged?.Invoke(prevType, _lastControllerType);
				}
			}
		}

		EControllerPlatformType _lastControllerPlatformType;
		public EControllerPlatformType LastControllerPlatformType
		{
			get { return _lastControllerPlatformType; }
			set
			{
				bool change = _lastControllerPlatformType != value;
				_lastControllerPlatformType = value;
				if(change)
				{
					OnControllerPlatformTypeChanged?.Invoke();
				}
			}
		}

		public InputDevice LastInputDevice;

		private PlayerInput _playerInput;
		private int InputDelay = 0;

		#region PersistentSystem

		private SystemLoadData _loadData = new SystemLoadData()
		{
			LoadStates = new SystemLoadState[] { new SystemLoadState { LoadStartState = ELoadingState.FrontEndData, BlockStateUntilFinished = ELoadingState.FrontEndData } },
			UpdateAfterLoadingState = ELoadingState.FrontEndData,
		};

		public override SystemLoadData LoadData
		{
			get { return _loadData; }
		}

		public override void StartLoading(ELoadingState state)
		{
			base.StartLoading(state);

			Load();
		}

		public override void UpdateSystem()
		{
			base.UpdateSystem();

#if !UNITY_EDITOR
			if(!Application.isFocused)
			{
				Clear();
				return;
			}
#endif

			if (InputDelay > 0)
			{
				InputDelay -= 1;
				return;
			}

			FaceButtonSouth = false;
			FaceButtonSouthPressed = false;
			FaceButtonWest = false;
			FaceButtonWestPressed = false;
			FaceButtonEast = false;
			FaceButtonEastPressed = false;
			FaceButtonNorth = false;
			FaceButtonNorthPressed = false;

			EControllerPlatformType platformType = LastControllerPlatformType;
			LastControllerType = GetCurrentController(out platformType, out LastInputDevice);
			LastControllerPlatformType = platformType;

#if (UNITY_XBOXONE || UNITY_PS4 || UNITY_GAMECORE || UNITY_SWITCH) && !UNITY_EDITOR
			MousePosition = new Vector2(-9999, -9999);
			Cursor.lockState = CursorLockMode.Locked;
#else
			//MousePosition = Mouse.current.position.ReadValue();
#endif

			bool lastFaceButtonUp = FaceButtonNorth;
			FaceButtonNorth = inputAction_FaceButtonUp.triggered;
			if(FaceButtonNorth && !lastFaceButtonUp)
			{
				FaceButtonNorthPressed = true;
			}

			bool lastFaceButtonDown = FaceButtonSouth;
			FaceButtonSouth = inputAction_FaceButtonDown.triggered;
			if(FaceButtonSouth && !lastFaceButtonDown)
			{
                FaceButtonSouthPressed = true;
			}

			bool lastFaceButtonLeft = FaceButtonWest;
			FaceButtonWest = inputAction_FaceButtonLeft.triggered;
			if (FaceButtonWest && !lastFaceButtonLeft)
			{
                FaceButtonWestPressed = true;
			}

			bool lastFaceButtonRight = FaceButtonEast;
			FaceButtonEast = inputAction_FaceButtonRight.triggered;
			if(FaceButtonEast && !lastFaceButtonRight)
			{
                FaceButtonEastPressed = true;
			}
		}
		#endregion // PersistentSystem

		#region Load

		public void Load()
		{
			Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Input/PlayerInput.prefab").Completed += InputLoadCompleted;
		}

		void InputLoadCompleted(AsyncOperationHandle<GameObject> op)
		{
			if(op.Result == null)
			{
				Debug.LogError(this.GetType().ToString() + "- Failed to load addressable.");
				return;
			}

			_initPerfMarker.Begin(this);
			float startTime = Time.realtimeSinceStartup;

			_playerInput = PlayerInput.Instantiate(op.Result);

			SetUpActionMaps(_playerInput);
			SetUpActions();

			FinishLoading(ELoadingState.FrontEndData);

			float endTime = Time.realtimeSinceStartup - startTime;
			_initPerfMarker.End();
			Debug.Log("INIT: " + GetType() + ".InputLoadCompleted " + endTime.ToString("F2") + " sec.)");

		}

#endregion //Load

		void SetUpActionMaps(PlayerInput playerInput)
		{
#if UNITY_SWITCH
			//SetUpActionMapsForSwitch(playerInput)
			return;
#else
			foreach(string inputActionMapId in INPUT_ACTION_MAP_IDS)
			{
				playerInput.actions.FindActionMap(inputActionMapId).Enable();
			}
#endif  //UNITY_SWITCH
		}

		private InputAction inputAction_FaceButtonUp;
		private InputAction inputAction_FaceButtonDown;
		private InputAction inputAction_FaceButtonLeft;
		private InputAction inputAction_FaceButtonRight;

		private void SetUpActions()
		{
			inputAction_FaceButtonDown = _playerInput.actions["Gameplay/FaceButtonDown"];
            inputAction_FaceButtonUp = _playerInput.actions["Gameplay/FaceButtonUp"];
            inputAction_FaceButtonLeft = _playerInput.actions["Gameplay/FaceButtonLeft"];
            inputAction_FaceButtonRight = _playerInput.actions["Gameplay/FaceButtonRight"];
        }

		public void Clear()
		{
			FaceButtonWest = false;
			FaceButtonEast = false;
			FaceButtonNorth = false;
			FaceButtonSouth = false;

			FaceButtonWestPressed = false;
			FaceButtonEastPressed = false;
			FaceButtonNorthPressed = false;
			FaceButtonSouthPressed = false;
		}

		EControllerType GetCurrentController(out EControllerPlatformType lastPlatformType, out InputDevice lastDevice)
		{
			lastDevice = LastInputDevice;
			EControllerType mostRecentType = LastControllerType;

			InputDevice activeDevice = null;
			InputDevice activeNonGamepad = null;
			InputDevice activeGamepad = null;
			double lastUpdate = 0;
			lastPlatformType = LastControllerPlatformType;

			bool hasKeyboard = Keyboard.current != null;
			bool hasMouse = Mouse.current != null;
			bool hasGamepad = Gamepad.current != null;

			if(hasGamepad && Gamepad.current.lastUpdateTime > lastUpdate)
			{
				mostRecentType = EControllerType.Gamepad;
				activeDevice = Gamepad.current;
				activeGamepad = Gamepad.current;
				lastUpdate = activeDevice.lastUpdateTime;
			}

			if(hasKeyboard && Keyboard.current.lastUpdateTime > lastUpdate)
			{
				mostRecentType = EControllerType.Keyboard;
				activeDevice = Keyboard.current;
				activeGamepad = Keyboard.current;
				lastUpdate = activeDevice.lastUpdateTime;
			}

			if (hasMouse && Mouse.current.lastUpdateTime > lastUpdate)
			{
				mostRecentType = EControllerType.Mouse;
				activeDevice = Mouse.current;
				activeGamepad = Mouse.current;
				lastUpdate = activeDevice.lastUpdateTime;
			}

			if(activeDevice == lastDevice)
			{
				lastPlatformType = LastControllerPlatformType;
				return LastControllerType;
			}

			if(hasGamepad && Gamepad.current.lastUpdateTime == lastUpdate && IsGamepadInputAcceptable(Gamepad.current))
			{
				lastDevice = activeGamepad;
#if UNITY_SWITCH
				lastPlatformType = EControllerPlatformType.Switch;
#else
				if(Gamepad.current is UnityEngine.InputSystem.XInput.XInputController)
				{
					lastPlatformType = EControllerPlatformType.Xbox;
				}
				else if(Gamepad.current is UnityEngine.InputSystem.DualShock.DualShockGamepad)
				{
					lastPlatformType = EControllerPlatformType.Playstation;
				}
				else if (Gamepad.current is UnityEngine.InputSystem.Switch.SwitchProControllerHID)
				{
					lastPlatformType = EControllerPlatformType.Switch;
				}
#endif
				return EControllerType.Gamepad;
			}
			else
			{
				lastDevice = activeNonGamepad;
#if UNITY_GAMECORE
				lastPlatformType = EControllerPlatformType.Xbox;
#elif UNITY_SWITCH
				lastPlatformType =e EControllerPlatformType.Switch;
#else
				lastPlatformType = EControllerPlatformType.None;
#endif
				return mostRecentType;
			}

		}

		private bool IsGamepadInputAcceptable(Gamepad gamepad)
		{
			for(int controlIndex = 0; controlIndex < gamepad.allControls.Count; ++controlIndex)
			{
				InputControl control = gamepad.allControls[controlIndex];
				if (control.valueType == typeof(float))
				{
					float inputFloat = (float)control.ReadValueAsObject();
					if (inputFloat >= 0.2f)
					{
						return true;
					}
				}
				else if (control.valueType == typeof(Vector2))
				{
					Vector2 inputAxes = (Vector2)control.ReadValueAsObject();
					if (inputAxes.sqrMagnitude >= 0.5f)
					{
						return true;
					}
				}
			}

			return false;
		}

		public void UseSelectEvent()
		{
			/*UICancel = false;
			UICancelPressed = false;

			UIConfirm = false;
			UIConfirmPressed = false;*/
		}


	}
}