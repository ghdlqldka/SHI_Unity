using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using EndlessJumper;

namespace _Base_Framework._EndlessJumper
{

    public class _UI_Handler : GUIManager
    {
		private static string LOG_FORMAT = "<color=#94B530><b>[_UI_Handler]</b></color> {0}";

		[SerializeField]
		protected UI_Ingame ingamePanel;

		protected int _coins;
		protected override void Awake()
		{
#pragma warning disable 0618
            if (GameObject.FindObjectsOfType<GUIManager>().Length > 1)
#pragma warning restore 0618
            {
                Destroy(this.gameObject);
				return;
			}

			_instance = this;

#if UNITY_IOS
			Application.targetFrameRate = 60;
#endif
            _GameManager.Instance.OnStateChanged += OnGameStateChanged;
		}

		protected override void OnEnable()
		{
			_coins = _ScoreManager.Instance.Coins;
			OpenPanel(defaultPanel);
		}

		protected virtual void OnDestroy()
		{
			_GameManager.Instance.OnStateChanged -= OnGameStateChanged;
		}

		protected virtual void FixedUpdate()
		{
			if (_ScoreManager.Instance != null)
			{
				ingamePanel.textScore.text = _ScoreManager.Instance.Score.ToString();
				if (_coins != _ScoreManager.Instance.Coins)
				{
					_coins = _ScoreManager.Instance.Coins;
					ingamePanel.UpdateCoins();
				}
			}
		}

		protected virtual void OnGameStateChanged(_GameManager.GameStateEx state)
        {
			Debug.LogFormat(LOG_FORMAT, "OnGameStateChanged(), state : " + state);

			// throw new NotImplementedException();
			if (state == _GameManager.GameStateEx.Lose)
			{
				ingamePanel.ShowFlash();
				StartCoroutine(PostOnGameLose());
			}
        }

		protected IEnumerator PostOnGameLose()
		{
			yield return new WaitForSeconds(1);
			OpenPanel(3, true); //Use the GUI Manager script to show the game over screen panel
		}

		public override void OpenPanel(int id, bool hidePrevious = false)
		{
			Debug.LogFormat(LOG_FORMAT, "OpenPanel(), id : " + id);
			if (hidePrevious)
			{
				if (panelStack.Peek() != null)
				{
					currentPanelObject.SetActive(false);
					panelStack.Pop();
				}
				panelStack.Push(panels[id]);
				currentPanelObject.SetActive(true);
			}
			else
			{
				panelStack.Push(panels[id]);
				currentPanelObject.SetActive(true);
			}
		}
	}

}