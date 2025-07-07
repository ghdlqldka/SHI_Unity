[System.Serializable]
public class XMLData : IBpsData
{
    //화면출력 
    public string fileNm;                  // 파일명
    public string motionMdyDt;             // 수정 일자
    public string motionXmlTextData;       // 모션 XML 텍스트 데이터
    public string motionXmlData;
    public int sn;                         // 일련번호
    public string updateContent;           // 업데이트 내용
    public string mdfyNm;                  // 수정자 이름
    public int incomBlkSeq;

    public string projNo;                  // 프로젝트 번호
    public string blkNo;                   // 블록 번호
    public string motionRmrk;              // 모션 비고
    public string motionRegdt;             // 등록 일자
    public int motionRegUsrSeq;            // 등록자 시퀀스
    public string regNm;                   // 등록자 이름
    public int motionMdfyUsrSeq;           // 수정자 시퀀스

    public int motionXmlSeq;               // 모션 XML 시퀀스

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
