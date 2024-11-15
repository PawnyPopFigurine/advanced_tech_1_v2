using JZK.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JZK.UI
{
    public class ControlSettingsUIController : UIController
    {
        ESpeechInputType _recordForType;
        public ESpeechInputType RecordForType => _recordForType;

        bool _popupOpen;
        string _latestRecordedTerm;
        public string LatestRecordedTerm => _latestRecordedTerm;

        [SerializeField] GameObject _recordingPopup;
        [SerializeField] GameObject _popupHeader;
        [SerializeField] GameObject _recordedTerm;
        [SerializeField] TMP_Text _recordedTermText;
        [SerializeField] TMP_Text _settingInputText;

        [SerializeField] List<ScrollItemTermText> _scrollItemTermTexts;

        [SerializeField] GameObject _defaultSelected;

        [SerializeField] ScrollRect _scrollRect;
        [SerializeField] RectTransform _contentPanel;

        [SerializeField] GameObject _resetButton;
        [SerializeField] GameObject _backButton;

        [SerializeField] GameObject _backOutPrompt;

        bool _leftPopupLastFrame;

        GameObject _lastSelectedBeforePopupOpen;

        [SerializeField] TMP_Dropdown _languageDropdown;
        [SerializeField] RectTransform _languageDropdownContentPanel;
        [SerializeField] ScrollRect _languageDropdownScrollRect;

        [SerializeField] GameObject _confirmLanguageButton;

        [SerializeField] GameObject _dropdownItemParent;

        public override void Initialise()
        {
            base.Initialise();

            _languageDropdown.ClearOptions();

            List<TMP_Dropdown.OptionData> optionDataList = new();
            foreach(ESpeechRegion region in SpeechHelper.ALL_LANGUAGE_ENUMS)
            {
                string regionString = region.ToString();
                TMP_Dropdown.OptionData optionData = new();
                optionData.text = regionString;
                optionDataList.Add(optionData);
            }

            _languageDropdown.AddOptions(optionDataList);
            //_languageDropdown.
        }


        public override void SetActive(bool active)
        {
            base.SetActive(active);
            if(active)
            {
                RefreshOnActive();
            }
        }

        void RefreshOnActive()
        {
            _recordForType = ESpeechInputType.None;

            TogglePopup(false);
            RefreshDisplayedTerms();
            RefreshRegionDropdown();
            EventSystem.current.SetSelectedGameObject(_defaultSelected);
            RectTransform defaultSelectRect = _defaultSelected.gameObject.GetComponent<RectTransform>();
            SnapScrollTo(defaultSelectRect);

            
        }

        void RefreshDisplayedTerms()
        {
            foreach(ScrollItemTermText item in _scrollItemTermTexts)
            {
                string currentTermForType = SpeechInputSystem.Instance.GetTermForType(item.InputType);
                TMP_Text itemText = item.gameObject.GetComponent<TMP_Text>();
                itemText.text = currentTermForType;
            }
        }

        void RefreshRegionDropdown()
        {
            _languageDropdown.value = (int)SpeechRecognitionSystem.Instance.CurrentRegion;
            _languageDropdown.RefreshShownValue();
        }

        public override void UpdateController()
        {
            base.UpdateController();

            UpdateInput();

            _leftPopupLastFrame = false;
        }

        public void UpdateInput()
        {
            if(SpeechInputSystem.Instance.UIBackPressed)
            {
                Input_BackButtonPressed();
                return;
            }

            if(_popupOpen)
            {
                if(SpeechInputSystem.Instance.UIConfirmPressed)
                {
                    ConfirmPopupRecording();
                }
                if(SpeechInputSystem.Instance.UIBackPressed)
                {
                    CancelPopupRecording();
                }
            }

            if(!_popupOpen)
            {
                GameObject currentSelectedGO = EventSystem.current.currentSelectedGameObject;
                if (null != currentSelectedGO)
                {
                    Selectable currentSelectable = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();

                    if (null != currentSelectable)
                    {

                        if (SpeechInputSystem.Instance.DPadDownPressed)
                        {
                            Selectable selectOnDown = currentSelectable.FindSelectableOnDown();
                            if (selectOnDown != null)
                            {
                                GameObject selectOnDownGO = selectOnDown.gameObject;
                                EventSystem.current.SetSelectedGameObject(selectOnDownGO);
                                RectTransform scrollToRect = selectOnDownGO.GetComponent<RectTransform>();
                                if (!_languageDropdown.IsExpanded)
                                {
                                    SnapScrollTo(scrollToRect);

                                }
                            }
                        }

                        if (SpeechInputSystem.Instance.DPadUpPressed)
                        {
                            Selectable selectOnUp = currentSelectable.FindSelectableOnUp();
                            if (selectOnUp != null)
                            {
                                GameObject selectOnUpGO = selectOnUp.gameObject;
                                EventSystem.current.SetSelectedGameObject(selectOnUpGO);
                                RectTransform scrollToRect = selectOnUpGO.GetComponent<RectTransform>();
                                if (!_languageDropdown.IsExpanded)
                                {
                                    SnapScrollTo(scrollToRect);

                                }
                            }
                        }
                        

                        if (SpeechInputSystem.Instance.DPadLeftPressed)
                        {
                            Selectable selectOnLeft = currentSelectable.FindSelectableOnLeft();
                            if (selectOnLeft != null)
                            {
                                GameObject selectOnLeftGO = selectOnLeft.gameObject;
                                EventSystem.current.SetSelectedGameObject(selectOnLeftGO);
                                RectTransform scrollToRect = selectOnLeftGO.GetComponent<RectTransform>();
                                SnapScrollTo(scrollToRect);
                            }
                        }

                        if (SpeechInputSystem.Instance.DPadRightPressed)
                        {
                            Selectable selectOnRight = currentSelectable.FindSelectableOnRight();
                            if (selectOnRight != null)
                            {
                                GameObject selectOnRightGO = selectOnRight.gameObject;
                                EventSystem.current.SetSelectedGameObject(selectOnRightGO);
                                RectTransform scrollToRect = selectOnRightGO.GetComponent<RectTransform>();
                                SnapScrollTo(scrollToRect);
                            }
                        }

                        if (!_leftPopupLastFrame)
                        {
                            if (SpeechInputSystem.Instance.UIConfirmPressed)
                            {
                                RecordButton selectedRecordButton = currentSelectable.gameObject.GetComponent<RecordButton>();
                                if (selectedRecordButton != null)
                                {
                                    selectedRecordButton.Input_ButtonPressed();
                                }
                                else
                                {
                                    if (currentSelectable.gameObject == _resetButton)
                                    {
                                        Input_ResetButtonPressed();
                                    }
                                    if (currentSelectable.gameObject == _backButton)
                                    {
                                        Input_BackButtonPressed();
                                    }
                                    if (currentSelectable.gameObject == _confirmLanguageButton)
                                    {
                                        Input_ConfirmLanguageButtonPressed();
                                    }
                                    if (currentSelectable.gameObject == _languageDropdown.gameObject)
                                    {
                                        _languageDropdown.Show();
                                    }
                                    if (currentSelectable.transform.IsChildOf(_languageDropdown.transform))
                                    {
                                        int dropdownItemIndex = currentSelectable.transform.GetSiblingIndex() - 1;
                                        if(dropdownItemIndex > 0 && dropdownItemIndex < SpeechHelper.ALL_LANGUAGE_ENUMS.Count)
                                        {
                                            _languageDropdown.value = dropdownItemIndex;
                                            _languageDropdown.RefreshShownValue();
                                        }
                                    }
                                }
                            }
                        }

                        if (InputSystem.Instance.DPadLeftPressed ||
                            InputSystem.Instance.DPadRightPressed)
                        {
                            SnapScrollTo(currentSelectable.gameObject.GetComponent<RectTransform>());
                        }

                        if(_languageDropdown.IsExpanded)
                        {
                            if(SpeechInputSystem.Instance.DPadUpPressed)
                            {
                                /*int upperDropdownIndex = _languageDropdown.value - 1;
                                if(upperDropdownIndex > 0)
                                {
                                    _languageDropdown.value = upperDropdownIndex;
                                    _languageDropdown.RefreshShownValue();
                                }*/
                            }

                            if(SpeechInputSystem.Instance.DPadDownPressed)
                            {
                                /*int lowerDropdownIndex = _languageDropdown.value + 1;
                                if(lowerDropdownIndex < _languageDropdown.options.Count)
                                {
                                    _languageDropdown.value = lowerDropdownIndex;
                                    _languageDropdown.RefreshShownValue();
                                }*/
                            }
                        }
                        else
                        {
                            if (InputSystem.Instance.DPadUpPressed ||
                                InputSystem.Instance.DPadDownPressed)
                            {
                                SnapScrollTo(currentSelectable.gameObject.GetComponent<RectTransform>());
                            }
                        }
                    }
                }
            }
        }

        void TogglePopup(bool active)
        {
            _recordingPopup.SetActive(active);
            _popupOpen = active;

            if(active)
            {
                RefreshPopupOnOpen();
                string inputName = SpeechHelper.INPUT_NAMES[_recordForType];
                _settingInputText.text = inputName;
                SpeechRecognitionSystem.Instance.IsSettingTerm = true;
                _lastSelectedBeforePopupOpen = EventSystem.current.currentSelectedGameObject;
            }
            else
            {
                SpeechRecognitionSystem.Instance.IsSettingTerm = false;
                _leftPopupLastFrame = true;
                EventSystem.current.SetSelectedGameObject(_lastSelectedBeforePopupOpen);
            }
        }

        void RefreshPopupOnOpen()
        {
            _popupHeader.SetActive(true);
            _recordedTerm.SetActive(false);
            _backOutPrompt.SetActive(true);
            _latestRecordedTerm = string.Empty;
            _recordedTermText.text = string.Empty;
        }

        public void Input_BackButtonPressed()
        {
            if(_popupOpen)
            {
                TogglePopup(false);
                return;
            }

            if(EventSystem.current.currentSelectedGameObject.transform.IsChildOf(_languageDropdown.transform))
            {
                _languageDropdown.Hide();
                return;
            }

            else
            {
                ControlSettingsUISystem.Instance.Input_BackButtonPressed();
            }
        }

        public void Input_RecordButtonPressed(ESpeechInputType type)
        {
            _recordForType = type;
            TogglePopup(true);
        }

        public void ConfirmPopupRecording()
        {
            if(!IsTermValidForCustomInput(_latestRecordedTerm))
            {
                Debug.Log(this.name + " - tried to confirm an invalid term " + _latestRecordedTerm + " - aborting action");
                return;
            }
            SpeechInputSystem.Instance.SetTermForType(_recordForType, _latestRecordedTerm);
            SpeechInputSystem.Instance.SaveCurrentSpeechData();
            RefreshDisplayedTerms();
            TogglePopup(false);
        }

        bool IsTermValidForCustomInput(string term)
        {
            if(term == string.Empty)
            {
                return false;
            }

            if(string.IsNullOrWhiteSpace(term))
            {
                return false;
            }

            if(term.Contains(" "))
            {
                return false;
            }

            string existingConfirmTerm = SpeechInputSystem.Instance.SpeechInputTerm_LUT[ESpeechInputType.UI_Confirm];
            if (existingConfirmTerm == term)
            {
                return false;
            }

            string existingBackTerm = SpeechInputSystem.Instance.SpeechInputTerm_LUT[ESpeechInputType.UI_Back];
            if (existingBackTerm == term)
            {
                return false;
            }

            return true;
        }

        public void CancelPopupRecording()
        {
            TogglePopup(false);
        }

        public void Input_ResetButtonPressed()
        {
            SpeechInputSystem.Instance.ResetTermsToDefault();
            SpeechRecognitionSystem.Instance.ResetRegion();
            SpeechInputSystem.Instance.SaveCurrentSpeechData();
            RefreshDisplayedTerms();
            RefreshRegionDropdown();
        }

        public void Input_PopupConfirmPressed()
        {
            ConfirmPopupRecording();
        }

        public void Input_PopupBackPressed()
        {
            CancelPopupRecording();
        }

        public void Input_ConfirmLanguageButtonPressed()
        {
            int dropdownIndex = _languageDropdown.value;
            ESpeechRegion regionEnum = SpeechHelper.ALL_LANGUAGE_ENUMS[dropdownIndex];
            string languageCode = SpeechHelper.RegionStringFromEnum(regionEnum);
            Debug.Log("[HELLO] set language to " + languageCode + " - " + regionEnum.ToString());
            SpeechRecognitionSystem.Instance.SetRegionString(languageCode, regionEnum);
            SpeechInputSystem.Instance.SaveCurrentSpeechData();
        }

        public void SnapScrollTo(RectTransform targetRect)
        {
            if(!targetRect.IsChildOf(_scrollRect.transform))
            {
                Debug.Log("Tried to snap scroll to item not in scroll rect - aborting action");
                return;
            }

            Canvas.ForceUpdateCanvases();

            Vector2 newPos = (Vector2)_scrollRect.transform.InverseTransformPoint(_contentPanel.position)
                    - (Vector2)_scrollRect.transform.InverseTransformPoint(targetRect.position);

            _contentPanel.anchoredPosition = new(_contentPanel.anchoredPosition.x, newPos.y);

        }

        public void OnSpeechRecognised(string speech)
        {
            if(!_isActive)
            {
                return;
            }

            if(!_popupOpen)
            {
                return;
            }

            if(!SpeechInputSystem.Instance.VoiceControlEnabled)
            {
                return;
            }

            if (IsTermValidForCustomInput(_latestRecordedTerm))
            {
                return;
            }

            string processedSpeech = SpeechHelper.ProcessSpeechTerm(speech);

            string displaySpeech = new(processedSpeech);
            if(!IsTermValidForCustomInput(processedSpeech))
            {
                displaySpeech = string.Concat(displaySpeech, " - INVALID");
                _backOutPrompt.SetActive(false);
            }

            else
            {
                _backOutPrompt.SetActive(true);
                _latestRecordedTerm = processedSpeech;
            }

            _popupHeader.SetActive(false);

            _recordedTermText.text = displaySpeech;
            _recordedTerm.SetActive(true);
        }
    }
}