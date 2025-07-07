using UnityEngine;

namespace BioIK 
{

	//This objective aims to minimise the translational distance between the transform and the target.
	// [AddComponentMenu("")]
	public class _Position : Position
    {
        private static string LOG_FORMAT = "<color=#B604FF><b>[_Position]</b></color> {0}";

        // [SerializeField] private Transform Target;
        protected Transform _Target
        {
            get
            {
                return Target;
            }
            set
            {
                if (Target != value)
                {
                    Target = value;
                    Debug.LogWarningFormat(LOG_FORMAT, "@@@@@@@ <color=red>_Target has CHANGED</color>!!!!!!!!! @@@@@@@");
                }
            }
        }

        protected override void Awake()
        {
#if DEBUG
            Debug.LogWarningFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
            if (_Target != null)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "_Target : <b><color=yellow>" + _Target.name + "</color></b>");
            }
            else
            {
                Debug.LogWarningFormat(LOG_FORMAT, "<color=red>_Target is NULL</color>!!!!!!!!!!");
            }
#endif
        }

        protected override void OnDestroy()
        {
            //
        }

        public override void SetTargetTransform(Transform target)
        {
            _Target = target;
            if (_Target != null)
            {
                SetTargetPosition(_Target.position);
            }
        }

    }

}