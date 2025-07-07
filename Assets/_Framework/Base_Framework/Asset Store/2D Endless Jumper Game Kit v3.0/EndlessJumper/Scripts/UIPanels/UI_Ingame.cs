using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessJumper
{

	public class UI_Ingame : MonoBehaviour
	{

		public Text textScore;
		public Image imageFlash;
		public Text textCoins;


		// Use this for initialization
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

		public void UpdateCoins()
		{
			textCoins.text = GameManager.Instance.coins.ToString();
		}

		public void ShowFlash()
		{
			imageFlash.gameObject.SetActive(true);
			StartCoroutine("HideFlash");
		}

		IEnumerator HideFlash()
		{
			yield return new WaitForSeconds(1.5f);
			imageFlash.gameObject.SetActive(false);
		}
		public void ButtonPause()
		{
			Time.timeScale = 0;
			GUIManager.Instance.OpenPanel(2);
			SoundManager.Instance.PlayButtonTapSound();
		}
	}
}