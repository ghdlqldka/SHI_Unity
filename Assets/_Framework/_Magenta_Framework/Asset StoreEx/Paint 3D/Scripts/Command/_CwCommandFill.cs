using System.Collections.Generic;
using UnityEngine;
using CW.Common;

namespace PaintCore
{
	/// <summary>This class manages the fill painting command.</summary>
	public class _CwCommandFill : CwCommandFill
    {
		public static new _CwCommandFill Instance = new _CwCommandFill();
        // private static Stack<CwCommandFill> pool = new Stack<CwCommandFill>();
        private static Stack<_CwCommandFill> pool = new Stack<_CwCommandFill>();

        static _CwCommandFill()
		{
            // BuildMaterial(ref cachedMaterial, ref cachedMaterialHash, "Hidden/PaintCore/CwFill");
            CwCommandFill.Instance = Instance;
        }

        public override void Pool()
        {
            pool.Push(this);
        }

        public override CwCommand SpawnCopy()
        {
            // base.SpawnCopy();
            _CwCommandFill command = SpawnCopy(pool);

            command.Blend = Blend;
            command.Texture = Texture;
            command.Color = Color;
            command.Opacity = Opacity;
            command.Minimum = Minimum;

            return command;
        }
    }
}