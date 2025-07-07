using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace _Base_Framework
{
    public class _Test_Timer : UnityTimer.Examples.__Test_Timer
    {
        private static string LOG_FORMAT = "<color=white><b>[_Test_Timer]</b></color> {0}";

        // [ReadOnly]
        // [SerializeField]
        protected new _Timer testTimer
        {
            get
            {
                return _testTimer as _Timer;
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            IsCancelledText = null; // Not used!!!!!
            IsCompletedText = null; // Not used!!!!!
            IsPausedText = null; // Not used!!!!!
            IsDoneText = null; // Not used!!!!!

            _GlobalObjectUtilities.timeScale = TimescaleSlider.value;
            Debug.LogFormat(LOG_FORMAT, "GlobalObjectUtilities.timeScale : " + _GlobalObjectUtilities.timeScale);

            ResetState();
        }

        protected override void Update()
        {
            if (testTimer == null)
            {
                TimeElapsedText.text = string.Format("Time elapsed: 0 seconds");
                TimeRemainingText.text = string.Format("Time remaining: 0 seconds");
                PercentageCompletedText.text = string.Format("Percentage completed: 0%");
                PercentageRemainingText.text = String.Format("Percentage remaining: 100%");
                NumberOfLoopsText.text = string.Format("# Loops: 0");

                statusText.text = "Status : None";

                StartTimerButton.interactable = false;
                PauseTimerButton.interactable = false;
                // ResumeTimerButton.interactable = testTimer.isPaused;
                ResumeTimerButton.interactable = false;

                NeedsRestartText.gameObject.SetActive(false);
            }
            else
            {
                // Time.timeScale = TimescaleSlider.value;
                testTimer.IsLoop = IsLoopedToggle.isOn;

                TimeElapsedText.text = string.Format("Time elapsed: <b><color=red>{0:F2}</color></b> seconds", testTimer.TimeElapsed);
                TimeRemainingText.text = string.Format("Time remaining: <b><color=red>{0:F2}</color></b> seconds", testTimer.TimeRemaining);
                PercentageCompletedText.text = string.Format("Percentage completed: <b><color=red>{0:F4}</color></b>%", testTimer.GetRatioComplete() * 100);
                PercentageRemainingText.text = String.Format("Percentage remaining: <b><color=red>{0:F4}</color></b>%", testTimer.GetRatioRemaining() * 100);
                NumberOfLoopsText.text = string.Format("# Loops: <b><color=red>{0}</color></b>", _numLoops);
                /*
                IsCancelledText.text = string.Format("Is Cancelled: <b><color=red>{0}</color></b>", testTimer.isCancelled);
                IsCompletedText.text = string.Format("Is Completed: <b><color=red>{0}</color></b>", testTimer.isCompleted);
                IsPausedText.text = String.Format("Is Paused: <b><color=red>{0}</color></b>", testTimer.isPaused);
                IsDoneText.text = string.Format("Is Done: <b><color=red>{0}</color></b>", testTimer.isDone);
                */
                statusText.text = "Status : <b><color=red>" + testTimer._Status + "</color></b>";

                // PauseTimerButton.interactable = !testTimer.isPaused;
                StartTimerButton.interactable = (testTimer._Status == _Timer.Status.Ready || testTimer._Status == _Timer.Status.Completed);
                PauseTimerButton.interactable = testTimer._Status == _Timer.Status.Running;
                // ResumeTimerButton.interactable = testTimer.isPaused;
                ResumeTimerButton.interactable = testTimer._Status == _Timer.Status.Paused;

                NeedsRestartText.gameObject.SetActive(ShouldShowRestartText());
            }
        }

        protected override void ResetState()
        {
            _numLoops = 0;
            OnClickCancelTimerButton(); //
        }

        public override void OnClickCreateTimerButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickCreateTimerButton()");

            ResetState();

            float duration = GetDurationValue();
            // this is the important code example bit where we register a new timer
            _testTimer = new _Timer(duration, OnTimerCompleted, OnTimerUpdated, loopToggle.isOn, !useRealTimeToggle.isOn);
            _TimerManager.Instance.RegisterTimer(testTimer);
            // testTimer.Play();

            CancelTimerButton.interactable = true;

            createTimerButton.interactable = false;
        }

        public override void OnClickCancelTimerButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickCancelTimerButton(), testTimer : " + testTimer);

            if (testTimer != null)
            {
                testTimer.Cancel();
                _testTimer = null;
            }

            createTimerButton.interactable = true;
            CancelTimerButton.interactable = false;
            StartTimerButton.interactable = false;
            PauseTimerButton.interactable = false;
            ResumeTimerButton.interactable = false;
            
            NeedsRestartText.gameObject.SetActive(false);
        }

        public override void OnClickStartTimerButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickStartTimerButton()");

            /*
            ResetState();

            float duration = GetDurationValue();
            // this is the important code example bit where we register a new timer
            _testTimer = new _Timer(duration, OnComplete, OnUpdate, loopToggle.isOn, !useRealTimeToggle.isOn);
            _TimerManager.Instance.RegisterTimer(testTimer);
            */
            testTimer.Play();

            // CancelTimerButton.interactable = true;
        }

        public override void OnClickPauseTimerButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickPauseTimerButton(), testTimer : " + testTimer);

            testTimer.Pause();
        }

        public override void OnClickResumeTimerButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickResumeTimerButton(), testTimer : " + testTimer);

            testTimer.Resume();
        }

        public override void OnValueChangedTimescaleSlider()
        {
            _GlobalObjectUtilities.timeScale = TimescaleSlider.value;
            Debug.LogFormat(LOG_FORMAT, "GlobalObjectUtilities.timeScale : " + _GlobalObjectUtilities.timeScale);
        }

        protected override void OnTimerCompleted(Guid guid)
        {
            Debug.LogFormat(LOG_FORMAT, "OnTimerCompleted(), guid : <b><color=yellow>" + guid + "</color></b>");

            _numLoops++;
        }

        protected override void OnTimerUpdated(Guid guid, float timeElapsed)
        {
            // Debug.LogFormat(LOG_FORMAT, "OnTimerUpdated(), timeElapsed : " + timeElapsed);

            UpdateText.text = string.Format("Timer ran update callback: <b><color=red>{0:F2}</color></b> seconds", timeElapsed);
        }

        protected override bool ShouldShowRestartText()
        {
            return (testTimer._Status != _Timer.Status.Completed) && // the timer is in progress and...
                   ((UseGameTimeToggle.isOn && testTimer.UsesRealTime) || // we switched to real-time or
                    (!UseGameTimeToggle.isOn && !testTimer.UsesRealTime) || // we switched to game-time or
                    Mathf.Abs(GetDurationValue() - testTimer.duration) >= Mathf.Epsilon); // our duration changed
        }
    }
}