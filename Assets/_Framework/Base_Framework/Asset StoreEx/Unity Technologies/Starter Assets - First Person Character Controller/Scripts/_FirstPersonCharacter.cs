using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class _FirstPersonCharacter : MonoBehaviour
	{
		private static string LOG_FORMAT = "<color=magenta><b>[_FirstPersonCharacter]</b></color> {0}";

		[ReadOnly]
		[SerializeField]
		protected _FirstPersonController controller;
		[ReadOnly]
		[SerializeField]
		protected BasicRigidBodyPush rigidBodyPush;

		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		[SerializeField]
		protected float moveSpeed = 4.0f;
		public float MoveSpeed
		{
			get
			{
				return moveSpeed;
			}
			set
			{
				moveSpeed = value;
				controller.MoveSpeed = value;
			}
		}

		[Tooltip("Sprint speed of the character in m/s")]
		[SerializeField]
		protected float sprintSpeed = 5.335f;
		public float SprintSpeed
		{
			get
			{
				return sprintSpeed;
			}
			set
			{
				sprintSpeed = value;
				controller.SprintSpeed = value;
			}
		}

		[Tooltip("Rotation speed of the character")]
		[Range(0.0f, 2f)]
		[SerializeField]
		protected float rotationSpeed = 0.12f;
		public float RotationSpeed
		{
			get
			{
				return rotationSpeed;
			}
			set
			{
				rotationSpeed = value;
				controller.RotationSpeed = value;
			}
		}

		[Tooltip("Acceleration and deceleration")]
		[SerializeField]
		protected float speedChangeRate = 10.0f;
		public float SpeedChangeRate
		{
			get
			{
				return speedChangeRate;
			}
			set
			{
				speedChangeRate = value;
				controller.SpeedChangeRate = value;
			}
		}

		[Space(10)]
		[Tooltip("The height the player can jump")]
		[SerializeField]
		protected float jumpHeight = 1.2f;
		public float JumpHeight
		{
			get
			{
				return jumpHeight;
			}
			set
			{
				jumpHeight = value;
				controller.JumpHeight = value;
			}
		}

		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		[SerializeField]
		protected float gravity = -15.0f;
		public float Gravity
		{
			get
			{
				return gravity;
			}
			set
			{
				gravity = value;
				controller.Gravity = value;
			}
		}

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		[SerializeField]
		protected float jumpTimeout = 0.50f;
		public float JumpTimeout
		{
			get
			{
				return jumpTimeout;
			}
			set
			{
				jumpTimeout = value;
				controller.JumpTimeout = value;
			}
		}

		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		[SerializeField]
		protected float fallTimeout = 0.15f;
		public float FallTimeout
		{
			get
			{
				return fallTimeout;
			}
			set
			{
				fallTimeout = value;
				controller.FallTimeout = value;
			}
		}

		[Tooltip("Useful for rough ground")]
		[SerializeField]
		protected float groundedOffset = -0.14f;
		public float GroundedOffset
		{
			get
			{
				return groundedOffset;
			}
			set
			{
				groundedOffset = value;
				controller.GroundedOffset = value;
			}
		}

		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		[SerializeField]
		protected float groundedRadius = 0.28f;
		public float GroundedRadius
		{
			get
			{
				return groundedRadius;
			}
			set
			{
				groundedRadius = value;
				controller.GroundedRadius = value;
			}
		}

		[Tooltip("What layers the character uses as ground")]
		[SerializeField]
		protected LayerMask groundLayers;
		public LayerMask GroundLayers
		{
			get
			{
				return this.groundLayers;
			}
			set
			{
				groundLayers = value;
				controller.GroundLayers = value;
			}
		}

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the \"Cinemachine Virtual Camer\"a that the camera will follow")]
		[SerializeField]
		protected GameObject cinemachineCameraFollow;
		public GameObject CinemachineCameraFollow
		{
			get
			{
				return cinemachineCameraFollow;
			}
			set
			{
				cinemachineCameraFollow = value;
				controller.CinemachineCameraTarget = value;
			}
		}

		[Tooltip("How far in degrees can you move the camera up")]
		[SerializeField]
		protected float topClamp = 90.0f;
		public float TopClamp
		{
			get
			{
				return topClamp;
			}
			set
			{
				topClamp = value;
				controller.TopClamp = value;
			}
		}

		[Tooltip("How far in degrees can you move the camera down")]
		[SerializeField]
		protected float bottomClamp = -90.0f;
		public float BottomClamp
		{
			get
			{
				return bottomClamp;
			}
			set
			{
				bottomClamp = value;
				controller.BottomClamp = value;
			}
		}

		[Header("Rigid Body Push")]
		[SerializeField]
		protected LayerMask pushLayers;
		public LayerMask PushLayers
		{
			get
			{
				return pushLayers;
			}
			set
			{
				pushLayers = value;
				rigidBodyPush.pushLayers = value;
			}
		}

		[SerializeField]
		protected bool canPush;
		public bool CanPush
		{
			get
			{
				return canPush;
			}
			set
			{
				canPush = value;
				rigidBodyPush.canPush = value;
			}
		}
		[Range(0.5f, 5f)]
		[SerializeField]
		protected float strength = 1.1f;
		public float Strength
		{
			get
			{
				return strength;
			}
			set
			{
				strength = value;
				rigidBodyPush.strength = value;
			}
		}

		protected virtual void Awake()
		{
			Debug.LogFormat(LOG_FORMAT, "Awake()");

			Debug.Assert(controller != null);
			Debug.Assert(rigidBodyPush);

			// Player
			controller.MoveSpeed = MoveSpeed;
			controller.SprintSpeed = SprintSpeed;
			controller.RotationSpeed = RotationSpeed;
			controller.SpeedChangeRate = SpeedChangeRate;

			controller.JumpHeight = JumpHeight;
			controller.Gravity = Gravity;

			controller.GroundedOffset = GroundedOffset;
			controller.GroundedRadius = GroundedRadius;
			controller.GroundLayers = GroundLayers;

			// Cinemachine
			controller.CinemachineCameraTarget = CinemachineCameraFollow;
			controller.TopClamp = TopClamp;
			controller.BottomClamp = BottomClamp;

			// Rigid Body Push
			rigidBodyPush.pushLayers = PushLayers;
			rigidBodyPush.canPush = canPush;
			rigidBodyPush.strength = strength;
		}

		protected virtual void Update()
		{
#if DEBUG
			// Player
			controller.MoveSpeed = MoveSpeed;
			controller.SprintSpeed = SprintSpeed;
			controller.RotationSpeed = RotationSpeed;
			controller.SpeedChangeRate = SpeedChangeRate;

			controller.JumpHeight = JumpHeight;
			controller.Gravity = Gravity;

			controller.GroundedOffset = GroundedOffset;
			controller.GroundedRadius = GroundedRadius;
			controller.GroundLayers = GroundLayers;

			// Cinemachine
			controller.CinemachineCameraTarget = CinemachineCameraFollow;
			controller.TopClamp = TopClamp;
			controller.BottomClamp = BottomClamp;

			// Rigid Body Push
			rigidBodyPush.pushLayers = PushLayers;
			rigidBodyPush.canPush = canPush;
			rigidBodyPush.strength = strength;
#endif
		}
	}
}