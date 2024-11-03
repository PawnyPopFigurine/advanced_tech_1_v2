using JZK.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JZK.UI
{
    public class MainMenuUIController : UIController
    {
        [SerializeField] GameObject _defaultSelected;

        [SerializeField] GameObject _controlSettingsButton;
        [SerializeField] GameObject _quitButton;

        public override void SetActive(bool active)
        {
            base.SetActive(active);
            if (active)
            {
                RefreshOnActive();
            }
        }

        void RefreshOnActive()
        {
            EventSystem.current.SetSelectedGameObject(_defaultSelected);
        }

        public override void UpdateController()
        {
            base.UpdateController();

            UpdateInput();
        }


        public void UpdateInput()
        {
            if(SpeechInputSystem.Instance.UIConfirmPressed)
            {
                if(EventSystem.current.currentSelectedGameObject == _controlSettingsButton)
                {
                    Input_ControlSettingsButton();
                    return;
                }

                if(EventSystem.current.currentSelectedGameObject == _quitButton)
                {
                    Input_QuitButton();
                    return;
                }
            }

            Selectable currentSelectable = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();

            if(SpeechInputSystem.Instance.DPadDownPressed)
            {
                Selectable selectOnDown = currentSelectable.FindSelectableOnDown();
                if(selectOnDown != null)
                {
                    GameObject selectOnDownGO = selectOnDown.gameObject;
                    EventSystem.current.SetSelectedGameObject(selectOnDownGO);
                }
            }

            if(SpeechInputSystem.Instance.DPadUpPressed)
            {
                Selectable selectOnUp = currentSelectable.FindSelectableOnUp();
                if (selectOnUp != null)
                {
                    GameObject selectOnUpGO = selectOnUp.gameObject;
                    EventSystem.current.SetSelectedGameObject(selectOnUpGO);
                }
            }
        }

        public void Input_ControlSettingsButton()
        {
            MainMenuUISystem.Instance.Input_ControlSettingsButton();
        }

        public void Input_QuitButton()
        {
            UIStateSystem.Instance.TriggerQuit();
        }
    }
}