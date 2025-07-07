using UnityEngine;

namespace __BioIK
{
	public class _Demo : Demo
    {
        private static string LOG_FORMAT = "<color=#8DA278><b>[_Demo]</b></color> {0}";

		protected virtual void Awake()
		{
			Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
		}

        protected override void Start()
		{
			Debug.LogFormat(LOG_FORMAT, "Start()");

			LoadModel(1);
		}

		public override void LoadModel(int index)
		{
			index -= 1;
			if (Models.Length > index)
			{
				if (ModelGO != null)
				{
					ModelGO.SetActive(false);
				}
				ModelCharacter = Models[index];
				ModelGO = ModelCharacter.transform.root.gameObject;
				ModelGO.SetActive(true);
				UpdateMotionType();
			}
		}
	}

}