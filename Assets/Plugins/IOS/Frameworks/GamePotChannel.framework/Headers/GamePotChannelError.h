
#ifndef GamePotChannelError_h
#define GamePotChannelError_h

#define CODE_AUTH_CHANNEL_IS_NOT_LOGIN      2000 // 해당 채널에 대해 로그인이 되지 않았을 때
#define CODE_AUTH_CHANNEL_IS_NOT_SETTING    2001 // 해당 채널에 대해 설정이 정상적이지 않을 때
#define CODE_AUTH_CHANNEL_LOGOUT_FAIL       2002 // 해당 채널에 대해 로그 아웃 실패 시
#define CODE_AUTH_CHANNEL_UNKNOWN_ERROR     2003 // 해당 채널 로그인 중 에러 발생 시
#define CODE_AUTH_LINKING_ONLY_GUEST        2004 // 링킹 시도 시 게스트가 아닐 때

#define CODE_AUTH_UNLINKING_ONLY_SOCIAL     2005 // 링킹 해제 시 게스트 일때
#define CODE_AUTH_UNLINKING_INVALID_CHANNELTYPE  2006 // 링킹 해제 시 채널 타입을 잘 못 넣었을 때
#define CODE_AUTH_ACHEIVEMENT_ERROR         2010 // GameCenter 업적 달성한 리스트 불러올 때 애러 발생 시
#endif
