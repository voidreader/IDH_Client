-------------------------------------------------- ------------------------------
                          Unity를위한 SocketIO - v1.0.1
-------------------------------------------------- ------------------------------

# 개요 #
  
  이 플러그인을 사용하면 Unity 게임을 Socket.IO 백엔드와 통합 할 수 있습니다
  socket.io-protocol github repo에 설명 된 프로토콜을 구현합니다.
  (https://github.com/automattic/socket.io-protocol)
  
  연결되어있는 동안 Socket.IO는 메인 스레드를 막지 않도록 자체 스레드에서 실행됩니다.
  실. 이벤트는 큐에 저장되고 수신 된 다음 프레임에서 전달됩니다.


# 지원하다 #
  
  fpanettieri@gmail.com
  skype : fabio.panettieri


# 빠른 시작 #

  프로젝트에서 Socket.IO를 사용하려면 다음을 수행해야합니다.
  1. SocketIO / Prefab /에서 SocketIO 프리 패브를 씬으로 드래그하십시오.
  2. Socket.IO 서버가 수신하는 url을 구성하십시오.
  3. 자동 연결 플래그가 항상 실행되도록하려면 토글하십시오.
  4. 그게 다야! 이제 게임에서 Socket.IO를 사용할 수 있습니다.


# 사용하는 방법 #
 
  1. Socket.IO 구성 요소 참조 얻기
  
    GameObject go = GameObject.Find ( "SocketIO");
    소켓 = go.GetComponent <SocketIOComponent> ();

GameObject.Find를 사용하면 꽤 비쌉니다.
나중에 사용할 수 있도록 변수에 해당 참조를 저장하려고 할 수 있습니다.


  2. 이벤트 수신
    
    소켓 참조를 사용하여 사용자 정의 이벤트를 수신 할 수 있습니다.
    
    public void Start () 
    {
      socket.On ( "boop", TestBoop);
    }
    
    public void TestBoop (SocketIOEvent 전자) 
    {
      Debug.Log (string.Format ( "[name : {0}, data : {1}]", e.name, e.data));
    }

	또한 람다 expresions을 콜백으로 사용할 수도 있습니다

    socket.On ( "boop", (SocketIOEvent e) => 
      { Debug.Log (string.Format ( "[name : {0}, data : {1}]", e.name, e.data)); });
  
  
  3. 이벤트 보내기
  
    Socket.IO 이벤트 나 사용자 정의 이벤트를 듣는 것 외에
    Emit 메소드를 사용하여 Socket.IO 서버에 정보를 보낸다.
    
    a) 일반 메시지 보내기
       socket.Emit ( "user : login");
       
    b) 추가 데이터 전송
    
       Dictionary<string, string> data = new Dictionary<string, string>();
       data["email"] = "some@email.com";
       data["pass"] = Encrypt("1234");
       socket.Emit("user:login", new JSONObject(data));
       
    c) 때로는 클라이언트가 확인할 때 콜백을 원할 수 있습니다.
       메시지 수신. 이렇게하려면 단순히 함수를 마지막으로 전달하십시오.
       .Emit ()의 매개 변수
       
       socket.Emit ( "user : login", OnLogin);
       socket.Emit ( "user : login", new JSONObject (data), OnLogin);


  4. 현재 소켓 ID 얻기 (socket.sid)
  
    public void Start(){
    	socket.On("open", OnSocketOpen);
    }
    
    public void OnSocketOpen(SocketIOEvent ev){
    	Debug.Log("updated socket id " + socket.sid);
    }



  5. 네임 스페이스 지원
    아직 구현되지 않았습니다!
  
  
  6. 바이너리 이벤트
    아직 구현되지 않았습니다!


# 예 #
  
  이 패키지에는 사용하고자하는 미니멀리스트 테스트도 포함되어 있습니다.
  환경을 올바르게 설정했는지 확인하십시오.
  
  1. 서버 디렉토리로 이동하십시오.
     CD PATH / TO / PROJECT / Assets / SocketIO / Server

  2. 유니티 폴더 밖에서 서버 코드의 압축을 풉니 다.
       beep.js.zip -d / tmp / socketio의 압축을 풉니 다.

  3. 서버 코드가 추출 된 대상 폴더로 이동합니다.
       cd / tmp / socketio
     
  4. Socket.IO 서버 패키지 설치
       npm은 socket.io를 설치합니다.
  
  5. (선택 사항) 디버그 모드 사용
       Windows : DEBUG = 설정
       Mac : 내보내기 DEBUG = *
  
  6. 테스트 서버 실행
       노드 ./beep.js
  
  7. 테스트 장면 열기
       SocketIO / Scene / SocketIOTest
  
  8. 장면을 실행하십시오. 일부 디버그 메시지가 Unity 콘솔에 인쇄됩니다.
  
  9. SocketIO / Scripts / Test / TestSocketIO.cs를 열고 무슨 일이 일어나고 있는지 확인하십시오.


# 문제 해결 #

  이것은 플러그인의 첫 번째 릴리스이므로 오류가 나타날 수 있습니다.
  그들을 추적하기 위해 몇 가지 디버그 코드를 포함 시켰습니다.
  일부 플래그를 주석 해제 할 때만 컴파일됩니다.
  
  디버그 메시지를 활성화하려면 다음을 수행하십시오.
  1. SocketIO / Scripts / SocketIO로 이동합니다.
  2. 파일 Decoder.cs, Encoder.cs 및 SocketIOComponent.cs를 엽니 다.
  3. 다음 행의 주석 처리를 제거하십시오.
       #define SOCKET_IO_DEBUG
  4. 게임을 다시 실행하십시오. 이번에 보내고받은 메시지는
     Unity 콘솔에 기록됩니다. 바라건대 당신은
     당신의 문제 / 버그의 근원.
  5. 사용한 후에 다시 언급해야합니다.
  

# 라이센스 #

  Unity를위한 SocketIO는 The MIT License에서 제공됩니다.

# 변경 로그 #

  1.0.1 - 개선 된 소켓 안정성 (멀티 스레딩, ack 가비지 콜렉션, ping)
  1.0.0 - 초기 출시