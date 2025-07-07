using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace UnityTimer.Examples
{
    public class __Test_Timer : TestTimerBehaviour
    {
        private static string LOG_FORMAT = "<color=white><b>[__Test_Timer]</b></color> {0}";

        // [ReadOnly]
        // [SerializeField]
        protected __Timer testTimer
        {
            get
            {
                return _testTimer as __Timer;
            }
        }

        // public Button StartTimerButton;
        [SerializeField]
        protected Button createTimerButton;

        // Toggle IsLoopedToggle;
        protected Toggle loopToggle
        {
            get
            {
                return IsLoopedToggle;
            }
        }

        // public Toggle UseGameTimeToggle;
        protected Toggle useRealTimeToggle
        {
            get
            {
                return UseGameTimeToggle;
            }
        }

        [SerializeField]
        protected Text statusText;

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            IsCancelledText = null; // Not used!!!!!
            IsCompletedText = null; // Not used!!!!!
            IsPausedText = null; // Not used!!!!!
            IsDoneText = null; // Not used!!!!!

            Time.timeScale = TimescaleSlider.value;
            Debug.LogFormat(LOG_FORMAT, "Time.timeScale : " + Time.timeScale);

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
                StartTimerButton.interactable = (testTimer._Status == __Timer.Status.Ready || testTimer._Status == __Timer.Status.Completed);
                PauseTimerButton.interactable = testTimer._Status == __Timer.Status.Running;
                // ResumeTimerButton.interactable = testTimer.isPaused;
                ResumeTimerButton.interactable = testTimer._Status == __Timer.Status.Paused;

                NeedsRestartText.gameObject.SetActive(ShouldShowRestartText());
            }
        }

        protected override void ResetState()
        {
            _numLoops = 0;
            OnClickCancelTimerButton(); //
        }

        public override void StartTestTimer()
        {
            throw new System.NotSupportedException("");
        }

        public virtual void OnClickCreateTimerButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickCreateTimerButton()");

            ResetState();

            float duration = GetDurationValue();
            // this is the important code example bit where we register a new timer
            _testTimer = new __Timer(duration, OnTimerCompleted, OnTimerUpdated, loopToggle.isOn, !useRealTimeToggle.isOn);
            __TimerManager.Instance.RegisterTimer(testTimer);
            // testTimer.Play();

            CancelTimerButton.interactable = true;

            createTimerButton.interactable = false;
        }

        public virtual void OnClickStartTimerButton()
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

        public override void CancelTestTimer()
        {
            throw new System.NotSupportedException("");
        }

        public virtual void OnClickCancelTimerButton()
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

        public override void PauseTestTimer()
        {
            throw new System.NotSupportedException("");
        }

        public virtual void OnClickPauseTimerButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickPauseTimerButton(), testTimer : " + testTimer);

            testTimer.Pause();
        }

        public override void ResumeTestTimer()
        {
            throw new System.NotSupportedException("");
        }

        public virtual void OnClickResumeTimerButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickResumeTimerButton(), testTimer : " + testTimer);

            testTimer.Resume();
        }

        public virtual void OnValueChangedTimescaleSlider()
        {
            Time.timeScale = TimescaleSlider.value;
            Debug.LogFormat(LOG_FORMAT, "Time.timeScale : " + Time.timeScale);
        }

        protected virtual void OnTimerCompleted(Guid guid)
        {
            Debug.LogFormat(LOG_FORMAT, "OnTimerCompleted(), guid : <b><color=yellow>" + guid + "</color></b>");

            _numLoops++;
        }

        protected virtual void OnTimerUpdated(Guid guid, float timeElapsed)
        {
            // Debug.LogFormat(LOG_FORMAT, "OnTimerUpdated(), timeElapsed : " + timeElapsed);

            UpdateText.text = string.Format("Timer ran update callback: <b><color=red>{0:F2}</color></b> seconds", timeElapsed);
        }

        protected override bool ShouldShowRestartText()
        {
            return (testTimer._Status != __Timer.Status.Completed) && // the timer is in progress and...
                   ((UseGameTimeToggle.isOn && testTimer.UsesRealTime) || // we switched to real-time or
                    (!UseGameTimeToggle.isOn && !testTimer.UsesRealTime) || // we switched to game-time or
                    Mathf.Abs(GetDurationValue() - testTimer.duration) >= Mathf.Epsilon); // our duration changed
        }
    }
}