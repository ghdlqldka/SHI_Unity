using UnityEngine;
using System.Collections;

namespace EndlessJumper
{

	public class Player : MonoBehaviour
	{

		public bool isMouseControl;
		public GameManager Game;
		public GameObject jumpBlue;
		public GameObject jumpOrange;


		public SoundManager SFXManager;
		protected float leftBorder;
		protected float rightBorder;

		int currentDirection;
		protected float timeDirection;

		protected Vector2 thisCharacterScaleLeft = new Vector2(1, 1);
		protected Vector2 thisCharacterScaleRight = new Vector2(-1, 1);
		protected SpriteRenderer thisRenderer;
		// Use this for initialization
		protected virtual void Start()
		{


			thisRenderer = this.GetComponent<SpriteRenderer>();

#if UNITY_IOS
			Application.targetFrameRate = 60;
#endif

			Vector3 playerSize = GetComponent<Renderer>().bounds.size;
			// Here is the definition of the boundary in world point
			float distance = (transform.position - Camera.main.transform.position).z;
			leftBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, distance)).x + (playerSize.x / 2);
			rightBorder = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, distance)).x - (playerSize.x / 2);
			timeDirection = Time.time;
			thisCharacterScaleLeft = this.transform.localScale;
			thisCharacterScaleRight = new Vector3(-this.transform.localScale.x, this.transform.localScale.y, this.transform.localScale.z);

		}





		protected Vector3 mousePosition;
		// Update is called once per frame
		protected virtual void Update()
		{

			if (Game.currentState == GameManager.GameState.RUNNING)
			{
				if (Input.GetKeyDown("escape")) //If escape is pressed, mouse is released
				{
					Game.EndGame();
					Cursor.visible = true;
				}

				//Get mouse position
				mousePosition = Input.mousePosition;
				mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
				mousePosition.y = 0f;
#if UNITY_EDITOR || UNITY_WEBPLAYER
				Vector3 diff;
				if (isMouseControl)
				{
					//If Mouse control is being used, the player follows the mouse
					diff = Vector3.MoveTowards(this.transform.localPosition, mousePosition, (0.1f * Time.time));
				}
				else
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



					diff = Vector3.MoveTowards(this.transform.localPosition, this.transform.localPosition + acc, (0.5f * Time.time));

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

		public void switchOn(string name)
		{
			if (name.Contains("j2")) //Switch on the jump sprite behind the player
				jumpBlue.SetActive(true);
			else
				jumpOrange.SetActive(true);
		}
		public void switchOff(string name)
		{
			if (name.Contains("j2"))//Switch off the jump sprite behind the player
				jumpBlue.SetActive(false);
			else
				jumpOrange.SetActive(false);
		}

		public virtual void jump(float x)
		{
			if (GameManager.Instance.currentState == GameManager.GameState.RUNNING)
			{
				//Jump (12*x) force - change force for lower jump or change tile jump in the tile.cs script
				this.gameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
				this.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, 12f * x), ForceMode2D.Impulse);

				SFXManager.PlaySFX(0);//Play Jump Sound
			}
		}



		protected virtual void OnTriggerEnter2D(Collider2D col)
		{

			if (col.name.Contains("platform"))
			{
				//Jump if the object is platform
				Game.player.jump(1);
			}

			else if (this.gameObject.GetComponent<Rigidbody2D>().linearVelocity.y <= 0)
			{
				if (col.gameObject.name.Contains("Floor"))
				{
					//If the player hits the floor object, then it means he has fallen - end game
					Game.EndGame();
				}
			}
		}
	}
}