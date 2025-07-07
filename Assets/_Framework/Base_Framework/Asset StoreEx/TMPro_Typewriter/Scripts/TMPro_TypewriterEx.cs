using DG.Tweening;
using KoganeUnityLib;
using System;
using TMPro;
using UnityEngine;

namespace _Base_Framework
{
	/// <summary>
	/// TextMesh Pro
	/// </summary>
	[RequireComponent(typeof(TMP_Text))]
	public class TMPro_TypewriterEx : TMP_Typewriter
	{
		private static string LOG_FORMAT = "<color=#00AEEF><b>[TMPro_TypewriterEx]</b></color> {0}";

		[ReadOnly]
		[SerializeField]
		protected bool isPlaying = false;
		public bool IsPlaying
		{
			get
			{
				return isPlaying;
			}
		}

		protected virtual void Awake()
		{
			m_textUI = GetComponent<TMP_Text>();
		}

		protected virtual void OnEnable()
		{
			//
		}

		public override void Play(string text, float speed, Action onComplete)
		{
			Debug.LogFormat(LOG_FORMAT, "Play()");

			isPlaying = true;

			m_textUI.text = text;
			m_onComplete = onComplete;

			m_textUI.ForceMeshUpdate();

			m_parsedText = m_textUI.GetParsedText();

			int length = m_parsedText.Length;
			float duration = 1 / speed * length;

			_OnUpdate(0);

			if (m_tween != null)
			{
				m_tween.Kill();
			}

			m_tween = DOTween
				.To(value => _OnUpdate(value), 0, 1, duration)
				.SetEase(Ease.Linear)
				.OnComplete(() => OnComplete());
		}

		public virtual void Stop()
		{
			if (m_tween != null)
			{
				m_tween.Kill();
			}

			m_tween = null;

			isPlaying = false;
		}

		public override void Skip(bool withCallbacks = true)
		{
			Debug.LogFormat(LOG_FORMAT, "Skip(), withCallbacks : " + withCallbacks);

			if (m_tween != null)
			{
				m_tween.Kill();
			}

			m_tween = null;

			isPlaying = false;

			_OnUpdate(1);

			if (withCallbacks == false)
			{
				return;
			}

			OnComplete(); // forcell-call
		}

		public override void Pause()
		{
			if (m_tween != null)
			{
				m_tween.Pause();
			}
		}

		public override void Resume()
		{
			if (m_tween != null)
			{
				m_tween.Play();
			}
		}

		protected virtual void _OnUpdate(float value)
		{
			float current = Mathf.Lerp(0, m_parsedText.Length, value);
			int count = Mathf.FloorToInt(current);

			m_textUI.maxVisibleCharacters = count;
		}

		protected override void OnComplete()
		{
			m_tween = null;

			if (m_onComplete != null)
			{
				m_onComplete.Invoke();
			}

			m_onComplete = null;

			isPlaying = false;
		}
	}
}