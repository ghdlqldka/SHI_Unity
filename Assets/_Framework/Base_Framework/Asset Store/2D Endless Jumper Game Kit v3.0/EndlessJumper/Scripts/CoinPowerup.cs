using UnityEngine;
using System.Collections;

namespace EndlessJumper
{

	public class CoinPowerup : MonoBehaviour
	{



		protected virtual void OnTriggerEnter2D(Collider2D col)
		{
			if (col.tag == "Player")
			{
				GameManager.Instance.coins += 1;
				GameManager.Instance.AddInactiveCoin(this.gameObject);
				SoundManager.Instance.PlaySFX(7);
				GameManager.Instance.ingamePanel.UpdateCoins();
			}
		}

	}
}