using UnityEngine;
using System.Collections;
using Realtime.LITJson;

public class NError
{
    //Detail Error code
    public static readonly int CODE_UNKNOWN_ERROR           = 0;                    // 알 수 없는 Error
    public static readonly int CODE_NOT_INITALIZE           = 1;                    // 초기화 실패
    public static readonly int CODE_INVAILD_PARAM           = 2;                    // 파라미터가 올바르지 않은 경우
    public static readonly int CODE_MEMBERID_IS_EMPTY       = 3;                    // 멤버아이디 데이터가 없을때
    public static readonly int CODE_NOT_SIGNIN              = 4;                    // 로그인이 되지 않은 상태
    public static readonly int CODE_NETWORK_MODULE_NOT_INIT = 3000;                 // 네트웍 모듈이 초기화 되지 않았을 때
    public static readonly int CODE_NETWORK_ERROR           = 3001;                 // 네트웍 연결 오류 및 타임아웃 발생 시
    public static readonly int CODE_SERVER_ERROR            = 4000;                 // server-side에서 발생하는 오류
    public static readonly int CODE_SERVER_HTTP_ERROR       = 4001;                 // http response code가 성공이 아닌 경우
    public static readonly int CODE_SERVER_NETWORK_ERROR    = 4002;                 // 네트웍 연결 오류 및 타임아웃 발생 시
    public static readonly int CODE_SERVER_PARSING_ERROR    = 4003;                 // 서버에서 받은 데이터를 파싱할때 오류
    public static readonly int CODE_CHARGE_UNKNOWN_ERROR    = 5000;                 // 결제에서 알 수 없는 오류 발생 및 스토어 측에서 Error를 전달 할 때
    public static readonly int CODE_CHARGE_PRODUCTID_EMPTY  = 5001;                 // product id를 넣지 않은 경우
    public static readonly int CODE_CHARGE_PRODUCTID_WRONG  = 5002;                 // product id를 잘못 넣은 경우
    public static readonly int CODE_CHARGE_CONSUME_ERROR    = 5003;                 // consume시 오류


    public int code { get; set; }  							                        // error Code
    public string message { get; set; }                                             // error Message
}