using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessJumper
{

	public class UI_MainPanel : MonoBehaviour
	{

		public Sprite spriteSoundOn, spriteSoundOff, spriteVibrateOn, spriteVibrateOff;

		public Button buttonSound, buttonVibrate;

		public Text textCoins;
		// Use this for initialization
		void Start()
		{

		}

		// Update is called once per frame
		void OnEnable()
		{
			UpdateStates();
			UpdateCoins();
		}

		void UpdateCoins()
		{
			textCoins.text = PlayerPrefs.GetInt("Coins", 100).ToString();
		}

		void UpdateStates()
		{
			if (PlayerPrefs.GetInt("isSoundOn", 1) == 1)
				buttonSound.image.sprite = spriteSoundOn;
			else
				buttonSound.image.sprite = spriteSoundOff;

			if (PlayerPrefs.GetInt("isVibrationOn", 1) == 1)
				buttonVibrate.image.sprite = spriteVibrateOn;
			else
				buttonVibrate.image.sprite = spriteVibrateOff;
		}

		public void ButtonToggleSound()
		{
			if (PlayerPrefs.GetInt("isSoundOn", 1) == 1)
				PlayerPrefs.SetInt("isSoundOn", 0);
			else
				PlayerPrefs.SetInt("isSoundOn", 1);

			UpdateStates();
			SoundManager.Instance.PlayButtonTapSound();
		}

		public void ButtonToggleVibration()
		{
			if (PlayerPrefs.GetInt("isVibrationOn", 1) == 1)
				PlayerPrefs.SetInt("isVibrationOn", 0);
			else
				PlayerPrefs.SetInt("isVibrationOn", 1);

			UpdateStates();
			SoundManager.Instance.PlayButtonTapSound();
		}

		public void ButtonSkins()
		{
			GUIManager.Instance.OpenPanel(4);
			SoundManager.Instance.PlayButtonTapSound();
		}

		public void ButtonPlay()
		{
			GUIManager.Instance.OpenPanel(1, true);
			GameManager.Instance.ResumeGame();
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