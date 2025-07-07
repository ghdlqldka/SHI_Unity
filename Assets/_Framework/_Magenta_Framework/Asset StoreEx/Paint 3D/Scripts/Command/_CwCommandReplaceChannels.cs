using System.Collections.Generic;
using UnityEngine;

namespace PaintCore
{
	/// <summary>This class manages the replace channels painting command.</summary>
	public class _CwCommandReplaceChannels : CwCommandReplaceChannels
    {
		
		public static new _CwCommandReplaceChannels Instance = new _CwCommandReplaceChannels();
        // private static Stack<CwCommandReplaceChannels> pool = new Stack<CwCommandReplaceChannels>();
        protected static Stack<_CwCommandReplaceChannels> pool = new Stack<_CwCommandReplaceChannels>();

        static _CwCommandReplaceChannels()
		{
            // BuildMaterial(ref cachedMaterial, ref cachedMaterialHash, "Hidden/Paint Core/CwReplaceChannels");
            CwCommandReplaceChannels.Instance = Instance;

        }

        public override void Pool()
        {
            pool.Push(this);
        }

        public override CwCommand SpawnCopy()
        {
            _CwCommandReplaceChannels command = SpawnCopy(pool);

            command.TextureR = TextureR;
            command.TextureG = TextureG;
            command.TextureB = TextureB;
            command.TextureA = TextureA;
            command.ChannelR = ChannelR;
            command.ChannelG = ChannelG;
            command.ChannelB = ChannelB;
            command.ChannelA = ChannelA;

            return command;
        }
    }
}