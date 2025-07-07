using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace EndlessJumper
{

	public class GameManager : MonoBehaviour
	{

		public Player player;
		public float ScreenDistance;
		public int score;
		public int coins = 0;
		public GameObject floor;
		public GameSettings Advanced;
		protected int initialSize = 40;
		protected float currentPosition = 2f;
		protected float totalWeight;
		protected float lastTime;

		public GameObject defaultTile;
		public GameObject defaultItem;
		public GameObject defaultEnemy;
		public GameObject defaultCoin;


		protected Vector3 initCameraPosition;
		protected Queue<GameObject> tilePool = new Queue<GameObject>();
		protected Queue<GameObject> enemyPool = new Queue<GameObject>();
		protected Queue<GameObject> itemPool = new Queue<GameObject>();
		protected Queue<GameObject> coinPool = new Queue<GameObject>();

		public UI_Ingame ingamePanel;

		public enum GameState
		{
			PAUSED, RUNNING, GAMEOVER
		}

		public GameState currentState = GameState.PAUSED;

		protected static GameManager _instance;
		public static GameManager Instance
		{
			get
			{
				return _instance;
			}
		}

		protected virtual void Awake()
		{
#pragma warning disable 0618
            if (GameObject.FindObjectsOfType<GameManager>().Length > 1)
#pragma warning restore 0618
            {
                Destroy(this.gameObject);
				return;
			}

			_instance = this;
		}

		protected virtual void FixedUpdate()
		{
			ingamePanel.textScore.text = score.ToString();
		}


		// Use this for initialization
		protected virtual void Start()
		{

			Cursor.visible = false;
			GetTotalProbabilityWeight();
			GenerateTilePool(); //Generate atleast 10-20 tiles at first and resuse them (pooling)
			GenerateEnemyPool(); //Generate atleast 10-20 enemies at first and resuse them (pooling)
			GenerateItemPool(); //Generate atleast 10-20 items at first and resuse them (pooling)
			GenerateCoinPool(); //Generate atleast 10-20 items at first and resuse them (pooling)

			Debug.Log("Total enemies " + enemyPool.Count + " " + " Total items : " + itemPool.Count);

			for (int i = 0; i < initialSize; i++)
			{
				GenerateTile(); //Use a tile from the pool and add to scene (make it visible at the right position)
				GenerateItem(); //Use an item from the pool and add to scene (make it visible at the right position)
				GenerateEnemy(); //Use a enemy from the pool and add to scene (make it visible at the right position)
				GenerateCoin();
			}

			PauseGame(); //Pause game at the start to show the start panel
			initCameraPosition = Camera.main.transform.position; //Store the initial position of the camera - reset it at game over

			SelectSkin();

			//AuthenticateGameCenter (); //Uncomment for GameCenter
		}

		protected void SelectSkin()
		{
			player.GetComponent<SpriteRenderer>().sprite = SkinsManager.Instance.skins[PlayerPrefs.GetInt("selectedSkin", 0)].spriteCharacter;
		}


		public virtual void PauseGame()
		{
			currentState = GameState.PAUSED; //Pause the game (using one variable)
		}

		public virtual void ResumeGame()
		{
			SelectSkin();
			player.gameObject.SetActive(true); //Resume game - make the player object active
			currentState = GameState.RUNNING; //Resume game - make the variable false
		}

		public virtual void EndGame()
		{
			if (currentState != GameState.GAMEOVER)
			{
				//Camera.main.GetComponent<PlayerCamera>().enabled = false; //Disable the script that moves the camera
				currentState = GameState.GAMEOVER; //Make game over using one variable
				ingamePanel.ShowFlash();
				StartCoroutine("OpenGameOverScreen");
				//Camera.main.transform.position = initCameraPosition; //reset camera to initial/original position to show game over screen


				//Scoring - if a score already exists that is less than current, make it the best score
				if (PlayerPrefs.HasKey("BestScore"))
				{
					if (PlayerPrefs.GetInt("BestScore") < score)
					{
						PlayerPrefs.SetInt("BestScore", score);

					}
				}
				else
				{
					PlayerPrefs.SetInt("BestScore", score);
				}

				Debug.LogError("Check below code");
				// iTween.ShakePosition(Camera.main.gameObject, new Vector3(0.7f, 0.7f, 0f), 0.5f);
				MusicManager.instance.isGameOver = true;

				if (PlayerPrefs.GetInt("isVibrationOn", 1) == 1)
				{
#if UNITY_ANDROID || UNITY_IOS
					Handheld.Vibrate();
#endif
				}

				SubmitScore(score, "Leaderboard");
				PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins", 100) + coins);
			}


		}

		void AuthenticateGameCenter()
		{
#pragma warning disable 0618
            Social.localUser.Authenticate(success =>
			{
				if (success)
				{
					Debug.Log("Authentication successful");
					string userInfo = "Username: " + Social.localUser.userName +
						"\nUser ID: " + Social.localUser.id +
						"\nIsUnderage: " + Social.localUser.underage;
					Debug.Log(userInfo);
				}
				else
					Debug.Log("Authentication failed");
			});
		}

		protected virtual void SubmitScore(long score, string leaderboardID)
		{
			Debug.Log("Reporting score " + score + " on leaderboard " + leaderboardID);
			Social.ReportScore(score, leaderboardID, success =>
			{
				Debug.Log(success ? "Reported score successfully" : "Failed to report score");
			});
#pragma warning restore 0618
        }

        protected virtual IEnumerator OpenGameOverScreen()
		{
			yield return new WaitForSeconds(1);
			GUIManager.Instance.OpenPanel(3, true); //Use the GUI Manager script to show the game over screen panel
		}

		//This is simply a function that creates one tile - used when a tile is destroyed.
		public virtual void CreateTile()
		{
			if (currentState != GameState.GAMEOVER)
			{
				GenerateTile();
				GenerateItem();
				GenerateEnemy();
				GenerateCoin();
				score += 5;

				IncreaseDifficulty(10);//Checks the time, increases difficulty by a set factor after certain seconds
			}
		}

		protected void IncreaseDifficulty(float seconds)
		{
			if (Time.time - lastTime > seconds)
			{
				Debug.Log("Increasing Diffuclty now");
				lastTime = Time.time;
				Advanced.enemyProbability += 0.01f;
				//Change settings to your liking. Increase the one time tiles by increasing their probability weight, reduce normal tiles, items etc.
			}

		}
		//This function creates an item (orange jump, blue jump) gameobject based on the probability 0 to 1 in Game Settings

		protected virtual void GenerateItem()
		{


			Vector2 tmpPos = new Vector2(Random.Range(-ScreenDistance, ScreenDistance), currentPosition);//Generate a vector with a random x and next y
			float rand = Random.Range(0f, 1f);

			//Generate Item
			if (rand < Advanced.itemAppearProbability)
			{
				GameObject gObj = GetInactiveItem();
				//Assign type to item - very important
				gObj.name = Random.Range(0, 2).ToString();
				//Set position of item and make it active
				gObj.transform.position = tmpPos;
				gObj.SetActive(true);
			}


		}

		protected virtual void GenerateCoin()
		{
			Vector2 tmpPos = new Vector2(Random.Range(-ScreenDistance, ScreenDistance), currentPosition);//Generate a vector with a random x and next y
			float rand = Random.Range(0f, 1f);

			if (rand < Advanced.coinAppearProbability)
			{
				GameObject gObj = GetInactiveCoin();
				gObj.transform.position = tmpPos;
				gObj.SetActive(true);
			}
		}

		//This function creates an enemy gameobject based on the probability 0 to 1 in Game Settings
		protected virtual void GenerateEnemy()
		{
			float rand = Random.Range(0f, 1f);
			Vector2 tmpPos = new Vector2(Random.Range(-ScreenDistance, ScreenDistance), currentPosition);//Generate a vector with a random x and next y
			if (rand < Advanced.enemyProbability)
			{
				GameObject gObj = GetInactiveEnemy();



				gObj.name = Random.Range(0, 3).ToString();//Change the max to number of enemies
				gObj.transform.position = tmpPos;
				gObj.SetActive(true);
			}
		}

		//This function adds the weights of different tiles into the sum of weights.

		protected virtual void GetTotalProbabilityWeight()
		{
			float sum = 0;
			sum += Advanced.normalTiles.probabilityWeight;
			sum += Advanced.brokenTiles.probabilityWeight;
			sum += Advanced.oneTimeOnlyTiles.probabilityWeight;
			sum += Advanced.springTiles.probabilityWeight;
			sum += Advanced.movingTilesHorizontal.probabilityWeight;
			sum += Advanced.movingTilesVertical.probabilityWeight;

			totalWeight = sum;
		}

		//getTileBasedOnRandomNumber function simply checks which range the random int (0 to sum of weights) belongs to.

		protected virtual int GetTileBasedOnRandomNumber(int randomNumber)
		{
			if (randomNumber <= Advanced.normalTiles.probabilityWeight)
				return 0;
			else if (randomNumber <= Advanced.normalTiles.probabilityWeight + Advanced.brokenTiles.probabilityWeight)
				return 1;
			else if (randomNumber <= Advanced.normalTiles.probabilityWeight + Advanced.brokenTiles.probabilityWeight + Advanced.oneTimeOnlyTiles.probabilityWeight)
				return 2;
			else if (randomNumber <= Advanced.normalTiles.probabilityWeight + Advanced.brokenTiles.probabilityWeight + Advanced.oneTimeOnlyTiles.probabilityWeight + Advanced.springTiles.probabilityWeight)
				return 3;
			else if (randomNumber <= Advanced.normalTiles.probabilityWeight + Advanced.brokenTiles.probabilityWeight + Advanced.oneTimeOnlyTiles.probabilityWeight + Advanced.springTiles.probabilityWeight + Advanced.movingTilesHorizontal.probabilityWeight)
				return 4;
			else if (randomNumber <= Advanced.normalTiles.probabilityWeight + Advanced.brokenTiles.probabilityWeight + Advanced.oneTimeOnlyTiles.probabilityWeight + Advanced.springTiles.probabilityWeight + Advanced.movingTilesHorizontal.probabilityWeight + Advanced.movingTilesVertical.probabilityWeight)
				return 5;

			return -1;
		}

		//Generate Tile Function creates a tile based on the probability specified in the inspector settings 
		//A random number from 0 to sum of weights is generated and the tile is generated according to the range, the number
		// falls into. For e.g. If normalTile has density = 30, then a number <= 30 will create a normal tile.


		protected virtual void GenerateTile()
		{
			GameObject gObj = GetInactiveTile();
			//Normal Tile = 0
			//Broken Tile = 1
			//One Time Only = 2
			//Spring Tile = 3
			//Moving Horizontally = 4
			//Moving Vertically = 5

			float rand = Random.Range(0, totalWeight);
			int randomNumber = GetTileBasedOnRandomNumber((int)rand); //A number from 0 to 5 will be generated

			Vector2 tmpPos = new Vector2(Random.Range(-ScreenDistance, ScreenDistance), currentPosition);//Generate a vector with a random x and next y

			switch (randomNumber)
			{
				case 0://Normal Tile
					gObj.transform.position = tmpPos;
					currentPosition += Random.Range(Advanced.normalTiles.minimumHeightBetweenTiles, Advanced.normalTiles.maximumHeightBetweenTiles);
					gObj.name = "0";
					gObj.SetActive(true);
					break;
				case 1://Broken Tile
					gObj.transform.position = tmpPos;
					currentPosition += Random.Range(Advanced.brokenTiles.minimumHeightBetweenTiles, Advanced.normalTiles.maximumHeightBetweenTiles);
					gObj.name = "1";
					gObj.SetActive(true);
					break;
				case 2://OneTime Tile
					gObj.transform.position = tmpPos;
					currentPosition += Random.Range(Advanced.oneTimeOnlyTiles.minimumHeightBetweenTiles, Advanced.normalTiles.maximumHeightBetweenTiles);
					gObj.name = "2";
					gObj.SetActive(true);
					break;
				case 3://Spring Tile
					gObj.transform.position = tmpPos;
					currentPosition += Random.Range(Advanced.springTiles.minimumHeightBetweenTiles, Advanced.springTiles.maximumHeightBetweenTiles);
					gObj.name = "3";
					gObj.SetActive(true);
					break;
				case 4://Moving Horizontal Tile
					gObj.transform.position = tmpPos;
					currentPosition += Random.Range(Advanced.movingTilesHorizontal.minimumHeightBetweenTiles, Advanced.movingTilesHorizontal.maximumHeightBetweenTiles);
					gObj.name = "4";
					gObj.SetActive(true);
					break;
				case 5://Moving Vertical Tile
					gObj.transform.position = tmpPos;
					currentPosition += Random.Range(Advanced.movingTilesVertical.minimumHeightBetweenTiles, Advanced.movingTilesVertical.maximumHeightBetweenTiles);
					gObj.name = "5";
					gObj.SetActive(true);
					break;
			}

		}



		//POOLING - OBJECTS ARE NOT DESTROYED BUT ADDED TO A QUEUE AND REUSED

		public virtual void AddInactiveTile(GameObject inactiveTile)
		{

			inactiveTile.SetActive(false);
			tilePool.Enqueue(inactiveTile);
			CreateTile();

		}
		public virtual GameObject GetInactiveTile()
		{
			return tilePool.Dequeue();
		}

		public virtual void AddInactiveItem(GameObject inactiveItem)
		{
			inactiveItem.SetActive(false);
			itemPool.Enqueue(inactiveItem);
		}

		public virtual void AddInactiveCoin(GameObject inactiveCoin)
		{
			inactiveCoin.SetActive(false);
			coinPool.Enqueue(inactiveCoin);

		}

		public virtual GameObject GetInactiveItem()
		{
			return itemPool.Dequeue();
		}

		public virtual GameObject GetInactiveCoin()
		{
			return coinPool.Dequeue();
		}

		public virtual void AddInactiveEnemy(GameObject inactiveEnemy)
		{

			inactiveEnemy.SetActive(false);
			enemyPool.Enqueue(inactiveEnemy);

		}
		public virtual GameObject GetInactiveEnemy()
		{
			return enemyPool.Dequeue();
		}


		//GENERATING POOL OF OBJECTS AT THE START TO BE REUSED THROUGHOUT THE GAME
		protected virtual void GenerateTilePool()
		{
			for (int i = 0; i < initialSize; i++)
			{
				GameObject gObj = (GameObject)GameObject.Instantiate(defaultTile, Vector3.zero, Quaternion.identity);
				gObj.SetActive(false);
				gObj.name = i.ToString();
				tilePool.Enqueue(gObj);
			}
		}

		protected virtual void GenerateItemPool()
		{
			for (int i = 0; i < initialSize; i++)
			{
				GameObject gObj = (GameObject)GameObject.Instantiate(defaultItem, Vector3.zero, Quaternion.identity);
				gObj.SetActive(false);
				gObj.name = i.ToString();
				itemPool.Enqueue(gObj);
			}
		}



		protected virtual void GenerateCoinPool()
		{
			for (int i = 0; i < initialSize; i++)
			{
				GameObject gObj = (GameObject)GameObject.Instantiate(defaultCoin, Vector3.zero, Quaternion.identity);
				gObj.SetActive(false);
				gObj.name = i.ToString();
				coinPool.Enqueue(gObj);
			}
		}

		protected virtual void GenerateEnemyPool()
		{
			for (int i = 0; i < initialSize; i++)
			{
				GameObject gObj = (GameObject)GameObject.Instantiate(defaultEnemy, Vector3.zero, Quaternion.identity);
				gObj.SetActive(false);
				gObj.name = i.ToString();
				enemyPool.Enqueue(gObj);
			}
		}

	}
}