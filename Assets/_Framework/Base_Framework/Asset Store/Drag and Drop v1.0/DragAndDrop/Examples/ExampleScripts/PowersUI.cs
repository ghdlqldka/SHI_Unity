using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _DragAndDrop
{

	public class PowersUI : ObjectContainerList<Power>
	{

		public Player player;

		// Use this for initialization
		void Start()
		{
			CreateSlots(player.powers);
		}
	}
}