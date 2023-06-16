using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTimer
{
	private float m_fStartTime;
	private float m_fCurTime;

	public CTimer()
	{
		ResetTime();
	}

	public float GetTime()
	{
		float fTime = (Time.time - m_fStartTime) + m_fCurTime;
		return fTime;
	}

	public void ResetTime(float fTime = 0f)
	{
		m_fStartTime = Time.time;
		m_fCurTime = fTime;
	}
}

public class CRealTimer
{
	private float m_fStartTime;
	private float m_fCurTime;

	public CRealTimer()
	{
		ResetTime();
	}

	public float GetTime()
	{
		float fTime = (Time.realtimeSinceStartup - m_fStartTime) + m_fCurTime;
		return fTime;
	}

	public void ResetTime(float fTime = 0f)
	{
		m_fStartTime = Time.realtimeSinceStartup;
		m_fCurTime = fTime;
	}
}