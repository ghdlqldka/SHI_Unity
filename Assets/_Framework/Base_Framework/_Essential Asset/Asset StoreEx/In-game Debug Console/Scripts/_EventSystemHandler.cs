using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#endif

namespace IngameDebugConsole
{
	// Avoid multiple EventSystems in the scene by activating the embedded EventSystem only if one doesn't already exist in the scene
	[DefaultExecutionOrder( 1000 )]
	public class _EventSystemHandler : EventSystemHandler
	{
		//
	}
}