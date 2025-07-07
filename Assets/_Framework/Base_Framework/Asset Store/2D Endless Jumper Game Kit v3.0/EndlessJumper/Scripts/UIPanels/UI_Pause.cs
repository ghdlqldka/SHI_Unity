using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EndlessJumper
{

	public class UI_Pause : MonoBehaviour
	{

		// Use this for initialization
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

		public void ButtonResume()
		{
			Time.timeScale = 1;
			GUIManager.Instance.Back();
			SoundManager.Instance.PlayButtonTapSound();
		}
	}
}