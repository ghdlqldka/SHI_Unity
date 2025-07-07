[System.Serializable]
public class XMLData : IBpsData
{
    //ȭ����� 
    public string fileNm;                  // ���ϸ�
    public string motionMdyDt;             // ���� ����
    public string motionXmlTextData;       // ��� XML �ؽ�Ʈ ������
    public string motionXmlData;
    public int sn;                         // �Ϸù�ȣ
    public string updateContent;           // ������Ʈ ����
    public string mdfyNm;                  // ������ �̸�
    public int incomBlkSeq;

    public string projNo;                  // ������Ʈ ��ȣ
    public string blkNo;                   // ��� ��ȣ
    public string motionRmrk;              // ��� ���
    public string motionRegdt;             // ��� ����
    public int motionRegUsrSeq;            // ����� ������
    public string regNm;                   // ����� �̸�
    public int motionMdfyUsrSeq;           // ������ ������

    public int motionXmlSeq;               // ��� XML ������

    //public string fileurl;
    public XMLData(string fileNm, string motionRmrk, string motionXmlTextData, string updateContent, int incomBlkSeq)
    {
        this.fileNm = fileNm;
        this.motionXmlTextData = motionXmlTextData;
        this.updateContent = updateContent;
        this.motionRmrk = motionRmrk;
        this.incomBlkSeq = incomBlkSeq;

    }
}
