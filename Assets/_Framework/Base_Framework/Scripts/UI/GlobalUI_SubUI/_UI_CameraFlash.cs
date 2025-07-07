using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// From "Draw On Screen" Asset

namespace _Base_Framework
{

	/// <summary>
	/// Camera flash.
	/// </summary>
	public class _UI_CameraFlash : MonoBehaviour
	{
		private Color flashColor;
		public static bool isRunning = false;
		private Image flashImage;

		protected void Awake()
		{
			flashImage = GetComponent<Image>();
			flashImage.enabled = false;
		}

		protected void OnEnable()
		{
			isRunning = false;

			flashColor = new Color(1, 1, 1, 0);
			flashImage.color = flashColor;
		}

		public void Flash()
		{
			if (isRunning == true)
				return;

			StartCoroutine(ShowFlash());
		}

		protected IEnumerator ShowFlash()
		{
			isRunning = true;
			flashImage.enabled = true;

			flashColor = new Color(1, 1, 1, 1);
			float targetAlhpa = 0;
			float alphaFraction = 0.04f;
			float delayTime = 0.01f;

			while (flashColor.a > targetAlhpa)
			{
				flashColor.a -= alphaFraction;
				flashImage.color = flashColor;
				yield return new WaitForSeconds(delayTime);
			}

			flashColor.a = 0;
			flashImage.color = flashColor;

			flashImage.enabled = false;
			isRunning = false;
		}
	}
}