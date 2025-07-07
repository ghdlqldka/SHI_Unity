[System.Serializable]
public class BPSData : IBpsData
{
    public int bpsDataSeq;       // Ű (PK)
    public string bpsDistance;   // �۾���Ÿ�
    public string bpsSprayAngle; // �л簢��
    public string bpsMotionGap;  // ��� ����
    public string bpsSpeed;      // ��Ʈ�� �ӵ�
    public string bpsAccel;      // ���ӵ�
    public string bpsRmrk;       // ���
    public string bpsShape;      //
    public string regDt;         // ����Ͻ� (���ڿ� ����)
    public string bpsMdfyDt;     // �����Ͻ� (���ڿ� ����)
    public string bpsFaceInfo;  // BPS �� ����

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
    public T[] data;       // JSON�� "data" �迭�� ��Ī
    public int last_page;  // JSON�� "last_page"�� ��Ī
}
