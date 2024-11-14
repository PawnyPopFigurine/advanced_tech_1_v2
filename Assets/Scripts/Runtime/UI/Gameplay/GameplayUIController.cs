using JZK.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JZK.UI
{
    public class GameplayUIController : UIController
    {
        [SerializeField] GameObject _deathOverlayRoot;

        bool _deathOverlayActive;

        [SerializeField] GameObject _defaultSelected;

        [SerializeField] GameObject _restartButton;
        [SerializeField] GameObject _menuButton;
        [SerializeField] GameObject _quitButton;

        public override void UpdateController()
        {
            base.UpdateController();

            if(_deathOverlayActive)
            {
                UpdateDeathOverlay();
            }
        }

        void UpdateDeathOverlay()
        {
            if(!_deathOverlayActive)
            {
                return;
            }

            if(SpeechInputSystem.Instance.UIConfirmPressed)
            {
                if(EventSystem.current.currentSelectedGameObject == _restartButton)
                {
                    Input_RestartButtonPressed();
                    return;
                }

                if(EventSystem.current.currentSelectedGameObject == _menuButton)
                {
                    Input_MainMenuButtonPressed();
                    return;
                }

                if(EventSystem.current.currentSelectedGameObject == _quitButton)
                {
                    Input_QuitButtonPressed();
                    return;
                }
            }

            if(null != EventSystem.current.currentSelectedGameObject)
            {
                Selectable currentSelectable = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();

                if (SpeechInputSystem.Instance.DPadDownPressed)
                {
                    Selectable selectOnDown = currentSelectable.FindSelectableOnDown();
                    if (selectOnDown != null)
                    {
                        GameObject selectOnDownGO = selectOnDown.gameObject;
                        EventSystem.current.SetSelectedGameObject(selectOnDownGO);
                    }
                }

                if (SpeechInputSystem.Instance.DPadUpPressed)
                {
                    Selectable selectOnUp = currentSelectable.FindSelectableOnUp();
                    if (selectOnUp != null)
                    {
                        GameObject selectOnUpGO = selectOnUp.gameObject;
                        EventSystem.current.SetSelectedGameObject(selectOnUpGO);
                    }
                }
            }
        }

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
            _deathOverlayActive = active;

            if(active)
            {
                EventSystem.current.SetSelectedGameObject(_defaultSelected);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
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