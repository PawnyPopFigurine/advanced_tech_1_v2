using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using JZK.UI;

namespace JZK.Input
{
    public class SpeechRecognitionSystem : GameSystem<SpeechRecognitionSystem>
    {
        TaskCompletionSource<int> _stopRecognition;
        bool _isRecording;

        public bool IsSettingTerm;

        public bool RecordedThisFrame { get; private set; }
        public string LatestRecordedSpeech { get; private set; }

        public delegate void SpeechEvent(string speech);
        public event SpeechEvent OnSpeechRecognised;

        public SystemLoadData _loadData = new SystemLoadData()
        {
            LoadStates = SystemLoadState.NoLoadingNeeded,
            UpdateAfterLoadingState = ELoadingState.Game,
        };

        public override SystemLoadData LoadData => _loadData;

        public override void UpdateSystem()
        {
            base.UpdateSystem();

            RecordedThisFrame = false;
            LatestRecordedSpeech = string.Empty;
        }

        public async void StartContinuousRecognition()
        {
            SpeechConfig config = SpeechConfig.FromSubscription("af6765f414254e4bb35c0efdfa9adeca", "eastus");
            config.SetProperty(PropertyId.Speech_SegmentationSilenceTimeoutMs, "300");

            using (var recognizer = new SpeechRecognizer(config))
            {
                _stopRecognition = new TaskCompletionSource<int>();

                recognizer.Recognized -= OnSpeechRecognized;
                recognizer.Recognized += OnSpeechRecognized;

                recognizer.Recognizing -= OnSpeechRecognizing;
                recognizer.Recognizing += OnSpeechRecognizing;

                recognizer.Canceled -= OnSpeechSessionCancelled;
                recognizer.Canceled += OnSpeechSessionCancelled;

                recognizer.SessionStopped -= OnSpeechSessionStopped;
                recognizer.SessionStopped += OnSpeechSessionStopped;

                recognizer.SpeechEndDetected -= OnSpeechEndDetected;
                recognizer.SpeechEndDetected += OnSpeechEndDetected;

                await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
                Task.WaitAny(new[] { _stopRecognition.Task });

                await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

                _isRecording = true;
                //Debug.Log("SpeechRecognitionSystem: Starting continuous recognition");

                //_recognizer = recognizer;
            }
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
            //Debug.Log("[HELLO] recognizing speech: " + e.Result.Text);
        }

        void OnSpeechSessionStopped(object sender, SessionEventArgs e)
        {
           // Debug.Log("[HELLO] session stopped");
            _stopRecognition.TrySetResult(0);
        }

        void OnSpeechSessionCancelled(object sender, SpeechRecognitionEventArgs e)
        {
            _stopRecognition.TrySetResult(0);
        }

        private void OnSpeechEndDetected(object sender, RecognitionEventArgs e)
        {
            //Debug.Log("[HELLO] speech end detected: ");
        }
    }
}