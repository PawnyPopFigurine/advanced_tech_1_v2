using JZK.Input;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

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

        bool _leftPopupLastFrame;


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
                Selectable currentSelectable = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();

                if (SpeechInputSystem.Instance.DPadDownPressed)
                {
                    Selectable selectOnDown = currentSelectable.FindSelectableOnDown();
                    if (selectOnDown != null)
                    {
                        GameObject selectOnDownGO = selectOnDown.gameObject;
                        EventSystem.current.SetSelectedGameObject(selectOnDownGO);
                        RectTransform scrollToRect = selectOnDownGO.GetComponent<RectTransform>();
                        SnapScrollTo(scrollToRect);
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
                        SnapScrollTo(scrollToRect);
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
                            if(currentSelectable.gameObject == _resetButton)
                            {
                                Input_ResetButtonPressed();
                            }
                            if(currentSelectable.gameObject == _backButton)
                            {
                                Input_BackButtonPressed();
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
            }
            else
            {
                SpeechRecognitionSystem.Instance.IsSettingTerm = false;
                _leftPopupLastFrame = true;
            }
        }

        void RefreshPopupOnOpen()
        {
            _popupHeader.SetActive(true);
            _recordedTerm.SetActive(false);
            _latestRecordedTerm = string.Empty;
            _recordedTermText.text = string.Empty;
        }

        public void Input_BackButtonPressed()
        {
            if(_popupOpen)
            {
                TogglePopup(false);
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
            if(_latestRecordedTerm == string.Empty)
            {
                Debug.Log(this.name + " - tried to confirm an empty recording. aborting action");
                return;
            }
            SpeechInputSystem.Instance.SetTermForType(_recordForType, _latestRecordedTerm);
            SpeechInputSystem.Instance.SaveCurrentTerms();
            RefreshDisplayedTerms();
            TogglePopup(false);
        }

        public void CancelPopupRecording()
        {
            TogglePopup(false);
        }

        public void Input_ResetButtonPressed()
        {
            SpeechInputSystem.Instance.ResetTermsToDefault();
            SpeechInputSystem.Instance.SaveCurrentTerms();
            RefreshDisplayedTerms();
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

            if (_latestRecordedTerm != string.Empty)
            {
                return;
            }

            string processedSpeech = SpeechHelper.ProcessSpeechTerm(speech);

            _popupHeader.SetActive(false);
            _recordedTerm.SetActive(true);
            _recordedTermText.text = processedSpeech;
            _latestRecordedTerm = processedSpeech;
        }
    }
}