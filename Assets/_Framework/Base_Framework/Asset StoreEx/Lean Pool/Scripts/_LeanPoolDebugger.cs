using UnityEngine;
using CW.Common;
using Lean.Pool.Examples;

namespace Lean.Pool
{
	/// <summary>This component can be added to your prefab GameObject, and it will throw warnings if it is instantiated without the use of <b>LeanPool.Spawn</b>, or despawned without the use of <b>LeanPool.Despawn</b>.</summary>
	// [HelpURL(LeanPool.HelpUrlPrefix + "LeanPoolDebugger")]
	// [AddComponentMenu(LeanPool.ComponentPathPrefix + "Pool Debugger")]
	public class _LeanPoolDebugger : LeanPoolDebugger
	{
		protected _LeanGameObjectPool Pool
		{
			get
			{
				return cachedPool as _LeanGameObjectPool;
			}
		}

		protected override bool Exists()
		{
			// if (LeanPool.Links.TryGetValue(gameObject, out cachedPool) == true)
			if (LeanPool.Links.TryGetValue(gameObject, out cachedPool) == true)
			{
				return true;
			}

			if (_LeanGameObjectPool.TryFindPoolByClone(gameObject, ref cachedPool) == true)
			{
				return true;
			}

			return false;
		}
	}
}
