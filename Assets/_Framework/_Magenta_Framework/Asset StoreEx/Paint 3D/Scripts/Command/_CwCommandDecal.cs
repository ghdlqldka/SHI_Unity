using System.Collections.Generic;
using UnityEngine;
using PaintCore;

namespace PaintIn3D
{
	/// <summary>This class manages the decal painting command.</summary>
	public class _CwCommandDecal : CwCommandDecal
    {
        private static string LOG_FORMAT = "<color=#06FF00><b>[_CwCommandDecal]</b></color> {0}";

        public static new _CwCommandDecal Instance = new _CwCommandDecal();
        private static Stack<_CwCommandDecal> pool = new Stack<_CwCommandDecal>();

        static _CwCommandDecal()
		{
            CwCommandDecal.Instance = Instance;
        }

        public override CwCommand SpawnCopy()
        {
            _CwCommandDecal command = SpawnCopy(pool);

            command.Blend = Blend;
            command.In3D = In3D;
            command.Position = Position;
            command.EndPosition = EndPosition;
            command.Position2 = Position2;
            command.EndPosition2 = EndPosition2;
            command.Extrusions = Extrusions;
            command.Clip = Clip;
            command.Matrix = Matrix;
            command.Direction = Direction;
            command.Color = Color;
            command.Opacity = Opacity;
            command.Hardness = Hardness;
            command.Wrapping = Wrapping;
            command.Texture = Texture;
            command.Shape = Shape;
            command.ShapeChannel = ShapeChannel;
            command.NormalFront = NormalFront;
            command.NormalBack = NormalBack;
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

        public override void Pool()
        {
            pool.Push(this);
        }

        public override void Apply(Material material)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "<b>Apply(), material : " + material + "<b>");

            base.Apply(material);
        }
    }
}