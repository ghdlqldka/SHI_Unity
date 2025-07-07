using System.Collections.Generic;
using UnityEngine;
using static Plane_and_NV;

namespace _SHI_BA
{
    [System.Serializable]
    public class CustomVertex { public float x, y, z; }

    [System.Serializable]
    public class BlockPos { public float x, y, z, roll, pitch, yaw; }


    public class BA_Motion
    {
        [System.Serializable]
        public class CustomQuad
        {
            public CustomVertex top_left, top_right, bottom_right, bottom_left;
            public CustomVertex edge_1_start, edge_1_end;
            public CustomVertex edge_2_start, edge_2_end;
            public CustomVertex edge_3_start, edge_3_end;
            public CustomVertex edge_4_start, edge_4_end;
            public CustomVertex edge_5_start, edge_5_end;
            public CustomVertex edge_6_start, edge_6_end;
            public CustomVertex edge_7_start, edge_7_end;
            public CustomVertex edge_8_start, edge_8_end;
        }

        [System.Serializable]
        public class CustomRoot
        {
            public BlockPos block_pos;
            public CustomQuad W1, W2, W3, W4;
            // ���� JSON���� R10���� ������, ���� Ȯ���� ���� �� ���� R �迭 ���� �ʵ带 �߰��� �� �ֽ��ϴ�.
            public CustomQuad R1, R2, R3, R4, R5, R6, R7, R8, R9, R10, R11, R12, R13, R14, R15, R16, R17, R18, R19, R20;
        }


        [System.Serializable]
        public struct Cube
        {
            public string Name;
            public Vector3 R1, R2, R3, R4;
            public Vector3 normal;
        }
        [System.Serializable]
        public struct Edge
        {
            public string Name;
            public Vector3 Start;
            public Vector3 End;
        }
    }
}
