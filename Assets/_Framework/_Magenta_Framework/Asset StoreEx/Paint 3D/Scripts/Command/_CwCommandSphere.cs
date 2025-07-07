using System.Collections.Generic;
using UnityEngine;
using CW.Common;
using PaintCore;

namespace PaintIn3D
{
	/// <summary>This class manages the sphere painting command.</summary>
	public class _CwCommandSphere : CwCommandSphere
    {
        private static string LOG_FORMAT = "<color=#06FF00><b>[_CwCommandSphere]</b></color> {0}";

        public static new _CwCommandSphere Instance = new _CwCommandSphere();
        // private static Stack<CwCommandSphere> pool = new Stack<CwCommandSphere>();
        protected static Stack<_CwCommandSphere> pool = new Stack<_CwCommandSphere>();

        static _CwCommandSphere()
		{
			Debug.LogFormat(LOG_FORMAT, "_CwCommandSphere.constructor!!!!!!!");

            CwCommandSphere.Instance = Instance;
        }

        public override void Pool()
        {
            pool.Push(this);
        }

        public override CwCommand SpawnCopy()
        {
            _CwCommandSphere command = SpawnCopy(pool);

            command.Blend = Blend;
            command.In3D = In3D;
            command.Position = Position;
            command.EndPosition = EndPosition;
            command.Position2 = Position2;
            command.EndPosition2 = EndPosition2;
            command.Extrusions = Extrusions;
            command.Clip = Clip;
            command.Matrix = Matrix;
            command.Color = Color;
            command.Opacity = Opacity;
            command.Hardness = Hardness;
            command.TileTexture = TileTexture;
            command.TileMatrix = TileMatrix;
            command.TileOpacity = TileOpacity;
            command.TileTransition = TileTransition;
            command.MaskMatrix = MaskMatrix;
            command.MaskShape = MaskShape;
            command.MaskChannel = MaskChannel;
            command.MaskStretch = MaskStretch;
            command.MaskInvert = MaskInvert;
            command.DepthMask = DepthMask;

            return command;
        }
    }
}