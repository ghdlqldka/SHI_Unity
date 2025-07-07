using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace _Base_Framework._EndlessJumper
{

	public class _SettingsManager : MonoBehaviour
	{
		private static string LOG_FORMAT = "<color=#94B530><b>[_SettingsManager]</b></color> {0}";

		protected static _SettingsManager _instance;
		public static _SettingsManager Instance
		{
			get
			{
				return _instance;
			}
			set
			{
				_instance = value;
			}
		}

		[ReadOnly]
		[SerializeField]
		protected float totalWeight;
		public float TotalWeight
		{
			get
			{
				return totalWeight;
			}
		}

		[SerializeField]
		protected GameSettings gameSettings;
		public GameSettings Settings
		{
			get
			{
				return gameSettings;
			}
		}

		protected virtual void Awake()
		{
			if (Instance == null)
			{
				Instance = this;

				totalWeight = 0;
				totalWeight += Settings.normalTiles.probabilityWeight;
				totalWeight += Settings.brokenTiles.probabilityWeight;
				totalWeight += Settings.oneTimeOnlyTiles.probabilityWeight;
				totalWeight += Settings.springTiles.probabilityWeight;
				totalWeight += Settings.movingTilesHorizontal.probabilityWeight;
				totalWeight += Settings.movingTilesVertical.probabilityWeight;
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

		public virtual int GetTileBasedOnRandomNumber(int randomNumber)
		{
			float normalTilesProbabilityWeight = Settings.normalTiles.probabilityWeight;
			float brokenTilesProbabilityWeight = Settings.brokenTiles.probabilityWeight;
			float oneTimeOnlyTilesProbabilityWeight = Settings.oneTimeOnlyTiles.probabilityWeight;
			float springTilesProbabilityWeight = Settings.springTiles.probabilityWeight;
			float movingTilesHorizontalProbabilityWeight = Settings.movingTilesHorizontal.probabilityWeight;
			float movingTilesVerticalProbabilityWeight = Settings.movingTilesVertical.probabilityWeight;

			if (randomNumber <= normalTilesProbabilityWeight)
				return 0;
			else if (randomNumber <= normalTilesProbabilityWeight + brokenTilesProbabilityWeight)
				return 1;
			else if (randomNumber <= normalTilesProbabilityWeight + brokenTilesProbabilityWeight + oneTimeOnlyTilesProbabilityWeight)
				return 2;
			else if (randomNumber <= normalTilesProbabilityWeight + brokenTilesProbabilityWeight + oneTimeOnlyTilesProbabilityWeight + springTilesProbabilityWeight)
				return 3;
			else if (randomNumber <= normalTilesProbabilityWeight + brokenTilesProbabilityWeight + oneTimeOnlyTilesProbabilityWeight + springTilesProbabilityWeight + movingTilesHorizontalProbabilityWeight)
				return 4;
			else if (randomNumber <= normalTilesProbabilityWeight + brokenTilesProbabilityWeight + oneTimeOnlyTilesProbabilityWeight + springTilesProbabilityWeight + movingTilesHorizontalProbabilityWeight + movingTilesVerticalProbabilityWeight)
				return 5;

			return -1;
		}
	}
}