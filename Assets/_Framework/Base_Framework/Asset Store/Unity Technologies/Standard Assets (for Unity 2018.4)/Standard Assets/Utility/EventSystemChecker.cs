using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemChecker : MonoBehaviour
{
    //public GameObject eventSystem;

	// Use this for initialization
	void Awake ()
	{
#pragma warning disable 0618
        if (!FindObjectOfType<EventSystem>())
#pragma warning restore 0618
        {
            //Instantiate(eventSystem);
            GameObject obj = new GameObject("EventSystem");
            obj.AddComponent<EventSystem>();
#pragma warning disable 0618
            obj.AddComponent<StandaloneInputModule>().forceModuleActive = true;
#pragma warning restore 0618
        }
    }
}
