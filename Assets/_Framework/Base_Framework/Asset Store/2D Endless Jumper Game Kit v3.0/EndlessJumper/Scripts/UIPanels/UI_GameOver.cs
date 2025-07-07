using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace EndlessJumper
{

	public class UI_GameOver : MonoBehaviour
	{

		public Text textScore;
		public Text textBest;

		// Use this for initialization
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

		void OnEnable()
		{
			textScore.text = GameManager.Instance.score.ToString();
			textBest.text = PlayerPrefs.GetInt("BestScore", 0).ToString();
		}

		public void ButtonRestart()
		{

			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			SoundManager.Instance.PlayButtonTapSound();
		}

		public void ButtonBack()
		{

			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			SoundManager.Instance.PlayButtonTapSound();
		}

		public void ButtonLeaderboard()
		{
			GUIManager.Instance.ButtonLeaderboard();
			SoundManager.Instance.PlayButtonTapSound();
		}

		public void ButtonRate()
		{
			GUIManager.Instance.ButtonRate();
			SoundManager.Instance.PlayButtonTapSound();
		}
	}
}