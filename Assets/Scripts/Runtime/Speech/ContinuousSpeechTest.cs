using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Threading.Tasks;
using System;
using TMPro;

public class ContinuousSpeechTest : MonoBehaviour
{
    [SerializeField] TMP_Text _outputText;
    TaskCompletionSource<int> _stopRecognition;
    //SpeechRecognizer _recognizer;

    bool _isRecording;

    // Start is called before the first frame update
    void Start()
    {
        StartContinuousRecognition();
        _isRecording = true;

    }

    async void StartContinuousRecognition()
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

            //_recognizer = recognizer;
        }
    }

    /*async void StopContinuousRecognition()
    {
        var 
        await _recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
        Debug.Log("[HELLO] STOPPED RECOGNITION");
    }*/

    

    void OnSpeechRecognized(object sender, SpeechRecognitionEventArgs e)
    {
        Debug.Log("[HELLO] recognized speech: " + e.Result.Text);
        _outputText.text = e.Result.Text;
        Canvas.ForceUpdateCanvases();
    }

    void OnSpeechRecognizing(object sender, SpeechRecognitionEventArgs e)
    {
        Debug.Log("[HELLO] recognizing speech: " + e.Result.Text);
    }

    void OnSpeechSessionStopped(object sender, SessionEventArgs e)
    {
        Debug.Log("[HELLO] session stopped");
        _stopRecognition.TrySetResult(0);
    }

    void OnSpeechSessionCancelled(object sender, SpeechRecognitionEventArgs e)
    {
        _stopRecognition.TrySetResult(0);
    }

    private void OnSpeechEndDetected(object sender, RecognitionEventArgs e)
    {
        Debug.Log("[HELLO] speech end detected: ");
    }

    private void Update()
    {
        /*if(_isRecording)
        {
            if (!Application.isPlaying)
            {
                StopContinuousRecognition();
            }
        }*/
        
    }

    private void OnApplicationQuit()
    {
        /*if(_isRecording)
        {
            StopContinuousRecognition();
        }*/
    }
}
