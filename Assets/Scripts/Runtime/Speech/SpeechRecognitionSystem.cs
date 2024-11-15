using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using JZK.UI;
using static JZK.UI.UIStateSystem;
using System;

namespace JZK.Input
{
    public class SpeechRecognitionSystem : GameSystem<SpeechRecognitionSystem>
    {
        SpeechRecognizer _recognizer;

        TaskCompletionSource<int> _stopRecognition;
        bool _isRecording;
        public bool IsRecording => _isRecording;

        public bool IsSettingTerm;

        public bool RecordedThisFrame { get; private set; }
        public string LatestRecordedSpeech { get; private set; }

        public delegate void SpeechEvent(string speech);
        public event SpeechEvent OnSpeechRecognised;

        private string _currentLanguageString = SpeechHelper.FALLBACK_DEFAULT_REGIONCODE;
        public string CurrentLanguageString => _currentLanguageString;

        ESpeechRegion _currentRegion = SpeechHelper.FALLBACK_DEFAULT_REGIONENUM;
        public ESpeechRegion CurrentRegion => _currentRegion;

        public SystemLoadData _loadData = new SystemLoadData()
        {
            LoadStates = SystemLoadState.NoLoadingNeeded,
            UpdateAfterLoadingState = ELoadingState.FrontEnd,
        };

        public override SystemLoadData LoadData => _loadData;

        public override void UpdateSystem()
        {
            base.UpdateSystem();

            if(!_isRecording && !UIStateSystem.Instance.QuitTriggered)
            {
                Debug.Log(this.name + " - not recording this frame - starting continuous recognition again");
                StartContinuousRecognition();
            }
        }

        public override void LateUpdateSystem()
        {
            base.LateUpdateSystem();

            RecordedThisFrame = false;
            LatestRecordedSpeech = string.Empty;
        }

        public override void SetCallbacks()
        {
            base.SetCallbacks();

            SceneInit.CurrentSceneInit.OnLoadingStateComplete -= OnLoadingStateComplete;
            SceneInit.CurrentSceneInit.OnLoadingStateComplete += OnLoadingStateComplete;

            /*SpeechDataSystem.Instance.OnSystemDataLoaded -= OnSystemDataLoaded;
            SpeechDataSystem.Instance.OnSystemDataLoaded += OnSystemDataLoaded;*/
        }

        public void StartContinuousRecognition()
        {
            if(_isRecording)
            {
                Debug.LogWarning(this.name + " - tried to start recording, when recording already - aborting action");
                return;
            }
            Debug.Log("[HELLO] starting continuous recognition");
            StartContinuousRecognitionAsync();
            _isRecording = true;
        }

        public void StopContinuousRecognition()
        {
            /*if(!_isRecording)
            {
                Debug.LogWarning(this.name + " - tried to stop recording before recording started - aborting action");
                return;
            }*/
            Debug.Log("[HELLO] stopping continuous recognition");
            StopContinuousRecognitionAsync();
            _isRecording = false;
        }

        async void StartContinuousRecognitionAsync()
        {
            SpeechConfig config = SpeechConfig.FromSubscription("af6765f414254e4bb35c0efdfa9adeca", "eastus");
            config.SetProperty(PropertyId.Speech_SegmentationSilenceTimeoutMs, "300");
            config.SpeechRecognitionLanguage = _currentLanguageString;
            //config.SpeechRecognitionLanguage = "en-GB";

            //_isRecording = true;

            _recognizer = new SpeechRecognizer(config);

            _stopRecognition = new TaskCompletionSource<int>();

            _recognizer.Recognized -= OnSpeechRecognized;
            _recognizer.Recognized += OnSpeechRecognized;

            _recognizer.Recognizing -= OnSpeechRecognizing;
            _recognizer.Recognizing += OnSpeechRecognizing;

            _recognizer.Canceled -= OnSpeechSessionCancelled;
            _recognizer.Canceled += OnSpeechSessionCancelled;

            _recognizer.SessionStopped -= OnSpeechSessionStopped;
            _recognizer.SessionStopped += OnSpeechSessionStopped;

            _recognizer.SpeechEndDetected -= OnSpeechEndDetected;
            _recognizer.SpeechEndDetected += OnSpeechEndDetected;

            await _recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            Task.WaitAny(new[] { _stopRecognition.Task });
        }

        async void StopContinuousRecognitionAsync()
        {
            try
            {
                if(null != _recognizer)
                {
                    await _recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                    _recognizer.Dispose();
                }
                else
                {
                    //Debug.LogWarning()
                }
            }
            catch(ObjectDisposedException e)
            {
                //Debug.LogWarning(this.name + " - tried to dispose disposed object _recognizer - " + e.ToString());
            }

            //_isRecording = false;
        }

        void OnSpeechRecognized(object sender, SpeechRecognitionEventArgs e)
        {
            //Debug.Log("[HELLO] recorded this frame? " + RecordedThisFrame.ToString());
            if(e.Result.Text == string.Empty)
            {
                return;
            }

            RecordedThisFrame = true;
            LatestRecordedSpeech = e.Result.Text;
        }

        void OnSpeechRecognizing(object sender, SpeechRecognitionEventArgs e)
        {

        }

        void OnSpeechSessionStopped(object sender, SessionEventArgs e)
        {
            _isRecording = false;
        }

        void OnSpeechSessionCancelled(object sender, SpeechRecognitionEventArgs e)
        {
            Debug.Log("[HELLO] speech session cancelled");
            _stopRecognition.TrySetResult(0);
            _isRecording = false;

            /*if (true)
            {
                Debug.Log("[HELLO] restarting recog after cancel");
                StartContinuousRecognition();
            }*/
        }

        private void OnSpeechEndDetected(object sender, RecognitionEventArgs e)
        {

        }


        private void OnLoadingStateComplete(ELoadingState state)
        {
            if (state != ELoadingState.FrontEnd)
            {
                return;
            }

            if (SceneInit.IsTestScene)
            {
                return;
            }

            StartContinuousRecognition();
        }

        public void OnQuitTriggered()
        {
            StopContinuousRecognition();
        }

        public void SetRegionString(string language, ESpeechRegion region)
        {
            if (language == string.Empty)   //TODO: set up language code list and check this value isn't in that
            {
                Debug.LogWarning(this.name + " - recieved save data with empty language string - defaulting to en-GB");
                language = ("en-GB");
            }


            _currentLanguageString = language;
            _currentRegion = region;
            Debug.Log(this.name + " - setting language code to " +  language);

            StopContinuousRecognition();
            StartContinuousRecognition();
        }

        public void ResetRegion()
        {
            _currentLanguageString = "en-GB";
            _currentRegion = ESpeechRegion.English_GB;
        }

        /*public void OnSystemDataLoaded(object loadedData)
        {
            SpeechSaveData saveData = (SpeechSaveData)loadedData;

            if(saveData.LanguageCode == string.Empty)   //TODO: set up language code list and check this value isn't in that
            {
                Debug.LogWarning(this.name + " - recieved save data with empty language string - defaulting to en-GB");
                SetLanguageString("en-GB");
            }

            else
            {
                SetLanguageString(saveData.LanguageCode);
            }
        }*/
    }
}