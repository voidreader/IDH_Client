using UnityEngine;
using System.Collections;
using Realtime.LITJson;

public class NAgreeInfo
{
    public NAgreeInfo()
    {
        theme = "BLUE";
        headerBackGradient = new string[] { };
        headerBottomColor = "";
        headerIconDrawable = "";
        headerTitle = "{none}";
        headerTitleColor = "";

        contentBackGradient = new string[] { };
        contentIconDrawable = "";
        contentIconColor = "";
        contentCheckColor = "";
        contentTitleColor = "";
        contentShowColor = "";

        footerBackGradient = new string[] { };
        footerButtonGradient = new string[] { };
        footerButtonOutlineColor = "";
        footerTitle = "";
        footerTitleColor = "";
        showNightPush = false;

        allMessage = "";
        termMessage = "";
        privacyMessage = "";
        nightPushMessage = "";
    }

    // 기본 테마
    public string theme { get; set; }

    // 타이틀
    // 배경 색상(gradient)
    public string[] headerBackGradient { get; set; }
    // 타이틀 영역 하단 라인 색상
    public string headerBottomColor { get; set; }
    // 아이콘 이미지 파일명(aos - drawable / ios - bundle)
    public string headerIconDrawable { get; set; }
    // 제목
    public string headerTitle { get; set; }
    // 제목 색상
    public string headerTitleColor { get; set; }

    // 컨텐츠
    // 배경 색상(gradient)
    public string[] contentBackGradient { get; set; }
    // 아이콘 이미지 파일명(aos - drawable / ios - bundle)
    public string contentIconDrawable { get; set; }
    // 아이콘 색상
    public string contentIconColor { get; set; }
    // 체크버튼 색상
    public string contentCheckColor { get; set; }
    // 체크내용 색상
    public string contentTitleColor { get; set; }
    // 보기문구 색상
    public string contentShowColor { get; set; }

    // 하단(게임시작)
    // 배경 색상(gradient)
    public string[] footerBackGradient { get; set; }
    // 게임시작 버튼 배경 색상(gradient)
    public string[] footerButtonGradient { get; set; }
    // 게임시작 버튼 외곽선 색상
    public string footerButtonOutlineColor { get; set; }
    // 게임시작 문구
    public string footerTitle { get; set; }
    // 게임시작 문구 색상
    public string footerTitleColor { get; set; }

    // 야간푸시 노출 여부
    public bool showNightPush { get; set; }

    // '모두 동의' 문구 변경 시
    public string allMessage { get; set; }

    // '이용 약관' 문구 변경 시
    public string termMessage { get; set; }

    // '개인정보 취급방침' 문구 변경 시
    public string privacyMessage { get; set; }

    // '야간 푸시' 문구 변경 시
    public string nightPushMessage { get; set; }

    public string ToJson()
    {
        JsonData data = new JsonData();

        data["theme"] = theme;
        data["headerBackGradient"] = string.Join(",", headerBackGradient);
        data["headerBottomColor"] = headerBottomColor;
        data["headerIconDrawable"] = headerIconDrawable;
        data["headerTitle"] = headerTitle;
        data["headerTitleColor"] = headerTitleColor;

        data["contentBackGradient"] = string.Join(",", contentBackGradient);
        data["contentIconDrawable"] = contentIconDrawable;
        data["contentIconColor"] = contentIconColor;
        data["contentCheckColor"] = contentCheckColor;
        data["contentTitleColor"] = contentTitleColor;
        data["contentShowColor"] = contentShowColor;

        data["footerBackGradient"] = string.Join(",", footerBackGradient);
        data["footerButtonGradient"] = string.Join(",", footerButtonGradient);
        data["footerButtonOutlineColor"] = footerButtonOutlineColor;
        data["footerTitle"] = footerTitle;
        data["footerTitleColor"] = footerTitleColor;

        data["showNightPush"] = showNightPush ? "true" : "false";

        data["allMessage"] = allMessage;
        data["termMessage"] = termMessage;
        data["privacyMessage"] = privacyMessage;
        data["nightPushMessage"] = nightPushMessage;

        Debug.Log("NAgreeInfo::ToJson() - " + data.ToJson());

        return data.ToJson();
    }
}