using UnityEngine;
using KoganeUnityLib.Example;

namespace _Base_Framework.TMPro_Typewriter
{
	public class ExampleEx : Example
	{
		private static string LOG_FORMAT = "<color=#00AEEF><b>[ExampleEx]</b></color> {0}";

		protected void OnCompletePlay()
		{
			Debug.LogFormat(LOG_FORMAT, "OnCompletePlay()");
		}

		protected override void Update()
		{
			if (Input.GetKeyDown(KeyCode.Z))
			{
				m_typewriter.Play("ABCDEFG HIJKLMN OPQRSTU", m_speed, OnCompletePlay);
			}

			if (Input.GetKeyDown(KeyCode.X))
			{
				m_typewriter.Play(@"<size=64>ABCDEFG</size> <color=red>HIJKLMN</color> <sprite=0> <link=""https://www.google.co.jp/"">OPQRSTU</link>", m_speed, OnCompletePlay);
			}

			if (Input.GetKeyDown(KeyCode.C))
			{
				m_typewriter.Play(@"<sprite=0><sprite=0><sprite=1><sprite=2><sprite=3><sprite=4><sprite=5><sprite=6><sprite=7><sprite=8><sprite=9><sprite=10>", m_speed, OnCompletePlay);
			}

			if (Input.GetKeyDown(KeyCode.V))
			{
				m_typewriter.Skip();
			}

			if (Input.GetKeyDown(KeyCode.B))
			{
				m_typewriter.Skip(false);
			}

			if (Input.GetKeyDown(KeyCode.N))
			{
				m_typewriter.Pause();
			}

			if (Input.GetKeyDown(KeyCode.M))
			{
				m_typewriter.Resume();
			}
		}
	}
}