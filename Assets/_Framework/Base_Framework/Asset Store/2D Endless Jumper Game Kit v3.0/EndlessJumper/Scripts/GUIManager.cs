using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class GUIManager: MonoBehaviour {


	public List<GameObject> panels;
	protected static GUIManager _instance;

	public GameObject currentPanelObject {
		get {
			return panelStack.Peek();
		}
	}

	public Stack<GameObject> panelStack = new Stack<GameObject> ();
	public int defaultPanel;

	public static GUIManager Instance {
		get {
			return _instance;
		}
	}

	protected virtual void Awake ()
	{
#pragma warning disable 0618
        if (GameObject.FindObjectsOfType<GUIManager> ().Length > 1) {
#pragma warning restore 0618
            Destroy(this.gameObject);
			return;
		}

		_instance = this;

		#if UNITY_IOS
		Application.targetFrameRate = 60;
		#endif
	}

	protected virtual void OnEnable()
	{
		OpenPanel (defaultPanel);

	}


	public virtual void OpenPanel(int id, bool hidePrevious = false)
	{
		if (hidePrevious) {
			if (panelStack.Peek () != null) {
				currentPanelObject.SetActive (false);
				panelStack.Pop ();
			}
			panelStack.Push (panels[id]);
			currentPanelObject.SetActive (true);
		} else {
			panelStack.Push (panels[id]);
			currentPanelObject.SetActive (true);
		}
	}

	public void Back()
	{
		if (panelStack.Peek () != null) {
			currentPanelObject.SetActive (false);
			panelStack.Pop ();
		}
		currentPanelObject.SetActive (true);
	}

	public void ButtonLeaderboard()
	{
#pragma warning disable 0618
        Social.ShowLeaderboardUI ();
#pragma warning restore 0618
    }

    public void ButtonRate()
	{
		Application.OpenURL ("https://www.yourrateurlhere.com");
	}

}

