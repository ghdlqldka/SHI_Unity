using UnityEngine;
using System.Collections;

namespace _Base_Framework._EndlessJumper
{
	public class _Tile : EndlessJumper.Tile
	{
		private static string LOG_FORMAT = "<color=##F3940F><b>[_Tile]</b></color> {0}";

		public enum Type
		{
			None,

			Normal,
			Broken,
			OneTime,
			Spring,
			MovingHorizontally,
			MovingVertically,
		}

		[SerializeField]
		protected Type _type;
		public Type _Type
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
			}
		}

		[SerializeField]
		protected AudioClip brokenClip;

		protected Rigidbody2D _rigidbody2D;

		protected virtual void Awake()
		{
			mySpriteRenderer = this.GetComponent<SpriteRenderer>();
			_rigidbody2D = this.GetComponent<Rigidbody2D>();

			Game = null; // Not used!!!!!
			tileType = -1; // Not used!!!!!
			SFXManager = null; // Not used!!!!!
		}

		protected override void OnEnable()
		{
			switch (_Type)
			{
				case Type.Normal:
					mySpriteRenderer.sprite = mySprites[0];
					break;
				case Type.Broken:
					mySpriteRenderer.sprite = mySprites[1];
					break;
				case Type.OneTime:
					mySpriteRenderer.sprite = mySprites[2];
					break;
				case Type.Spring:
					mySpriteRenderer.sprite = mySprites[3];
					break;
				case Type.MovingHorizontally:
					mySpriteRenderer.sprite = mySprites[4];
					startPosition = this.transform.position;
					distance = _SettingsManager.Instance.Settings.movingTilesHorizontal.distance;
					speed = _SettingsManager.Instance.Settings.movingTilesHorizontal.speed;
					break;
				case Type.MovingVertically:
					mySpriteRenderer.sprite = mySprites[5];
					startPosition = this.transform.position;
					distance = _SettingsManager.Instance.Settings.movingTilesVertical.distance;
					speed = _SettingsManager.Instance.Settings.movingTilesVertical.speed;
					break;

				default:
					Debug.Assert(false);
					break;
			}
		}

		protected override void Update()
		{
			switch (_Type)
			{
				case Type.Normal:
					break;
				case Type.Broken:
					break;
				case Type.OneTime:
					break;
				case Type.Spring:
					break;
				case Type.MovingHorizontally:
					if (direction == 0) //left
					{
						this.transform.Translate(new Vector2(-speed * Time.deltaTime, 0));
						if ((startPosition - this.transform.position).x > distance)
							direction = 1;
					}
					else if (direction == 1) //left
					{
						this.transform.Translate(new Vector2(speed * Time.deltaTime, 0));
						if ((startPosition - this.transform.position).x < -distance)
							direction = 0;
					}
					break;

				case Type.MovingVertically:
					if (direction == 0) //up
					{
						this.transform.Translate(new Vector2(0, -speed * Time.deltaTime));
						if ((startPosition - this.transform.position).y > distance)
							direction = 1;
					}
					else if (direction == 1) //down
					{
						this.transform.Translate(new Vector2(0, speed * Time.deltaTime));
						if ((startPosition - this.transform.position).y < -distance)
							direction = 0;
					}
					break;

				default:
					Debug.Assert(false);
					break;
			}

			if (_GameManager.Instance.floor.transform.position.y > this.transform.position.y + 1)
			{
				_rigidbody2D.gravityScale = 0;
				_GameManager.Instance.AddInactiveTile(this.gameObject);

			}

		}

		protected override void OnTriggerEnter2D(Collider2D col)
		{
			if (col.name.Contains("Player") == false)
			{
				return;
			}

			Debug.LogWarningFormat(LOG_FORMAT, "OnTriggerEnter2D(), _Type : " + _Type);

			Rigidbody2D playerRigidbody2D = col.gameObject.GetComponent<Rigidbody2D>();
			//(col.gameObject.rigidbody2D.velocity.y <= 0 ) Checks if the player is falling down only then the tile is in effect
			switch (_Type)
			{
				case Type.Normal:
					if (playerRigidbody2D.linearVelocity.y <= 0)
					{
						_GameManager.Instance._Player.Jump(1);
					}
					break;

				case Type.Broken:
					if (playerRigidbody2D.linearVelocity.y <= 0)
					{
						this.GetComponent<Rigidbody2D>().gravityScale = 1; //Make the tile fall down as soon as player touches it
						// SFXManager.PlaySFX(2);//Play Broken Sound
						_FxManager.Instance.PlayOneShot(brokenClip);
					}
					break;

				case Type.OneTime:
					if (playerRigidbody2D.linearVelocity.y <= 0)
					{
						// Game.player.jump(1);
						_GameManager.Instance._Player.Jump(1.0f);
						this.GetComponent<Rigidbody2D>().gravityScale = 1; //Make the tile fall down as soon as player touches it
					}
					break;

				case Type.Spring:

					if (playerRigidbody2D.linearVelocity.y <= 0)
					{
						// Game.player.jump(1.5f);
						_GameManager.Instance._Player.Jump(1.5f);
						//Big Jump, Vibrate

						if (PlayerPrefs.GetInt("isVibrationOn", 1) == 1)
						{
#if UNITY_ANDROID || UNITY_IOS
							Handheld.Vibrate();
#endif
						}
					}
					break;

				case Type.MovingHorizontally:
					if (playerRigidbody2D.linearVelocity.y <= 0)
					{
						// Game.player.jump(1);
						_GameManager.Instance._Player.Jump(1.0f);
					}
					break;

				case Type.MovingVertically:
					if (playerRigidbody2D.linearVelocity.y <= 0)
					{
						// Game.player.jump(1);
						_GameManager.Instance._Player.Jump(1.0f);
					}
					break;

				default:
					Debug.Assert(false);
					break;
			}

		}


	}
}