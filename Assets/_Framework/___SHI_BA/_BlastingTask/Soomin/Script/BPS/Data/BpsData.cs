[System.Serializable]
public class BPSData : IBpsData
{
    public int bpsDataSeq;       // 키 (PK)
    public string bpsDistance;   // 작업면거리
    public string bpsSprayAngle; // 분사각도
    public string bpsMotionGap;  // 모션 간격
    public string bpsSpeed;      // 겐트리 속도
    public string bpsAccel;      // 가속도
    public string bpsRmrk;       // 비고
    public string bpsShape;      //
    public string regDt;         // 등록일시 (문자열 형태)
    public string bpsMdfyDt;     // 수정일시 (문자열 형태)
    public string bpsFaceInfo;  // BPS 면 정보

    public BPSData(string _bpsDistance,string _bpsMotionGap, string _bpsSpeed, string _bpsAccel)
    {
        bpsDistance = _bpsDistance;
        bpsSpeed = _bpsSpeed;
        bpsAccel = _bpsAccel;
        bpsMotionGap = _bpsMotionGap;
        bpsSprayAngle = "0";
        bpsMotionGap = "0";
        bpsSpeed = "0";
        bpsRmrk = "0";
        regDt = "0";
        bpsShape = "0";



    }
}
[System.Serializable]
public class Wrapper<T>
{
    public T[] data;       // JSON의 "data" 배열과 매칭
    public int last_page;  // JSON의 "last_page"와 매칭
}
