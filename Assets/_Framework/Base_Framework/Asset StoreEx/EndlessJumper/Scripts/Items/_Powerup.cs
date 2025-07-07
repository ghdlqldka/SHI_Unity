using UnityEngine;
using System.Collections;

namespace _Base_Framework._EndlessJumper
{
	public class _Powerup : EndlessJumper.JumpPowerup
    {
		private static string LOG_FORMAT = "<color=#88D5F6><b>[_Powerup]</b></color> {0}";

		[SerializeField]
		protected AudioClip _audioClip;

		protected virtual void Awake()
		{
			Game = null; // Not used
			SFXManager = null; // Not used!!!!!

			mySpriteRenderer = this.GetComponent<SpriteRenderer>();
			Debug.Assert(mySpriteRenderer != null);
		}

		protected override void OnEnable()
		{

			//Assign the type - which item is it (0 or 1) - 0 = small jump - 1 = large jump


			string typeOfPowerup = this.name;


			itemType = int.Parse(typeOfPowerup);
			Debug.LogFormat(LOG_FORMAT, "Item type is " + itemType);

			mySpriteRenderer.sprite = mySprites[itemType];
		}

		protected override void Update()
		{
			if (jump == true)
			{
				//Jump - move player object at a constact speed upwards
				player.transform.Translate(new Vector2(0, 12 * Time.deltaTime));
			}

			else if (_GameManager.Instance.floor.transform.position.y > this.transform.position.y + 1)
			{
				_GameManager.Instance.AddInactiveItem(this.gameObject);
			}
		}

		protected override void OnTriggerEnter2D(Collider2D col)
		{
			if (col.name.Contains("Player"))
			{
				if (itemType == 2)//Item type is 2 = coin
				{
					//Add to score
					// GameManagerEx.Instance.score += 20;
					_ScoreManager.Instance.Score += 20;
					_GameManager.Instance.AddInactiveItem(this.gameObject);
				}
				else
				{
					_FxManager.Instance.PlayOneShot(_audioClip);

					player = col.gameObject;//Assign player
					player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;//Cancel out downwards force of the player
					jump = true; //Make jump var true 
#pragma warning disable 0618
                    player.GetComponent<Rigidbody2D>().isKinematic = true; //disable gravity
#pragma warning restore 0618
                    player.GetComponent<Collider2D>().enabled = false; //disable collider while using powerup jump
					myObjects[itemType].SetActive(true); //make the jump sprite active
					StartCoroutine(StopJumping()); //stop jump after certain seconds - depending on the type - edit freely
					mySpriteRenderer.enabled = false;
				}
			}
		}

		protected override IEnumerator StopJumping()
		{
			//Stop jumping - reset state
			yield return new WaitForSeconds(itemPower[itemType]);

#pragma warning disable 0618
            player.GetComponent<Rigidbody2D>().isKinematic = false;
#pragma warning restore 0618
            player.GetComponent<Collider2D>().enabled = true;

			myObjects[itemType].SetActive(false);
			jump = false;

			_FxManager.Instance.Stop();


		}
	}
}