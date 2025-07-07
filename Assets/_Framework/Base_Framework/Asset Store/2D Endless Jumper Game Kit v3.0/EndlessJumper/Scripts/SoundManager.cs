using UnityEngine;
using System.Collections;

namespace EndlessJumper
{

	public class SoundManager : MonoBehaviour
	{

		//plays sfx - SFX provided by Freesfx.co.uk (Free)
		public AudioClip[] sfx;


		static SoundManager _instance;
		public static SoundManager Instance
		{
			get
			{
				return _instance;
			}
		}

		void Awake()
		{
#pragma warning disable 0618
            if (GameObject.FindObjectsOfType<SoundManager>().Length > 1)
#pragma warning restore 0618
            {
                Destroy(this.gameObject);
				return;
			}

			_instance = this;
		}

		public void PlayButtonTapSound()
		{
			this.GetComponent<AudioSource>().PlayOneShot(sfx[3]);
		}

		public virtual void PlaySFX(int id)
		{

			//play sound effect by id
			this.GetComponent<AudioSource>().PlayOneShot(sfx[id]);

		}

		public void StopSFX()
		{
			this.GetComponent<AudioSource>().Stop();
		}
	}
}