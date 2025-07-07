using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Base_Framework._EndlessJumper
{
    public class _ScoreManager : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#9CF7FF><b>[_ScoreManager]</b></color> {0}";

        protected static _ScoreManager _instance;
        public static _ScoreManager Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected int _score;
        public int Score
        {
            get
            {
                return _score;
            }
            set
            {
                _score = value;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected int _coins = 0;
        public int Coins
        {
            get
            {
                return _coins;
            }
            set
            {
                _coins = value;
            }
        }

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            if (Instance == null)
            {
                Instance = this;

                StartCoroutine(PostAwake());
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this);
                return;
            }
        }

        protected virtual IEnumerator PostAwake()
        {
            while (_GameManager.Instance == null)
            {
                yield return null;
            }

            _GameManager.Instance.OnStateChanged += OnGameStateChanged;
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            _GameManager.Instance.OnStateChanged -= OnGameStateChanged;

            Instance = null;
        }

        protected virtual void OnGameStateChanged(_GameManager.GameStateEx state)
        {
            // throw new System.NotImplementedException();
            Debug.LogWarningFormat(LOG_FORMAT, "OnGameStateChanged(), state : " + state);

            if (state == _GameManager.GameStateEx.Win ||
                state == _GameManager.GameStateEx.Lose)
            {
                //Scoring - if a score already exists that is less than current, make it the best score
                if (PlayerPrefs.HasKey("BestScore"))
                {
                    if (PlayerPrefs.GetInt("BestScore") < Score)
                    {
                        PlayerPrefs.SetInt("BestScore", Score);

                    }
                }
                else
                {
                    PlayerPrefs.SetInt("BestScore", Score);
                }

#if false
                if (PlayerPrefs.GetInt("isVibrationOn", 1) == 1)
                {
#if UNITY_ANDROID || UNITY_IOS
					Handheld.Vibrate();
#endif
                }
#endif

                SubmitScore(Score, "Leaderboard");
                PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins", 100) + Coins);
            }
        }

        protected void SubmitScore(long score, string leaderboardID)
        {
            Debug.LogFormat(LOG_FORMAT, "Reporting score " + score + " on leaderboard " + leaderboardID);
            /*
            Social.ReportScore(score, leaderboardID, success =>
            {
                Debug.Log(success ? "Reported score successfully" : "Failed to report score");
            });
            */
        }
    }
}