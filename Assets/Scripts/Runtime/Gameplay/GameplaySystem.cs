using JZK.Framework;
using JZK.Input;
using JZK.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace JZK.Gameplay
{

    public enum EGameState
    {
        Active,
        Failure,
    }

    public class GameplaySystem : GameSystem<GameplaySystem>
    {
        #region PersistentSystem
        public SystemLoadData _loadData = new SystemLoadData()
        {
            LoadStates = new SystemLoadState[] { new SystemLoadState { LoadStartState = ELoadingState.GameData, BlockStateUntilFinished = ELoadingState.GameData } },
            UpdateAfterLoadingState = ELoadingState.GameData,
        };

        public override SystemLoadData LoadData => _loadData;

        public override void StartLoading(ELoadingState state)
        {
            base.StartLoading(state);

            Load();
        }

        public override void SetCallbacks()
        {
            base.SetCallbacks();

            OptionsDataSystem.Instance.OnSystemDataLoaded -= OnSystemDataLoaded;
            OptionsDataSystem.Instance.OnSystemDataLoaded += OnSystemDataLoaded;

        }
        #endregion //PersistentSystem

        public static Vector2 PLAYER_START_POS = new(-5f, -2.5f);
        public static Vector2 HAZARD_BLOCK_START_POS = new(10f, -2.8f);

        public static float MIN_BLOCK_INTERVAL = 1f;
        public static float MAX_BLOCK_INTERVAL = 3f;

        PlayerController _player;

        bool _isInGameplay;

        [SerializeField] GameObject _hazardPrefab;

        private List<HazardBlockController> _activeBlocks = new();

        private float _currentGameSpeed;

        private float _maxGameSpeed;
        public float MaxGameSpeed => _maxGameSpeed;

        public static float DEFAULT_MAX_SPEED = 3.5f;

        private EGameState _currentState;

        float _scaledTimeSinceStart = 0;
        float _unscaledTimeSinceStart = 0;

        float _timeToNextBlock;

        float _defaultStartSpeed = 1.5f;

        float _timeToNextSpeedIncrease;
        static float SPEED_INCREASE_INTERVAL = 4.5f;
        static float SPEED_INCREASE_BY_AMOUNT = 0.25f;

        static List<ESpeechInputType_Flag> VALID_HAZARD_FLAGS = new()
        {
            ESpeechInputType_Flag.Game_FaceNorth,
            ESpeechInputType_Flag.Game_FaceSouth,
            ESpeechInputType_Flag.Game_FaceWest,
            ESpeechInputType_Flag.Game_FaceEast,
            ESpeechInputType_Flag.Game_DPadUp,
            ESpeechInputType_Flag.Game_DPadDown,
            ESpeechInputType_Flag.Game_DPadLeft,
            ESpeechInputType_Flag.Game_DPadRight,
            ESpeechInputType_Flag.Game_LeftShoulder,
            ESpeechInputType_Flag.Game_RightShoulder,
            ESpeechInputType_Flag.Game_LeftTrigger,
            ESpeechInputType_Flag.Game_RightTrigger,
        };

        #region Load

        void Load()
        {
            Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Gameplay/Player/Player.prefab").Completed += PlayerLoadCompleted;
            Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Gameplay/Hazards/HazardBlock.prefab").Completed += HazardLoadCompleted;
        }

        void PlayerLoadCompleted(AsyncOperationHandle<GameObject> op)
        {
            if (op.Result == null)
            {
                Debug.LogError(this.GetType().ToString() + "- Failed to load addressable.");
                return;
            }

            GameObject playerGO = Instantiate(op.Result);

            if (!playerGO.TryGetComponent(out PlayerController controller))
            {
                Debug.LogError(this.name + " - failed to find PlayerController component on player prefab.");
                return;
            }

            _player = controller;
            playerGO.SetActive(false);
            playerGO.transform.parent = transform;

            FinishLoading(ELoadingState.GameData);
        }

        void HazardLoadCompleted(AsyncOperationHandle<GameObject> op)
        {
            if (op.Result == null)
            {
                Debug.LogError(this.GetType().ToString() + "- Failed to load addressable.");
                return;
            }

            _hazardPrefab = op.Result;

            FinishLoading(ELoadingState.GameData);
        }

        #endregion //Load

        public void StartGameplay()
        {
            _player.gameObject.SetActive(true);
            _player.transform.position = PLAYER_START_POS;

            SetCurrentGameSpeed(_defaultStartSpeed);

            _currentState = EGameState.Active;

            _isInGameplay = true;
            _scaledTimeSinceStart = 0f;
            _unscaledTimeSinceStart = 0f;
            _timeToNextSpeedIncrease = _unscaledTimeSinceStart + SPEED_INCREASE_INTERVAL;
            PlaceHazardBlock();

            GameplayUISystem.Instance.OnGameplayStart();
        }

        public void RestartGameplay()
        {
            DestroyAllHazardBlocks();
            StartGameplay();
        }

        public void DestroyAllHazardBlocks()
        {
            List<HazardBlockController> hazardsCache = new(_activeBlocks);
            foreach (HazardBlockController hazardBlockController in hazardsCache)
            {
                DestroyHazardBlock(hazardBlockController);
            }
        }

        ESpeechInputType_Flag GetSpeechInputForHazardBlock()
        {
            int flagIndex = Random.Range(0, VALID_HAZARD_FLAGS.Count);
            return VALID_HAZARD_FLAGS[flagIndex];
        }

        void PlaceHazardBlock()
        {
            GameObject hazardBlock = Instantiate(_hazardPrefab);
            hazardBlock.transform.SetParent(transform);
            hazardBlock.transform.localPosition = HAZARD_BLOCK_START_POS;

            HazardBlockController controller = hazardBlock.GetComponent<HazardBlockController>();

            ESpeechInputType_Flag blockInput = GetSpeechInputForHazardBlock();
            controller.Initialise(blockInput);

            _activeBlocks.Add(controller);

            //set time of next block placement
            float nextTimeInterval = Random.Range(MIN_BLOCK_INTERVAL, MAX_BLOCK_INTERVAL);
            _timeToNextBlock = _scaledTimeSinceStart + nextTimeInterval;
        }

        public override void UpdateSystem()
        {
            if (!_isInGameplay)
            {
                return;
            }

            switch (_currentState)
            {
                case EGameState.Active:
                    _scaledTimeSinceStart += Time.deltaTime * (_currentGameSpeed / 2);
                    _unscaledTimeSinceStart += Time.deltaTime;

                    UpdateInput();
                    UpdateBlockPlacement();
                    UpdateBlockMovement();
                    UpdateGameSpeedIncrease();
                    break;
                case EGameState.Failure:
                    break;
            }
        }

        void UpdateInput()
        {
            if (_activeBlocks.Count > 0)
            {
                HazardBlockController hazardToDestroy = _activeBlocks[0];

                switch (hazardToDestroy.InputType)
                {
                    case ESpeechInputType_Flag.Game_FaceNorth:
                        if (Input.InputSystem.Instance.FaceButtonNorthPressed || SpeechInputSystem.Instance.NorthFacePressed)
                        {
                            DestroyHazardBlock(hazardToDestroy);
                        }
                        break;
                    case ESpeechInputType_Flag.Game_FaceSouth:
                        if (Input.InputSystem.Instance.FaceButtonSouthPressed || SpeechInputSystem.Instance.SouthFacePressed)
                        {
                            DestroyHazardBlock(hazardToDestroy);
                        }
                        break;
                    case ESpeechInputType_Flag.Game_FaceWest:
                        if (Input.InputSystem.Instance.FaceButtonWestPressed || SpeechInputSystem.Instance.WestFacePressed)
                        {
                            DestroyHazardBlock(hazardToDestroy);
                        }
                        break;
                    case ESpeechInputType_Flag.Game_FaceEast:
                        if (Input.InputSystem.Instance.FaceButtonEastPressed || SpeechInputSystem.Instance.EastFacePressed)
                        {
                            DestroyHazardBlock(hazardToDestroy);
                        }
                        break;
                    case ESpeechInputType_Flag.Game_DPadUp:
                        if (Input.InputSystem.Instance.DPadUpPressed || SpeechInputSystem.Instance.DPadUpPressed)
                        {
                            DestroyHazardBlock(hazardToDestroy);
                        }
                        break;
                    case ESpeechInputType_Flag.Game_DPadDown:
                        if (Input.InputSystem.Instance.DPadDownPressed || SpeechInputSystem.Instance.DPadDownPressed)
                        {
                            DestroyHazardBlock(hazardToDestroy);
                        }
                        break;
                    case ESpeechInputType_Flag.Game_DPadLeft:
                        if (Input.InputSystem.Instance.DPadLeftPressed || SpeechInputSystem.Instance.DPadLeftPressed)
                        {
                            DestroyHazardBlock(hazardToDestroy);
                        }
                        break;
                    case ESpeechInputType_Flag.Game_DPadRight:
                        if (Input.InputSystem.Instance.DPadRightPressed || SpeechInputSystem.Instance.DPadRightPressed)
                        {
                            DestroyHazardBlock(hazardToDestroy);
                        }
                        break;
                    case ESpeechInputType_Flag.Game_LeftShoulder:
                        if (Input.InputSystem.Instance.LeftShoulderPressed || SpeechInputSystem.Instance.LeftShoulderPressed)
                        {
                            DestroyHazardBlock(hazardToDestroy);
                        }
                        break;
                    case ESpeechInputType_Flag.Game_LeftTrigger:
                        if (Input.InputSystem.Instance.LeftTriggerPressed || SpeechInputSystem.Instance.LeftTriggerPressed)
                        {
                            DestroyHazardBlock(hazardToDestroy);
                        }
                        break;
                    case ESpeechInputType_Flag.Game_RightTrigger:
                        if (Input.InputSystem.Instance.RightTriggerPressed || SpeechInputSystem.Instance.RightTriggerPressed)
                        {
                            DestroyHazardBlock(hazardToDestroy);
                        }
                        break;
                    case ESpeechInputType_Flag.Game_RightShoulder:
                        if (Input.InputSystem.Instance.RightShoulderPressed || SpeechInputSystem.Instance.RightShoulderPressed)
                        {
                            DestroyHazardBlock(hazardToDestroy);
                        }
                        break;
                }
            }
        }

        void UpdateBlockPlacement()
        {
            if (_scaledTimeSinceStart >= _timeToNextBlock)
            {
                PlaceHazardBlock();
            }
        }

        void UpdateGameSpeedIncrease()
        {
            if (_currentGameSpeed >= _maxGameSpeed)
            {
                return;
            }

            if (_unscaledTimeSinceStart >= _timeToNextSpeedIncrease)
            {
                IncreaseGameSpeedByInterval();
            }
        }

        void IncreaseGameSpeedByInterval()
        {
            if (_currentGameSpeed >= _maxGameSpeed)
            {
                return;
            }

            float nextGameSpeed = _currentGameSpeed + SPEED_INCREASE_BY_AMOUNT;
            Debug.Log("[SPEEDUP] - time is " + _unscaledTimeSinceStart + " , set speed from " + _currentGameSpeed + " to " + nextGameSpeed + " - next speed increase at " + (_unscaledTimeSinceStart + _timeToNextSpeedIncrease).ToString());
            SetCurrentGameSpeed(nextGameSpeed);
            _timeToNextSpeedIncrease = _unscaledTimeSinceStart + SPEED_INCREASE_INTERVAL;
        }

        void UpdateBlockMovement()
        {
            foreach (HazardBlockController hazardBlock in _activeBlocks)
            {
                GameObject hazardGO = hazardBlock.gameObject;
                Vector2 newPos = new(hazardGO.transform.localPosition.x - (_currentGameSpeed * Time.deltaTime), hazardGO.transform.localPosition.y);
                hazardGO.transform.localPosition = newPos;
            }
        }

        void DestroyHazardBlock(HazardBlockController hazardBlock)
        {
            _activeBlocks.Remove(hazardBlock);
            Destroy(hazardBlock.gameObject);
        }

        public void OnPlayerHitHazard()
        {
            Debug.Log("[YOU DIED]");
            _currentState = EGameState.Failure;

            GameplayUISystem.Instance.OnPlayerDeath();
        }

        public void OnSystemDataLoaded(object loadedData)
        {
            OptionsSaveData saveData = (OptionsSaveData)loadedData;

            _maxGameSpeed = saveData.MaxGameSpeed;
        }

        public void SetMaxGameSpeed(float maxSpeed)
        {
            _maxGameSpeed = maxSpeed;
        }

        public void ResetMaxSpeed()
        {
            SetMaxGameSpeed(DEFAULT_MAX_SPEED);
        }

        void SetCurrentGameSpeed(float gameSpeed)
        {
            float settingSpeed = Mathf.Min(gameSpeed, _maxGameSpeed);
            _currentGameSpeed = settingSpeed;
        }
    }
}