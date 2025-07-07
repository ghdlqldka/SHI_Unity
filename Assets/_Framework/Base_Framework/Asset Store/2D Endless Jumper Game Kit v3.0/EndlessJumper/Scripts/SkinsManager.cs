using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinsManager : MonoBehaviour {

	static SkinsManager _instance;
	public static SkinsManager Instance {
		get {
			return _instance;
		}
	}

	void Awake ()
	{
#pragma warning disable 0618
        if (GameObject.FindObjectsOfType<SkinsManager> ().Length > 1) {
#pragma warning restore 0618
            Destroy(this.gameObject);
			return;
		}

		_instance = this;
	}

	public List<Skin> skins = new List<Skin> ();

	[System.Serializable]
	public class Skin {
		public bool isUnlockedByDefault = false;
		public int price;
		public Sprite spriteCharacterThumbnail;
		public Sprite spriteCharacter;
	}
}
