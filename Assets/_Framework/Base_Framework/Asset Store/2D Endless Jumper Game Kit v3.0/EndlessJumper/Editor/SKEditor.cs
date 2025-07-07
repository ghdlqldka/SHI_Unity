using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor (typeof(SKEditor))]
public class SKEditor : Editor {
	

	[MenuItem ("Tools/Saad Khawaja/Endless 2D Jumper/Delete Preferences  %#-")]
	static void DeletePreferences()
	{
		PlayerPrefs.DeleteAll ();
		PlayerPrefs.Save ();

	}

	[MenuItem ("Tools/Saad Khawaja/Endless 2D Jumper/Add 1000 Coins  %#-")]
	static void AddCoins()
	{
		PlayerPrefs.SetInt ("Coins", PlayerPrefs.GetInt ("Coins") + 1000);
		PlayerPrefs.Save ();

	}
}