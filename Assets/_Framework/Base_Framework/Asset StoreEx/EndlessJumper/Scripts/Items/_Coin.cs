using UnityEngine;
using System.Collections;

namespace _Base_Framework._EndlessJumper
{

	public class _Coin : EndlessJumper.CoinPowerup
    {
		[SerializeField]
		public AudioClip _audioClip;

		protected override void OnTriggerEnter2D(Collider2D col)
		{
			if (col.tag == "Player")
			{
				// GameManagerEx.Instance.coins += 1;
				_ScoreManager.Instance.Coins += 1;
				_GameManager.Instance.AddInactiveCoin(this.gameObject);
				_FxManager.Instance.PlayOneShot(_audioClip);

				// GameManagerEx.Instance.ingamePanel.UpdateCoins();
			}
		}
	}
}