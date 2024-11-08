using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.UI
{
    public class GameplayUIController : UIController
    {
        [SerializeField] GameObject _deathOverlayRoot;

        public void OnPlayerDeath()
        {
            ToggleDeathOverlay(true);
        }

        public void OnGameplayStart()
        {
            ToggleDeathOverlay(false);
        }

        public void OnRestart()
        {
            ToggleDeathOverlay(false);
        }

        public void OnMainMenu()
        {
            ToggleDeathOverlay(false);
        }

        void ToggleDeathOverlay(bool active)
        {
            _deathOverlayRoot.SetActive(active);
        }


        public void Input_RestartButtonPressed()
        {
            GameplayUISystem.Instance.Input_RestartButtonPressed();
        }

        public void Input_MainMenuButtonPressed()
        {
            GameplayUISystem.Instance.Input_MainMenuButtonPressed();
        }

        public void Input_QuitButtonPressed()
        {
            GameplayUISystem.Instance.Input_QuitButtonPressed();
        }   
    }
}