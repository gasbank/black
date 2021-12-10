using UnityEngine;

[DisallowMultipleComponent]
public class PlatformReport : MonoBehaviour {
    [SerializeField]
    PlatformInterface platformInterface;
    
    // 버그 메일 보내기 (세이브 파일 첨부) 기능을 위한 아래 함수는 Unity 이벤트 핸들러로서 연결되어 있으므로
    // Visual Studio에서 참고(레퍼런스) 체크 시 검사되지 않음
    // 사용되지 않는 것이 아니므로 삭제하지 말 것...
    public void ReportBugByMailSaveFileOnUiThread() {
        var reportPopupTitle = PlatformInterface.instance.textHelper.GetText("platform_report_popup_title");
        var mailTo = PlatformInterface.instance.textHelper.GetText("platform_report_mail");
        var subject = PlatformInterface.instance.textHelper.GetText("platform_report_subject");
        var text = PlatformInterface.instance.textHelper.GetText("platform_report_text");
        var saveData = PlatformInterface.instance.saveUtil.SerializeSaveData();

        Platform.instance.Report(reportPopupTitle, mailTo, subject, text, saveData);
    }
}
