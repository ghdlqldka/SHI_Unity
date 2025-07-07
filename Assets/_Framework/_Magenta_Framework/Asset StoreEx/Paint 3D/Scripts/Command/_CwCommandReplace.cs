using System.Collections.Generic;
using UnityEngine;
using CW.Common;

namespace PaintCore
{
	/// <summary>This class manages the replace painting command.</summary>
	public class _CwCommandReplace : CwCommandReplace
    {
		public static new CwCommandReplace Instance = new CwCommandReplace();
        // private static Stack<CwCommandReplace> pool = new Stack<CwCommandReplace>();
        protected static Stack<_CwCommandReplace> pool = new Stack<_CwCommandReplace>();

        static _CwCommandReplace()
		{
            // BuildMaterial(ref cachedMaterial, ref cachedMaterialHash, "Hidden/PaintCore/CwReplace");
            CwCommandReplace.Instance = Instance;

        }

        public override void Pool()
        {
            pool.Push(this);
        }

        public override CwCommand SpawnCopy()
        {
            _CwCommandReplace command = SpawnCopy(pool);

            command.Texture = Texture;
            command.Color = Color;

            return command;
        }
    }
}