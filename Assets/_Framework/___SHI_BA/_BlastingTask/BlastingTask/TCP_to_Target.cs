// public void PrintTCPWorldPose(BioIK.BioIK bioIK)
// {
//     BioSegment tcpSegment = null;
//     foreach (var segment in bioIK.Segments)
//     {
//         if (segment != null && segment.Transform != null && segment.Transform.name == "TCP")
//         {
//             tcpSegment = segment;
//             break;
//         }
//     }

//     if (tcpSegment != null)
//     {
//         Vector3 pos = tcpSegment.Transform.position;
//         Quaternion rot = tcpSegment.Transform.rotation;
//         Debug.Log($"TCP 월드 위치: {pos}, 회전(오일러): {rot.eulerAngles}");
//     }
//     else
//     {
//         Debug.LogError("TCP라는 이름의 BioSegment를 찾을 수 없습니다.");
//     }
// }