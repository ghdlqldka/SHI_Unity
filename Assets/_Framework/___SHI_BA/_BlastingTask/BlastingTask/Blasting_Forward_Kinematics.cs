using UnityEngine;
using BioIK; // BioIK 네임스페이스 사용

public static class BlastingKinematics
{
    /// <summary>
    /// BioIK 시스템의 현재 관절값(Solution)으로 엔드 이펙터의 월드 포즈(위치 및 회전)를 계산합니다.
    /// </summary>
    /// <param name="bioIKSystem">타겟 BioIK 시스템입니다.</param>
    /// <param name="endEffectorSegment">월드 포즈를 알고자 하는 엔드 이펙터 BioSegment입니다.</param>
    /// <param name="worldPosition">계산된 월드 위치를 반환합니다.</param>
    /// <param name="worldRotation">계산된 월드 회전을 반환합니다.</param>
    /// <returns>계산 성공 여부를 반환합니다.</returns>
    public static bool GetWorldPoseCurrent(BioIK.BioIK bioIKSystem, BioSegment endEffectorSegment, out Vector3 worldPosition, out Quaternion worldRotation)
    {
        worldPosition = Vector3.zero;
        worldRotation = Quaternion.identity;

        if (bioIKSystem == null || bioIKSystem.Evolution == null || bioIKSystem.Evolution.GetModel() == null)
        {
            Debug.LogError("BioIK 시스템 또는 내부 모델이 초기화되지 않았습니다.");
            return false;
        }

        if (endEffectorSegment == null)
        {
            Debug.LogError("엔드 이펙터 세그먼트가 지정되지 않았습니다.");
            return false;
        }

        Model robotModel = bioIKSystem.Evolution.GetModel();
        double[] currentJoints = bioIKSystem.Solution;

        if (currentJoints == null || currentJoints.Length != robotModel.GetDoF())
        {
            Debug.LogError("현재 관절값(Solution)이 없거나 길이가 DoF와 일치하지 않습니다.");
            return false;
        }

        // 현재 모델의 configuration을 백업합니다.
        double[] originalConfiguration = new double[robotModel.GetDoF()];
        System.Array.Copy(currentJoints, originalConfiguration, robotModel.GetDoF());

        // 최신 관절값으로 FK 계산
        robotModel.FK(currentJoints);

        Model.Node eeNode = robotModel.FindNode(endEffectorSegment.Transform);
        if (eeNode == null)
        {
            Debug.LogError("엔드 이펙터에 해당하는 노드를 모델에서 찾을 수 없습니다.");
            // 모델 상태 복원
            robotModel.FK(originalConfiguration);
            return false;
        }

        worldPosition = new Vector3((float)eeNode.WPX, (float)eeNode.WPY, (float)eeNode.WPZ);
        worldRotation = new Quaternion((float)eeNode.WRX, (float)eeNode.WRY, (float)eeNode.WRZ, (float)eeNode.WRW);

        // FK 계산 후 모델을 원래 configuration으로 복원
        robotModel.FK(originalConfiguration);

        return true;
    }

    public struct Pose6D
    {
        public Vector3 position; // x, y, z
        public Vector3 eulerZYX; // roll(X), pitch(Y), yaw(Z) in degrees

        public Pose6D(Vector3 pos, Vector3 eul)
        {
            position = pos;
            eulerZYX = eul;
        }
    }
    /// <summary>
    /// BioIK 모델에서 base~TCP까지의 로컬 위치/회전을 계산합니다.
    /// </summary>
    /// <param name="bioIKSystem">BioIK 시스템</param>
    /// <param name="baseSegment">기준이 되는 base BioSegment (예: "base")</param>
    /// <param name="tcpSegment">TCP 역할의 BioSegment (예: "TCP")</param>
    /// <param name="localPosition">base 기준 TCP의 로컬 위치</param>
    /// <param name="localRotation">base 기준 TCP의 로컬 회전</param>
    /// <returns>계산 성공 여부</returns>
        
    public static bool GetLocalPoseCurrent(
        BioIK.BioIK bioIKSystem,
        BioSegment baseSegment,
        BioSegment tcpSegment,
        out Pose6D localPose)
    {
        localPose = new Pose6D();

        if (bioIKSystem == null || bioIKSystem.Evolution == null || bioIKSystem.Evolution.GetModel() == null)
        {
            Debug.LogError("BioIK 시스템 또는 내부 모델이 초기화되지 않았습니다.");
            return false;
        }
        if (baseSegment == null || tcpSegment == null)
        {
            Debug.LogError("baseSegment 또는 tcpSegment가 지정되지 않았습니다.");
            return false;
        }

        Model robotModel = bioIKSystem.Evolution.GetModel();
        double[] currentJoints = bioIKSystem.Solution;

        if (currentJoints == null || currentJoints.Length != robotModel.GetDoF())
        {
            Debug.LogError("현재 관절값(Solution)이 없거나 길이가 DoF와 일치하지 않습니다.");
            return false;
        }

        double[] originalConfiguration = new double[robotModel.GetDoF()];
        System.Array.Copy(currentJoints, originalConfiguration, robotModel.GetDoF());

        robotModel.FK(currentJoints);

        Model.Node baseNode = robotModel.FindNode(baseSegment.Transform);
        Model.Node tcpNode = robotModel.FindNode(tcpSegment.Transform);

        if (baseNode == null || tcpNode == null)
        {
            Debug.LogError("base 또는 TCP에 해당하는 노드를 모델에서 찾을 수 없습니다.");
            robotModel.FK(originalConfiguration);
            return false;
        }

        Vector3 baseWorldPos = new Vector3((float)baseNode.WPX, (float)baseNode.WPY, (float)baseNode.WPZ);
        Quaternion baseWorldRot = new Quaternion((float)baseNode.WRX, (float)baseNode.WRY, (float)baseNode.WRZ, (float)baseNode.WRW);

        Vector3 tcpWorldPos = new Vector3((float)tcpNode.WPX, (float)tcpNode.WPY, (float)tcpNode.WPZ);
        Quaternion tcpWorldRot = new Quaternion((float)tcpNode.WRX, (float)tcpNode.WRY, (float)tcpNode.WRZ, (float)tcpNode.WRW);

        // base 기준 TCP의 로컬 pose 계산
        Vector3 localPosition = Quaternion.Inverse(baseWorldRot) * (tcpWorldPos - baseWorldPos);
        Quaternion localRotation = Quaternion.Inverse(baseWorldRot) * tcpWorldRot;

        // 오일러 각 (ZYX, 즉 Unity의 eulerAngles는 기본적으로 ZYX)
        Vector3 eulerZYX = localRotation.eulerAngles;

        localPose = new Pose6D(localPosition, eulerZYX);

        robotModel.FK(originalConfiguration);

        return true;
    }
}