using System;
using System.Runtime.InteropServices;


/// <summary>
/// 동작 소요시간 체크용 디버깅 클래스. 에디터에서만 동작한다.
/// 1. Bigin()이 콜된 시점과, End()가 콜된 시점의 시간 차를 Delay에 누적한다.
/// 2. Bigin()이 호출된 횟수를 카운팅하여 Counter에 저장한다.
///  Reset()으로 초기화한다.
/// </summary>
public class AcumulateTimer
{
#if UNITY_EDITOR
	[DllImport( "kernel32.dll" )]
	extern static short QueryPerformanceCounter( ref long x );
	[DllImport( "kernel32.dll" )]
	extern static short QueryPerformanceFrequency( ref long x );

	public double delay;
	private long Frequency;
	private long BeginTime;
	private long Endtime;
#endif
	private int counter;

	public AcumulateTimer( )
	{
#if UNITY_EDITOR
		//QueryPerformanceFrequency( ref Frequency );
#endif
		//Reset();
	}

	/// <summary>
	/// 타이머를 킨 횟수
	/// </summary>
	public int Counter
	{
		get { return counter; }
	}

	/// <summary>
	/// 총 지연 시간을 ms단위 문자열로 반환
	/// </summary>
	public string Delay
	{
		get
		{
#if UNITY_EDITOR
			//UnityEngine.Debug.Log( "F:" + Frequency + "  B:" + BeginTime + "  E:" + Endtime );
			return ((int)(delay * 1000)).ToString() + "ms";
#else
			return "NON";
#endif
		}
	}

	/// <summary>
	/// 총 지연시간에서 카운트당 평균으로 계산하며 ms단위 문자열로 반환
	/// </summary>
	public string Averge
	{
		get
		{
#if UNITY_EDITOR
			//UnityEngine.Debug.Log( "F:" + Frequency + "  B:" + BeginTime + "  E:" + Endtime );
			return ((int)((delay/counter) * 1000)).ToString() + "ms";
#else
			return "NON";
#endif
		}
	}


	/// <summary>
	/// 타이머 시작
	/// </summary>
	public void Begin(bool _reset = false )
	{
		++counter;
#if UNITY_EDITOR
		//QueryPerformanceCounter( ref BeginTime );
#endif
	}

	/// <summary>
	/// 타이머 종료
	/// </summary>
	public void End( )
	{
#if UNITY_EDITOR
		//QueryPerformanceCounter( ref Endtime );
		delay += ( Endtime - BeginTime ) * 1.0 / Frequency;
#endif
	}

	/// <summary>
	///  타이머 리셋
	/// </summary>
	public void Reset()
	{
#if UNITY_EDITOR
		delay = 0.0;
#endif
		counter = 0;
	}
}