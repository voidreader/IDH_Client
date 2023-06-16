#ifndef GamePotError_h
#define GamePotError_h

// Common
#define CODE_UNKNOWN_ERROR      0 // 알 수 없는 Error
#define CODE_NOT_INITALIZE      1 // 초기화 실패
#define CODE_INVAILD_PARAM      2 // Vaild한 파라미터가 아닐 때
#define CODE_MEMBERID_IS_EMPTY  3 // 멤버아이디 데이터가 없을때

// Network
#define CODE_NETWORK_MODULE_NOT_INIT    3000  // 네트웍 모듈이 초기화 되지 않았을 때
#define CODE_NETWORK_ERROR              3001 // 네트웍 연결 오류 및 타임아웃 발생 시

// Server
#define CODE_SERVER_ERROR 4000 // 서버 Error

// Charge
#define CODE_CHARGE_UNKNOWN_ERROR 5000 // 결제에서 알 수 없는 오류 발생 및 스토어 측에서 Error를 전달 할 때
#define CODE_CHARGE_NO_ITEM 5001 // 없는 결제 아이템 일때

#endif /* NError_h */
