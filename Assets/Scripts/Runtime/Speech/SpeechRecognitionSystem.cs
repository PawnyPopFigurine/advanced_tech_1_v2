using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using JZK.UI;
using static JZK.UI.UIStateSystem;

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

        public SystemLoadData _loadData = new SystemLoadData()
        {
            LoadStates = SystemLoadState.NoLoadingNeeded,
            UpdateAfterLoadingState = ELoadingState.FrontEnd,
        };

        public override SystemLoadData LoadData => _loadData;

        public override void UpdateSystem()
        {
            base.UpdateSystem();
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
        }

        public async void StartContinuousRecognition()
        {
            SpeechConfig config = SpeechConfig.FromSubscription("af6765f414254e4bb35c0efdfa9adeca", "eastus");
            config.SetProperty(PropertyId.Speech_SegmentationSilenceTimeoutMs, "300");

            _isRecording = true;

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

        public async void StopContinuousRecognition()
        {
            await _recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            _recognizer.Dispose();

            _isRecording = false;
        }

        void OnSpeechRecognized(object sender, SpeechRecognitionEventArgs e)
        {
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
            _stopRecognition.TrySetResult(0);
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
    }
}