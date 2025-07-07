using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessJumper
{

	public class UI_Skins : MonoBehaviour
	{

		public List<GameObject> skinItems = new List<GameObject>();
		public GameObject item;
		public GameObject panelSkins;
		public Text textCoins;
		// Use this for initialization
		void Start()
		{
			if (!PlayerPrefs.HasKey("Coins"))
			{
				PlayerPrefs.SetInt("Coins", 100);
			}
		}

		// Update is called once per frame
		void Update()
		{

		}

		public void ButtonBack()
		{
			GUIManager.Instance.Back();
			SoundManager.Instance.PlayButtonTapSound();
		}

		void OnEnable()
		{
			RefreshList();
			UpdateCoins();
		}

		void UpdateCoins()
		{
			textCoins.text = PlayerPrefs.GetInt("Coins", 100).ToString();
		}

		void RefreshList()
		{
			//Destroy current items in the list to refresh
			foreach (GameObject gObj in skinItems)
			{
				Destroy(gObj);
			}

			//Create items of the skins
			for (int i = 0; i < SkinsManager.Instance.skins.Count; i++)
			{
				GameObject gObj = Instantiate(item) as GameObject;
				gObj.transform.Find("Image_Character").GetComponent<Image>().sprite = SkinsManager.Instance.skins[i].spriteCharacterThumbnail;

				//If skin is unlocked, hide the coins below it
				if (PlayerPrefs.GetInt("isSkin" + i + "Unlocked", 0) == 0 && !SkinsManager.Instance.skins[i].isUnlockedByDefault)
				{
					gObj.transform.Find("Image_Coins").gameObject.SetActive(true);
					gObj.transform.Find("Image_Coins/Text_Coins").GetComponent<Text>().text = SkinsManager.Instance.skins[i].price.ToString();
				}
				else
				{
					gObj.transform.Find("Image_Coins").gameObject.SetActive(false);
				}

				//If skin is selected, enable the selected background sprite
				if (i == PlayerPrefs.GetInt("selectedSkin", 0))
				{
					gObj.transform.Find("Image_Selected").gameObject.SetActive(true);
					gObj.transform.Find("Image_Unselected").gameObject.SetActive(false);
				}
				else
				{
					gObj.transform.Find("Image_Selected").gameObject.SetActive(false);
					gObj.transform.Find("Image_Unselected").gameObject.SetActive(true);
				}

				//Very important - each gameobject is now named after the skin id
				gObj.name = i.ToString();
				gObj.transform.parent = panelSkins.transform;
				gObj.transform.localScale = Vector3.one;
				gObj.SetActive(true);
				skinItems.Add(gObj);
			}
		}

		public void ItemClicked(GameObject itemObject)
		{
			//Skin Item is clicked - each item is named after its id, so we get the id from the name.
			int id = int.Parse(itemObject.name);
			SkinsManager.Skin skin = SkinsManager.Instance.skins[id];

			//If it is unlocked by default, select it
			if (skin.isUnlockedByDefault || (PlayerPrefs.GetInt("isSkin" + id + "Unlocked") == 1))
			{
				SelectSkin(id);
				SoundManager.Instance.PlaySFX(6);
			}
			else
			{
				if (skin.price <= PlayerPrefs.GetInt("Coins", 100))
				{
					//User has enough coins, purchase the skin
					PlayerPrefs.SetInt("Coins", (PlayerPrefs.GetInt("Coins") - skin.price));
					PlayerPrefs.SetInt("isSkin" + id + "Unlocked", 1);
					//Select the skin and refresh the list
					SelectSkin(id);
					UpdateCoins();
					SoundManager.Instance.PlaySFX(4);
				}
				else
				{
					//Not enough coins - play an error sound
					SoundManager.Instance.PlaySFX(5);
				}
			}
		}

		void SelectSkin(int id)
		{
			//Select skin
			PlayerPrefs.SetInt("selectedSkin", id);
			RefreshList();
		}
	}
}