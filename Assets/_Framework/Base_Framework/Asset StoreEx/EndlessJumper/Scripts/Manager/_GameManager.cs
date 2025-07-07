using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace _Base_Framework._EndlessJumper
{
	public class _GameManager : EndlessJumper.GameManager
	{
		private static string LOG_FORMAT = "<color=#94B530><b>[_GameManager]</b></color> {0}";

		public static new _GameManager Instance
		{
			get
			{
				return _instance as _GameManager;
			}
			set
			{
				_instance = value;
			}
		}

		[ReadOnly]
		[SerializeField]
		protected Camera _cam;
		public Camera _Cam
		{
			get
			{
				return _cam;
			}
		}

		public enum GameStateEx
		{
			None,

			Ready, 
			Pause,
			Playing,
			Win,
			Lose,
		}

		[ReadOnly]
		[SerializeField]
		protected GameStateEx _state = GameStateEx.Ready;
		public GameStateEx _State
		{
			get
			{
				return _state;
			}
			protected set
			{
				if (_state != value)
				{
					Debug.LogWarningFormat(LOG_FORMAT, "GameState : <b><color=magenta>" + _state + "</color></b> => " + "<b><color=red>" + value + "</color></b>");
					_state = value;
					Invoke_OnStateChanged(value);
				}
				else
				{
					Debug.LogWarningFormat(LOG_FORMAT, "Duplicated GameState : <b><color=red>" + value + "</color></b> SET!!!!!!");
				}
			}
		}

		public delegate void StateChanged(GameStateEx state);
		public event StateChanged OnStateChanged;
		protected virtual void Invoke_OnStateChanged(GameStateEx state)
		{
			if (OnStateChanged != null)
			{
				OnStateChanged(state);
			}
		}

		[ReadOnly]
		[SerializeField]
		protected _PoolManager poolManagerEx;

		// public Player player;
		public _Player _Player
		{
			get
			{
				return player as _Player;
			}
		}

		public enum ItemType
		{
			Powerup_00,
			Powerup_01,
			Coin,
		}

		protected override void Awake()
		{
			if (_Base_Framework_Config.Product == _Base_Framework_Config._Product.Base_Framework)
			{
				if (_GlobalObjectUtilities.Instance == null)
				{
					GameObject prefab = Resources.Load<GameObject>(_GlobalObjectUtilities.prefabPath);
					Debug.Assert(prefab != null);
					GameObject obj = Instantiate(prefab);
					Debug.LogFormat(LOG_FORMAT, "<color=magenta>Instantiate \"<b>GlobalObjectUtilities</b>\"</color>");
					obj.name = prefab.name;
				}
			}
			else
			{
				Debug.Assert(false);
			}

			if (Instance == null)
			{
				Instance = this;

				Debug.Assert(poolManagerEx != null);

				score = 0; // Not used
				coins = 0; // Not used

				Advanced = null; // Not used!!!!!
				totalWeight = 0; // Not used!!!!!

				defaultTile = null;
				defaultItem = null;
				defaultEnemy = null;
				defaultCoin = null;

				tilePool = null;
				enemyPool = null;
				itemPool = null;
				coinPool = null;

				ingamePanel = null; // Do Not Use This variable!!!!!
				currentState = GameState.GAMEOVER; // Do Not Use This variable!!!!!
			}
			else
			{
				Debug.LogErrorFormat(LOG_FORMAT, "");
				Destroy(this);
				return;
			}
		}

		protected virtual void OnDestroy()
		{
			if (Instance != this)
			{
				return;
			}

			Instance = null;
		}

		protected override void Start()
		{
			// Cursor.visible = false;

			// GetTotalProbabilityWeight();

			poolManagerEx.GeneratePools();
			/*
			GenerateTilePool(); //Generate atleast 10-20 tiles at first and resuse them (pooling)
			GenerateEnemyPool(); //Generate atleast 10-20 enemies at first and resuse them (pooling)
			GenerateItemPool(); //Generate atleast 10-20 items at first and resuse them (pooling)
			GenerateCoinPool(); //Generate atleast 10-20 items at first and resuse them (pooling)
			*/

			// Debug.LogFormat(LOG_FORMAT, "Total enemies " + enemyPool.Count + " " + " Total items : " + itemPool.Count);

			for (int i = 0; i < initialSize; i++)
			{
				GenerateTile(); //Use a tile from the pool and add to scene (make it visible at the right position)
				GenerateItem(); //Use an item from the pool and add to scene (make it visible at the right position)
				GenerateEnemy(); //Use a enemy from the pool and add to scene (make it visible at the right position)
				GenerateCoin();
			}

			// PauseGame(); //Pause game at the start to show the start panel
			_State = GameStateEx.Pause;

			// initCameraPosition = Camera.main.transform.position; //Store the initial position of the camera - reset it at game over
			initCameraPosition = _cam.transform.position; //Store the initial position of the camera - reset it at game over

			SelectSkin();

			//AuthenticateGameCenter (); //Uncomment for GameCenter
		}

		protected override void FixedUpdate()
		{
			// ingamePanel.textScore.text = score.ToString();
		}

		public override void CreateTile()
		{
			if (_State != GameStateEx.Lose)
			{
				GenerateTile();
				GenerateItem();
				GenerateEnemy();
				GenerateCoin();

				// score += 5;
				_ScoreManager.Instance.Score += 5;

				IncreaseDifficulty(10);//Checks the time, increases difficulty by a set factor after certain seconds
			}
		}

		protected override void GenerateItem()
		{


			Vector2 tmpPos = new Vector2(Random.Range(-ScreenDistance, ScreenDistance), currentPosition);//Generate a vector with a random x and next y
			float rand = Random.Range(0f, 1f);

			//Generate Item
			if (rand < _SettingsManager.Instance.Settings.itemAppearProbability)
			{
				GameObject gObj = GetInactiveItem();
				//Assign type to item - very important
				gObj.name = Random.Range(0, 2).ToString();
				//Set position of item and make it active
				gObj.transform.position = tmpPos;
				gObj.SetActive(true);
			}


		}

		protected override void GenerateEnemy()
		{
			float rand = Random.Range(0f, 1f);
			Vector2 tmpPos = new Vector2(Random.Range(-ScreenDistance, ScreenDistance), currentPosition);//Generate a vector with a random x and next y
			if (rand < _SettingsManager.Instance.Settings.enemyProbability)
			{
				GameObject gObj = GetInactiveEnemy();

				gObj.name = Random.Range(0, 3).ToString();//Change the max to number of enemies
				gObj.transform.position = tmpPos;
				gObj.SetActive(true);
			}
		}

		protected override void GenerateCoin()
		{
			Vector2 tmpPos = new Vector2(Random.Range(-ScreenDistance, ScreenDistance), currentPosition);//Generate a vector with a random x and next y
			float rand = Random.Range(0f, 1f);

			if (rand < _SettingsManager.Instance.Settings.coinAppearProbability)
			{
				GameObject gObj = GetInactiveCoin();
				gObj.transform.position = tmpPos;
				gObj.SetActive(true);
			}
		}

		public override void AddInactiveTile(GameObject inactiveTile)
		{
			poolManagerEx.PushTile(inactiveTile);
			/*
			inactiveTile.SetActive(false);
			tilePool.Enqueue(inactiveTile);
			*/
			CreateTile();

		}
		public override GameObject GetInactiveTile()
		{
			// return tilePool.Dequeue();
			return poolManagerEx.PopTile();
			throw new System.NotSupportedException("");
		}

		public override void AddInactiveItem(GameObject inactiveItem)
		{
			poolManagerEx.PushItem(inactiveItem);
			/*
			inactiveItem.SetActive(false);
			itemPool.Enqueue(inactiveItem);
			*/
		}

		public override void AddInactiveCoin(GameObject inactiveCoin)
		{
			/*
			inactiveCoin.SetActive(false);
			coinPool.Enqueue(inactiveCoin);
			*/
			poolManagerEx.PushCoin(inactiveCoin);
		}

		public override GameObject GetInactiveItem()
		{
			// return itemPool.Dequeue();
			return poolManagerEx.PopItem();
		}

		public override GameObject GetInactiveCoin()
		{
			// return coinPool.Dequeue();
			return poolManagerEx.PopCoin();
		}

		public override void AddInactiveEnemy(GameObject inactiveEnemy)
		{
			poolManagerEx.PushEnemy(inactiveEnemy);
			/*
			inactiveEnemy.SetActive(false);
			enemyPool.Enqueue(inactiveEnemy);
			*/
		}
		public override GameObject GetInactiveEnemy()
		{
			// return enemyPool.Dequeue();
			return poolManagerEx.PopEnemy();
		}

		public override void PauseGame()
		{
			throw new System.NotSupportedException("");
		}

		public override void ResumeGame()
		{
			SelectSkin();

			player.gameObject.SetActive(true); //Resume game - make the player object active
			_State = GameStateEx.Playing; //Resume game - make the variable false
		}

		public override void EndGame()
		{
			if (_State != GameStateEx.Lose)
			{
				//Camera.main.GetComponent<PlayerCamera>().enabled = false; //Disable the script that moves the camera
				_State = GameStateEx.Lose; //Make game over using one variable

				// ingamePanel.ShowFlash();
				// StartCoroutine(OpenGameOverScreen());
				//Camera.main.transform.position = initCameraPosition; //reset camera to initial/original position to show game over screen

#if false
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
#endif

				// Debug.LogError("Check below code");
				// iTween.ShakePosition(Camera.main.gameObject, new Vector3(0.7f, 0.7f, 0f), 0.5f);
				MusicManager.instance.isGameOver = true;

				if (PlayerPrefs.GetInt("isVibrationOn", 1) == 1)
				{
#if UNITY_ANDROID || UNITY_IOS
					Handheld.Vibrate();
#endif
				}
				// SubmitScore(score, "Leaderboard");
				// PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins", 100) + coins);

			}
		}

		protected override void SubmitScore(long score, string leaderboardID)
		{
			throw new System.NotSupportedException("");
		}

		protected override IEnumerator OpenGameOverScreen()
		{
			yield return new WaitForSeconds(1);
			GUIManager.Instance.OpenPanel(3, true); //Use the GUI Manager script to show the game over screen panel

			throw new System.NotSupportedException("");
		}

		protected override void GetTotalProbabilityWeight()
		{
			throw new System.NotSupportedException("");
		}

		protected override int GetTileBasedOnRandomNumber(int randomNumber)
		{
			throw new System.NotSupportedException("");
		}

		protected override void GenerateTile()
		{
			GameObject obj = poolManagerEx.PopTile();
			_Tile tile = obj.GetComponent<_Tile>();
			Debug.Assert(tile != null);
			
			//Normal Tile = 0
			//Broken Tile = 1
			//One Time Only = 2
			//Spring Tile = 3
			//Moving Horizontally = 4
			//Moving Vertically = 5

			float rand = Random.Range(0, _SettingsManager.Instance.TotalWeight);
			Debug.LogFormat(LOG_FORMAT, "rand : " + rand);
			int randomNumber = _SettingsManager.Instance.GetTileBasedOnRandomNumber((int)rand); //A number from 0 to 5 will be generated

			Vector2 tmpPos = new Vector2(Random.Range(-ScreenDistance, ScreenDistance), currentPosition);//Generate a vector with a random x and next y

			GameSettings _settings = _SettingsManager.Instance.Settings;

			switch (randomNumber)
			{
				case 0://Normal Tile
					obj.transform.position = tmpPos;
					currentPosition += Random.Range(_settings.normalTiles.minimumHeightBetweenTiles, _settings.normalTiles.maximumHeightBetweenTiles);
					tile._Type = _Tile.Type.Normal;
					obj.name = "0";
					obj.SetActive(true);
					break;

				case 1://Broken Tile
					obj.transform.position = tmpPos;
					currentPosition += Random.Range(_settings.brokenTiles.minimumHeightBetweenTiles, _settings.normalTiles.maximumHeightBetweenTiles);
					tile._Type = _Tile.Type.Broken;
					obj.name = "1";
					obj.SetActive(true);
					break;

				case 2://OneTime Tile
					obj.transform.position = tmpPos;
					currentPosition += Random.Range(_settings.oneTimeOnlyTiles.minimumHeightBetweenTiles, _settings.normalTiles.maximumHeightBetweenTiles);
					tile._Type = _Tile.Type.OneTime;
					obj.name = "2";
					obj.SetActive(true);
					break;

				case 3://Spring Tile
					obj.transform.position = tmpPos;
					currentPosition += Random.Range(_settings.springTiles.minimumHeightBetweenTiles, _settings.springTiles.maximumHeightBetweenTiles);
					tile._Type = _Tile.Type.Spring;
					obj.name = "3";
					obj.SetActive(true);
					break;

				case 4://Moving Horizontal Tile
					obj.transform.position = tmpPos;
					currentPosition += Random.Range(_settings.movingTilesHorizontal.minimumHeightBetweenTiles, _settings.movingTilesHorizontal.maximumHeightBetweenTiles);
					tile._Type = _Tile.Type.MovingHorizontally;
					obj.name = "4";
					obj.SetActive(true);
					break;

				case 5://Moving Vertical Tile
					obj.transform.position = tmpPos;
					currentPosition += Random.Range(_settings.movingTilesVertical.minimumHeightBetweenTiles, _settings.movingTilesVertical.maximumHeightBetweenTiles);
					tile._Type = _Tile.Type.MovingVertically;
					obj.name = "5";
					obj.SetActive(true);
					break;

				default:
					Debug.Assert(false);
					break;
			}

		}
	}
}