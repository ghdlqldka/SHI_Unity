using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Lean.Pool
{
	/// <summary>This component allows you to pool GameObjects, giving you a very fast alternative to Instantiate and Destroy.
	/// Pools also have settings to preload, recycle, and set the spawn capacity, giving you lots of control over your spawning.</summary>
	[ExecuteInEditMode]
	// [HelpURL(LeanPool.HelpUrlPrefix + "LeanGameObjectPool")]
	// [AddComponentMenu(LeanPool.ComponentPathPrefix + "GameObject Pool")]
	public class _LeanGameObjectPool : LeanGameObjectPool
	{
		private static string LOG_FORMAT = "<color=#00E1FF><b>[_LeanGameObjectPool]</b></color> {0}";

		public static LinkedList<LeanGameObjectPool> InstanceList
		{
			get
			{
				return Instances;
			}
		}

		// [SerializeField]
		// protected List<GameObject> despawnedClones = new List<GameObject>();
		protected List<GameObject> despawnedCloneList
		{
			get
			{
				return despawnedClones;
			}
		}

		public delegate void _Spawned(GameObject clone);
		public event _Spawned OnSpawned;
		public event _Spawned OnDespawned;

		protected override void Awake()
		{
			Debug.LogFormat(LOG_FORMAT, "Awake()");

			Debug.Assert(prefab != null);
			notification = NotificationType.IPoolable; // forcelly set
			strategy = StrategyType.ActivateAndDeactivate; // forcelly set
			Debug.Assert(preload >= 0);
			capacity = 0; // Not used!!!!!
			recycle = true; // forcelly set
			persist = false; // Not used!!!!!
			stamp = false; // Not used!!!!!
			warnings = true; // Not used!!!!!

			if (Application.isPlaying == true)
			{
				PreloadAll();

				/*
				if (persist == true)
				{
					DontDestroyOnLoad(this);
				}
				*/
			}
		}

		protected override void OnDestroy()
		{
			// If OnDestroy is called then the scene is likely changing, so we detach the spawned prefabs from the global links dictionary to prevent issues.
			foreach (GameObject clone in spawnedClonesList)
			{
				if (clone != null)
				{
					LeanPool.Detach(clone, false);
				}
			}

			foreach (GameObject clone in spawnedClonesHashSet)
			{
				if (clone != null)
				{
					LeanPool.Detach(clone, false);
				}
			}
		}

		protected override void OnEnable()
		{
			instancesNode = InstanceList.AddLast(this);

			RegisterPrefab();
		}

		protected override void OnDisable()
		{
			UnregisterPrefab();

			InstanceList.Remove(instancesNode);
			instancesNode = null;
		}

		protected override void Update()
		{
			// Decay the life of all delayed destruction calls
			for (int i = delays.Count - 1; i >= 0; i--)
			{
				Delay delay = delays[i];

				delay.Life -= Time.deltaTime;

				// Skip to next one?
				if (delay.Life > 0.0f)
				{
					continue;
				}

				// Remove and pool delay
				delays.RemoveAt(i);
				LeanClassPool<Delay>.Despawn(delay);

				// Finally despawn it after delay
				if (delay.Clone != null)
				{
					Despawn(delay.Clone);
				}
				else
				{
					Debug.LogWarningFormat(LOG_FORMAT, "Attempting to update the delayed destruction of a prefab clone that no longer exists, did you accidentally destroy it?");
				}
			}
		}

		protected override void InvokeOnSpawn(GameObject clone)
		{
			Debug.LogWarningFormat(LOG_FORMAT, "InvokeOn<b>Spawn</b>(), clone : <b><color=yellow>" + clone.name + "</color></b>");
			// base.InvokeOnSpawn(clone);

			/*
			switch (notification)
			{
				case NotificationType.SendMessage:
					clone.SendMessage("OnSpawn", SendMessageOptions.DontRequireReceiver);
					break;
				case NotificationType.BroadcastMessage: 
					clone.BroadcastMessage("OnSpawn", SendMessageOptions.DontRequireReceiver); 
					break;
				case NotificationType.IPoolable: 
					clone.GetComponents(tempPoolables); 
					for (var i = tempPoolables.Count - 1; i >= 0; i--) 
						tempPoolables[i].OnSpawn(); 
					break;
				case NotificationType.BroadcastIPoolable: 
					clone.GetComponentsInChildren(tempPoolables); 
					for (var i = tempPoolables.Count - 1; i >= 0; i--) 
						tempPoolables[i].OnSpawn(); 
					break;
			}
			*/
			if (OnSpawned != null)
			{
				OnSpawned(clone);
			}
		}

		protected override void InvokeOnDespawn(GameObject clone)
		{
			Debug.LogWarningFormat(LOG_FORMAT, "Invoke<b>OnDespawn</b>(), clone : " + clone.name);
			/*
			switch (notification)
			{
				case NotificationType.SendMessage: 
					clone.SendMessage("OnDespawn", SendMessageOptions.DontRequireReceiver); 
					break;
				case NotificationType.BroadcastMessage: 
					clone.BroadcastMessage("OnDespawn", SendMessageOptions.DontRequireReceiver); 
					break;
				case NotificationType.IPoolable:
					clone.GetComponents(tempPoolables); 
					for (var i = tempPoolables.Count - 1; i >= 0; i--) 
						tempPoolables[i].OnDespawn(); 
					break;
				case NotificationType.BroadcastIPoolable: 
					clone.GetComponentsInChildren(tempPoolables); 
					for (var i = tempPoolables.Count - 1; i >= 0; i--) 
						tempPoolables[i].OnDespawn(); 
					break;
			}
			*/
			if (OnDespawned != null)
			{
				OnDespawned(clone);
			}
		}

		public override void Spawn() // <==============
		{
			Debug.LogWarningFormat(LOG_FORMAT, "Spawn()");

			GameObject clone = default(GameObject);
			// TrySpawn(ref clone);
			Transform parent = null;
			TrySpawn(ref clone, prefab.transform.localPosition, prefab.transform.localRotation, prefab.transform.localScale, parent, false);
		}

		public override void DespawnOldest() // <==============
		{
			Debug.LogWarningFormat(LOG_FORMAT, "DespawnOldest()");

			var clone = default(GameObject);

			TryDespawnOldest(ref clone, true);
		}

		public override void DespawnAll() // <==============
		{
			Debug.LogWarningFormat(LOG_FORMAT, "DespawnAll()");
			DespawnAll(true);
		}

#pragma warning disable 0809
		[System.Obsolete("")]
		public override void Spawn(Vector3 position)
#pragma warning restore 0809
		{
			throw new System.NotSupportedException("");
		}

		public override GameObject Spawn(Transform parent, bool worldPositionStays = false)
		{
			Debug.LogWarningFormat(LOG_FORMAT, "Spawn()");

			GameObject clone = default(GameObject); 
			TrySpawn(ref clone, parent, worldPositionStays); 
			return clone;
		}

		public override GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent = null)
		{
			Debug.LogWarningFormat(LOG_FORMAT, "Spawn()");

			GameObject clone = default(GameObject); 
			TrySpawn(ref clone, position, rotation, parent); 
			return clone;
		}

		public override bool TrySpawn(ref GameObject clone, Transform parent, bool worldPositionStays = false)
		{
			Debug.LogWarningFormat(LOG_FORMAT, "TrySpawn()");

			if (prefab == null) 
			{ 
				Debug.LogWarningFormat(LOG_FORMAT, "You're attempting to spawn from a pool with a null prefab"); 
				return false;
			}

			if (parent != null && worldPositionStays == true)
			{
				return TrySpawn(ref clone, prefab.transform.position, Quaternion.identity, Vector3.one, parent, worldPositionStays);
			}
			return TrySpawn(ref clone, this.transform.localPosition, this.transform.localRotation, this.transform.localScale, parent, worldPositionStays);
		}

		public override bool TrySpawn(ref GameObject clone, Vector3 position, Quaternion rotation, Transform parent = null)
		{
			Debug.LogWarningFormat(LOG_FORMAT, "TrySpawn()");

			if (prefab == null) 
			{ 
				Debug.LogWarningFormat(LOG_FORMAT, "You're attempting to spawn from a pool with a null prefab"); 
				return false; 
			}

			if (parent != null)
			{
				position = parent.InverseTransformPoint(position);
				rotation = Quaternion.Inverse(parent.rotation) * rotation;
			}
			return TrySpawn(ref clone, position, rotation, prefab.transform.localScale, parent, false);
		}

		public override bool TrySpawn(ref GameObject clone)
		{
			Debug.LogWarningFormat(LOG_FORMAT, "TrySpawn()");
			// base.TrySpawn(ref clone);

			Debug.Assert(prefab != null);
			/*
			if (prefab == null) 
			{ 
				Debug.LogWarningFormat(LOG_FORMAT, "You're attempting to spawn from a pool with a null prefab"); 
				return false; 
			}
			*/
			Transform parent = null;
			return TrySpawn(ref clone, prefab.transform.localPosition, prefab.transform.localRotation, prefab.transform.localScale, parent, false);
		}

		public override bool TrySpawn(ref GameObject clone, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Transform parent, bool worldPositionStays)
		{
			// Debug.LogWarningFormat(LOG_FORMAT, "TrySpawn()");

			Debug.Assert(prefab != null);
			// if (prefab != null)
			{
				// Spawn a previously despawned/preloaded clone?
				for (int i = despawnedCloneList.Count - 1; i >= 0; i--)
				{
					clone = despawnedCloneList[i];

					despawnedCloneList.RemoveAt(i);

					if (clone != null)
					{
						SpawnClone(clone, localPosition, localRotation, localScale, parent, worldPositionStays);

						return true;
					}

					Debug.LogWarningFormat(LOG_FORMAT, "This pool contained a null despawned clone, did you accidentally destroy it?");
				}

				Debug.Assert(capacity == 0);
				// Make a new clone?
				if (capacity <= 0 || Total < capacity)
				{
					clone = CreateClone(localPosition, localRotation, localScale, parent, worldPositionStays);

					// Add clone to spawned list
					Debug.Assert(recycle == true);
					// if (recycle == true)
					{
						spawnedClonesList.Add(clone);
					}
					/*
					else
					{
						spawnedClonesHashSet.Add(clone);
					}
					*/

					// Activate?
					Debug.Assert(strategy == StrategyType.ActivateAndDeactivate);
					// if (strategy == StrategyType.ActivateAndDeactivate)
					{
						clone.SetActive(true);
					}

					InvokeOnSpawn(clone); // Notifications

					return true;
				}

				// Recycle?
				Debug.Assert(recycle == true);
				if (/*recycle == true &&*/ TryDespawnOldest(ref clone, false) == true)
				{
					SpawnClone(clone, localPosition, localRotation, localScale, parent, worldPositionStays);

					return true;
				}
			}
			/*
			else
			{
				Debug.LogWarningFormat(LOG_FORMAT, "You're attempting to spawn from a pool with a null prefab");
			}
			*/

			return false;
		}

		protected override void SpawnClone(GameObject clone, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Transform parent, bool worldPositionStays)
		{
			Debug.LogFormat(LOG_FORMAT, "SpawnClone()");

			// Register
			Debug.Assert(recycle == true);
			// if (recycle == true)
			{
				spawnedClonesList.Add(clone);
			}
			/*
			else
			{
				spawnedClonesHashSet.Add(clone);
			}
			*/

			// Update transform
			Transform cloneTransform = clone.transform;

			cloneTransform.SetParent(null, false);

			cloneTransform.localPosition = localPosition;
			cloneTransform.localRotation = localRotation;
			cloneTransform.localScale = localScale;

			cloneTransform.SetParent(parent, worldPositionStays);

			// Make sure it's in the current scene
			if (parent == null)
			{
				// Move a GameObject from its current Scene to a new Scene
				SceneManager.MoveGameObjectToScene(clone, SceneManager.GetActiveScene());
			}

			// Activate
			Debug.Assert(strategy == StrategyType.ActivateAndDeactivate);
			// if (strategy == StrategyType.ActivateAndDeactivate)
			{
				clone.SetActive(true);
			}

			// Notifications
			InvokeOnSpawn(clone);
		}

		protected override GameObject CreateClone(Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Transform parent, bool worldPositionStays)
		{
			Debug.LogFormat(LOG_FORMAT, "CreateClone()");

			GameObject clone = DoInstantiate(prefab, localPosition, localRotation, localScale, parent, worldPositionStays);

			// if (stamp == true)
			{
				clone.name = prefab.name + "_" + Total;
			}
			/*
			else
			{
				clone.name = prefab.name;
			}
			*/

			return clone;
		}

		public override void DespawnAll(bool cleanLinks)
		{
			Debug.LogFormat(LOG_FORMAT, "DespawnAll(), cleanLinks : " + cleanLinks);

			// Merge
			MergeSpawnedClonesToList();

			// Despawn
			for (int i = spawnedClonesList.Count - 1; i >= 0; i--)
			{
				GameObject clone = spawnedClonesList[i];

				if (clone != null)
				{
					if (cleanLinks == true)
					{
						LeanPool.Links.Remove(clone);
					}

					DespawnNow(clone);
				}
			}

			spawnedClonesList.Clear();

			// Clear all delays
			for (var i = delays.Count - 1; i >= 0; i--)
			{
				LeanClassPool<Delay>.Despawn(delays[i]);
			}

			delays.Clear();
		}

		public override void PreloadOneMore()
		{
			Debug.Assert(prefab != null);
			// if (prefab != null)
			{
				// Create clone
				GameObject clone = CreateClone(Vector3.zero, Quaternion.identity, Vector3.one, null, false);

				// Add clone to despawned list
				despawnedCloneList.Add(clone);

				Debug.Assert(strategy == StrategyType.ActivateAndDeactivate);
				// Deactivate it
				// if (strategy == StrategyType.ActivateAndDeactivate)
				{
					clone.SetActive(false);

					clone.transform.SetParent(this.transform, false);
				}
				/*
				else
				{
					clone.transform.SetParent(DeactivatedChild, false);
				}
				*/

				/*
				if (warnings == true && capacity > 0 && Total > capacity)
					Debug.LogWarningFormat(LOG_FORMAT, "You've preloaded more than the pool capacity, please verify you're preloading the intended amount.");
				*/
			}
			/*
			else
			{
				// if (warnings == true)
					Debug.LogWarningFormat(LOG_FORMAT, "Attempting to preload a null prefab.");
			}
			*/
		}

		public override void PreloadAll()
		{
			Debug.LogFormat(LOG_FORMAT, "PreloadAll(), preload : <b><color=cyan>" + preload + "</color></b>");
			
			if (preload > 0)
			{
				Debug.Assert(prefab != null);
				// if (prefab != null)
				{
					for (int i = Total; i < preload; i++)
					{
						PreloadOneMore();
					}
				}
				/*
				else
				{
					Debug.LogWarningFormat(LOG_FORMAT, "Attempting to preload a null prefab", this);
				}
				*/
			}
		}
	}
}
