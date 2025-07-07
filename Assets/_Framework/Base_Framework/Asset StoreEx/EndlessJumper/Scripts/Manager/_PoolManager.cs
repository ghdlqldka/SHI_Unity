using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using EndlessJumper;

namespace _Base_Framework._EndlessJumper
{

	public class _PoolManager : MonoBehaviour
	{
		private static string LOG_FORMAT = "<color=#94B530><b>[_PoolManager]</b></color> {0}";

		protected static _PoolManager _instance;
		public static _PoolManager Instance
		{
			get
			{
				return _instance;
			}
			protected set
			{
				_instance = value;
			}
		}

		[SerializeField]
		protected int initialSize = 40;

		[Header("Prefabs")]
		public GameObject defaultTile;
		public GameObject defaultItem;
		public GameObject defaultEnemy;
		public GameObject defaultCoin;

		[Header("Pools")]
		[SerializeField]
		protected Transform tilePoolTransform;
		[SerializeField]
		protected Transform itemPoolTransform;
		[SerializeField]
		protected Transform coinPoolTransform;
		[SerializeField]
		protected Transform enemyPoolTransform;

		protected Queue<GameObject> tilePool = new Queue<GameObject>();
		protected Queue<GameObject> enemyPool = new Queue<GameObject>();
		protected Queue<GameObject> itemPool = new Queue<GameObject>();
		protected Queue<GameObject> coinPool = new Queue<GameObject>();

		protected virtual void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
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

		public virtual void GeneratePools()
		{
			Debug.LogWarningFormat(LOG_FORMAT, "GeneratePools()");

			GenerateTilePool(); //Generate atleast 10-20 tiles at first and resuse them (pooling)
			GenerateEnemyPool(); //Generate atleast 10-20 enemies at first and resuse them (pooling)
			GenerateItemPool(); //Generate atleast 10-20 items at first and resuse them (pooling)
			GenerateCoinPool(); //Generate atleast 10-20 items at first and resuse them (pooling)
		}

		protected virtual void GenerateTilePool()
		{
			for (int i = 0; i < initialSize; i++)
			{
				GameObject obj = Instantiate(defaultTile, Vector3.zero, Quaternion.identity);
				obj.transform.parent = tilePoolTransform;
				obj.SetActive(false);
				obj.name = i.ToString();

				tilePool.Enqueue(obj);
			}
		}

		protected virtual void GenerateItemPool()
		{
			for (int i = 0; i < initialSize; i++)
			{
				GameObject obj = Instantiate(defaultItem, Vector3.zero, Quaternion.identity);
				obj.transform.parent = itemPoolTransform;
				obj.SetActive(false);
				obj.name = i.ToString();

				itemPool.Enqueue(obj);
			}
		}

		protected virtual void GenerateCoinPool()
		{
			for (int i = 0; i < initialSize; i++)
			{
				GameObject obj = Instantiate(defaultCoin, Vector3.zero, Quaternion.identity);
				obj.transform.parent = coinPoolTransform;
				obj.SetActive(false);
				obj.name = i.ToString();

				coinPool.Enqueue(obj);
			}
		}

		protected virtual void GenerateEnemyPool()
		{
			for (int i = 0; i < initialSize; i++)
			{
				GameObject obj = Instantiate(defaultEnemy, Vector3.zero, Quaternion.identity);
				obj.transform.parent = enemyPoolTransform;
				obj.SetActive(false);
				obj.name = i.ToString();

				enemyPool.Enqueue(obj);
			}
		}

		public virtual void PushTile(GameObject obj)
		{
			obj.SetActive(false);
			tilePool.Enqueue(obj);
		}

		public virtual GameObject PopTile()
		{
			return tilePool.Dequeue();
		}

		public virtual void PushEnemy(GameObject obj)
		{
			obj.SetActive(false);
			enemyPool.Enqueue(obj);
		}

		public virtual GameObject PopEnemy()
		{
			return enemyPool.Dequeue();
		}

		public virtual void PushItem(GameObject obj)
		{
			obj.SetActive(false);
			itemPool.Enqueue(obj);
		}

		public virtual GameObject PopItem()
		{
			return itemPool.Dequeue();
		}

		public virtual void PushCoin(GameObject obj)
		{
			obj.SetActive(false);
			coinPool.Enqueue(obj);
		}

		public virtual GameObject PopCoin()
		{
			return coinPool.Dequeue();
		}
	}
}