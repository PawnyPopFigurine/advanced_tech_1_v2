using JZK.Framework;
using JZK.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class GameplaySystem : PersistentSystem<GameplaySystem>
    {
        #region PersistentSystem
        public SystemLoadData _loadData = new SystemLoadData()
        {
            LoadStates = SystemLoadState.NoLoadingNeeded,
            UpdateAfterLoadingState = ELoadingState.Game,
        };

        public override SystemLoadData LoadData => _loadData;
        #endregion //PersistentSystem

        public override void UpdateSystem()
        {
            base.UpdateSystem();
            UpdateInput();
        }

        void UpdateInput()
        {
            if(InputSystem.Instance.FaceButtonNorthPressed || SpeechInputSystem.Instance.NorthFacePressed)
            {
                Debug.Log("[HELLO] pressed Triangle!");
            }

            if(InputSystem.Instance.FaceButtonSouthPressed || SpeechInputSystem.Instance.SouthFacePressed)
            {
                Debug.Log("[HELLO] pressed Cross!");
            }

            if(InputSystem.Instance.FaceButtonWestPressed || SpeechInputSystem.Instance.WestFacePressed)
            {
                Debug.Log("[HELLO] pressed Square!");
            }

            if(InputSystem.Instance.FaceButtonEastPressed || SpeechInputSystem.Instance.EastFacePressed)
            {
                Debug.Log("[HELLO] pressed Circle!");
            }
        }
    }
}