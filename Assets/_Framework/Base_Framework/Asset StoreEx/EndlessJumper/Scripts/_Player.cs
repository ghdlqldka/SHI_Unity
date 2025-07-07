using UnityEngine;
using System.Collections;

namespace _Base_Framework._EndlessJumper
{
	public class _Player : EndlessJumper.Player
	{
		private static string LOG_FORMAT = "<color=#94B530><b>[_Player]</b></color> {0}";

		[SerializeField]
		public AudioClip _jumpClip;

		protected Rigidbody2D _rigidbody2D;

		protected virtual void Awake()
		{
			isMouseControl = false; // Not used!!!!!

			Game = null; // Not used
			SFXManager = null; // Not used

			_rigidbody2D = this.gameObject.GetComponent<Rigidbody2D>();
			Debug.Assert(_rigidbody2D != null);
		}

		// Use this for initialization
		protected override void Start()
		{
			// base.Start();

			thisRenderer = this.GetComponent<SpriteRenderer>();

#if UNITY_IOS
			Application.targetFrameRate = 60;
#endif

			Vector3 playerSize = GetComponent<Renderer>().bounds.size;
			// Here is the definition of the boundary in world point
			float distance = (this.transform.position - _GameManager.Instance._Cam.transform.position).z;
			leftBorder = _GameManager.Instance._Cam.ViewportToWorldPoint(new Vector3(0, 0, distance)).x + (playerSize.x / 2);
			rightBorder = _GameManager.Instance._Cam.ViewportToWorldPoint(new Vector3(1, 0, distance)).x - (playerSize.x / 2);

			timeDirection = Time.time;
			thisCharacterScaleLeft = this.transform.localScale;
			thisCharacterScaleRight = new Vector3(-this.transform.localScale.x, this.transform.localScale.y, this.transform.localScale.z);

		}

		protected override void Update()
		{
			if (_GameManager.Instance._State == _GameManager.GameStateEx.Playing)
			{
				if (Input.GetKeyDown("escape")) //If escape is pressed, mouse is released
				{
					_GameManager.Instance.EndGame();
					Cursor.visible = true;
				}

				//Get mouse position
				mousePosition = Input.mousePosition;
				mousePosition = _GameManager.Instance._Cam.ScreenToWorldPoint(mousePosition);
				mousePosition.y = 0f;
#if UNITY_EDITOR || UNITY_WEBPLAYER
				Vector3 diff;

				Debug.Assert(isMouseControl == false);
				/*
				if (isMouseControl == true)
				{
					//If Mouse control is being used, the player follows the mouse
					diff = Vector3.MoveTowards(this.transform.localPosition, mousePosition, (0.1f * Time.time));
				}
				else
				*/
				{
					//Keyboard Control - use arrow keys to move the player
					Vector3 acc = Vector3.zero;
					if (Input.GetKey(KeyCode.LeftArrow))
					{
						acc.x = -0.1f;
						this.gameObject.transform.localScale = thisCharacterScaleLeft;
					}
					if (Input.GetKey(KeyCode.RightArrow))
					{
						acc.x = 0.1f;
						this.gameObject.transform.localScale = thisCharacterScaleRight;
					}

					Vector3 target = this.transform.localPosition + acc;
					float maxDistanceDelta = 0.5f * Time.time;
					diff = Vector3.MoveTowards(this.transform.localPosition, target, maxDistanceDelta);

				}
#else

			//Accelerometer Control - default for mobile builds

			Vector3 acc = Input.acceleration;
			acc.y = 0f;
			acc.z = 0f;
			Vector3 diff =  Vector3.MoveTowards(this.transform.localPosition,this.transform.localPosition + acc,(0.5f * Time.time));
#endif

				diff.y = this.transform.localPosition.y;
				diff.z = 0f;

				if (diff.x < leftBorder - 1f)
				{
					diff.x = rightBorder;
				}
				else if (diff.x > rightBorder + 1f)
				{
					diff.x = leftBorder;
				}

				this.transform.localPosition = diff;
			}
		}

		public override void jump(float x)
		{
			throw new System.NotSupportedException("");
		}

		public virtual void Jump(float x)
		{
			Debug.LogFormat(LOG_FORMAT, "Jump(), x : " + x + ", GameManagerEx.Instance._State : " + _GameManager.Instance._State);
			if (_GameManager.Instance._State == _GameManager.GameStateEx.Playing)
			{
				//Jump (12*x) force - change force for lower jump or change tile jump in the tile.cs script
				_rigidbody2D.linearVelocity = Vector2.zero;
				_rigidbody2D.AddForce(new Vector2(0f, 12f * x), ForceMode2D.Impulse);

				_FxManager.Instance.PlayOneShot(_jumpClip);
			}
		}

		protected override void OnTriggerEnter2D(Collider2D col)
		{
			Debug.LogFormat(LOG_FORMAT, "OnTriggerEnter2D(), col : " + col.name);

			if (col.name.Contains("platform"))
			{
				//Jump if the object is platform
				Jump(1);
			}
			else if (_rigidbody2D.linearVelocity.y <= 0)
			{
				if (col.gameObject.name.Contains("Floor"))
				{
					//If the player hits the floor object, then it means he has fallen - end game
					_GameManager.Instance.EndGame();
				}
			}
		}


	}
}