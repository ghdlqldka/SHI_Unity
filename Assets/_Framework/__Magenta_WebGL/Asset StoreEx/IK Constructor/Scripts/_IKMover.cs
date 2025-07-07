using UnityEngine;

[ExecuteInEditMode]
public class _IKMover : IKMover
{
    private static string LOG_FORMAT = "<color=#00FF1C><b>[_IKMover]</b></color> {0}";

    protected virtual void Awake()
    {
#if DEBUG
        DrawVisual = true;
#else
        DrawVisual = false;
#endif
    }

    protected override void Start()
    {
        Debug.LogFormat(LOG_FORMAT, "Start(), this.gameObject.name : <b>" + this.gameObject.name + "</b>");

        if (Application.isPlaying == false)
            return;

        if (IsInit == false)
            Init();
    }

    protected override void LateUpdate()
    {
        // Update the object automatically if set
        if (AutoUpdate == true)
        {
            ManualUpdate();
        }

#if DEBUG
        // Draws a debug visual if set
        if (DrawVisual == true)
        {
            DrawDebug();
        }
#endif
    }

    public override bool Init()
    {
        if (Origin == null)
        {
            Debug.Assert(false);
            IsInit = false;
            return false;
        }

        StartPosition = Origin.localPosition;
        IsInit = true;

        return true;
    }

    public override void ManualUpdate()
    {
        float swap;
        bool isLimited;

        if (Origin == null)
        {
            IsInit = false;
            return;
        }

        if (IsInit == false)
        {
            if (Init() == false)
            {
                return;
            }
        }

        if (Minimum > Maximum)
        {
            swap = Minimum;
            Minimum = Maximum;
            Maximum = swap;
        }

        Speed = Mathf.Clamp(Speed, 0, float.PositiveInfinity);
        AccelerationTime = Mathf.Clamp(AccelerationTime, 0, float.PositiveInfinity);

        // Determines what type of Kinematic in use
        if (Kinematic == KinematicType.Inverse)
        {
            // We're in Inverse mode
            // Doing position measurement
            if (SolveIKPosition(Origin, Target, ref TargetPosition) == false)
            {
                return;
            }
        }
        else
        {
            // We're in Forward mode
            // Checking if follow sub-mode is active, if so
            // overriding user setting by one of the fields
            // from another IK-script supplied, otherwise letting
            // user to control position directly
            if (MasterMover != null)
            {
                switch (MasterField)
                {
                    case MasterSource.TargetValue: 
                        TargetPosition = MasterMover.TargetPosition;
                        break;
                    case MasterSource.AlteredValue: 
                        TargetPosition = MasterMover.AlteredPosition; 
                        break;
                    case MasterSource.CurrentValue: 
                        TargetPosition = MasterMover.CurrentPosition;
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }
        }

        // Convertion stage
        AlteredPosition = ConvertPosition(TargetPosition);

        // Limit stage, can be inactive
        if (Limits == true)
            isLimited = LimitPosition(Minimum, Maximum, ref AlteredPosition);
        else
            isLimited = false;

        // Animation stage
        AnimatePosition(ref CurrentPosition, AlteredPosition, isLimited);

        // Setting position
        SetPosition(CurrentPosition);
    }

    protected override void AnimatePosition(ref float destPosition, float srcPosition, bool isLimited)
    {
        AnimationType animType;
        float moveDelta;

        // Now evaluate type of movement
        animType = AnimationType.Instant;
        if (Speed > 0)
        {
            animType = AnimationType.ConstSpeed;
            if (AccelerationTime > 0)
                animType = AnimationType.Accelerated;
        }

        // Move, according to movement type
        switch (animType)
        {
            // Performing instant move
            case AnimationType.Instant:
                destPosition = srcPosition;
                SetFinishState(true, isLimited);
                break;

            // Moving towards AlteredPosition with constant speed
            case AnimationType.ConstSpeed:
                moveDelta = 0;
                if (srcPosition != destPosition)
                    moveDelta = Speed * Mathf.Sign(destPosition - srcPosition) * Time.deltaTime;

                if (Mathf.Abs(srcPosition - destPosition) <= moveDelta || moveDelta == 0)
                {
                    destPosition = srcPosition;
                    SetFinishState(true, isLimited);
                }
                else
                {
                    destPosition += moveDelta;
                    SetFinishState(false, isLimited);
                }
                break;

            // Moving to AlteredPosition with acceleration and start push brakes before we reach it
            case AnimationType.Accelerated:
                destPosition = IKUtility.IKRamp(destPosition, srcPosition, Speed, AccelerationTime, ref CurrentSpeed, Time.deltaTime);

                if (destPosition == srcPosition)
                    SetFinishState(true, isLimited);
                else
                    SetFinishState(false, isLimited);
                break;

            default:
                Debug.Assert(false);
                break;
        }
    }

    protected override void SetFinishState(bool state, bool isLimited)
    {
        IsFinished = state;

        if (state)
        {
            IsTargetReached = !isLimited;
        }
        else
        {
            IsTargetReached = false;
        }
    }

    protected override void DrawDebug()
    {
        Vector3 startPos;
        Vector3 v1, v2;
        Vector3 v3, v4;

        if (Origin == null)
            return;

        if (Origin.parent != null)
            startPos = Origin.parent.transform.TransformPoint(StartPosition);
        else
            startPos = StartPosition;

        // Drawing a marker of default position
        v1 = startPos - Origin.right * VisualSize / 2;
        v2 = startPos + Origin.right * VisualSize / 2;
        Debug.DrawLine(v1, v2, Color.white);

        // Drawing main axis
        v1 = startPos;
        v2 = startPos + Origin.forward * VisualSize / 2;
        Debug.DrawLine(v1, v2, Color.white);

        v2 = Origin.position;
        Debug.DrawLine(v1, v2, VisualLineColor);

        // Drawing a marker of current position
        v1 = Origin.position - Origin.forward * VisualSize / 2 - Origin.right * VisualSize / 2;
        v2 = Origin.position + Origin.forward * VisualSize / 2 - Origin.right * VisualSize / 2;
        v3 = Origin.position - Origin.forward * VisualSize / 2 + Origin.right * VisualSize / 2;
        v4 = Origin.position + Origin.forward * VisualSize / 2 + Origin.right * VisualSize / 2;

        Debug.DrawLine(v1, v2, VisualMarkerColor);
        Debug.DrawLine(v2, v4, VisualMarkerColor);
        Debug.DrawLine(v4, v3, VisualMarkerColor);
        Debug.DrawLine(v3, v1, VisualMarkerColor);
        Debug.DrawLine(v1, v4, VisualMarkerColor);
        Debug.DrawLine(v2, v3, VisualMarkerColor);
    }
}
