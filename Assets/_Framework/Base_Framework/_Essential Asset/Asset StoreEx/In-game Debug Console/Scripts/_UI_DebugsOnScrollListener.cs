using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Listens to scroll events on the scroll rect that debug items are stored
// and decides whether snap to bottom should be true or not
// 
// Procedure: if, after a user input (drag or scroll), scrollbar is at the bottom, then 
// snap to bottom shall be true, otherwise it shall be false
namespace IngameDebugConsole
{
	public class _UI_DebugsOnScrollListener : DebugsOnScrollListener
	{
		//
	}
}