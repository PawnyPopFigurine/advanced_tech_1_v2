using JZK.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.UI
{
    public class MainMenuUIController : UIController
    {
        public override void UpdateController()
        {
            base.UpdateController();
        }

        public void UpdateInput()
        {

        }

        public void Input_ControlSettingsButton()
        {
            MainMenuUISystem.Instance.Input_ControlSettingsButton();
        }
    }
}