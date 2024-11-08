using JZK.Framework;
using JZK.Input;
using JZK.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
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

        static float DEFAULT_GAME_SPEED = 2f;

        private EGameState _currentState;

        float _timeSinceGameStart = 0;
        float _timeToNextBlock;

        static List<ESpeechInputType_Flag> VALID_HAZARD_FLAGS = new()
        {
            ESpeechInputType_Flag.Game_FaceNorth,
            ESpeechInputType_Flag.Game_FaceSouth,

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

            if(!playerGO.TryGetComponent(out PlayerController controller))
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

            _currentGameSpeed = DEFAULT_GAME_SPEED;

            _currentState = EGameState.Active;

            _isInGameplay = true;
            _timeSinceGameStart = 0f;
            PlaceHazardBlock();
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
            _timeToNextBlock = _timeSinceGameStart + nextTimeInterval;
        }

        public override void UpdateSystem()
        {
            if (!_isInGameplay)
            {
                return;
            }

            switch(_currentState)
            {
                case EGameState.Active:
                    UpdateInput();
                    UpdateBlockPlacement();
                    UpdateBlockMovement();
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
                }
            }
        }
        
        void UpdateBlockPlacement()
        {
            _timeSinceGameStart += Time.deltaTime;

            if(_timeSinceGameStart >= _timeToNextBlock)
            {
                PlaceHazardBlock();
            }
        }

        void UpdateBlockMovement()
        {
            foreach(HazardBlockController hazardBlock in _activeBlocks)
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
        }
    }
}