using UnityEngine;
using System.Collections;
using System.Text;

public class ShowFPS : MonoBehaviour
{

	public float f_UpdateInterval = 0.5F;

	private float f_LastInterval;

	private int i_Frames = 0;

	private float f_Fps;

	private StringBuilder sb = new StringBuilder(256);

	void Start()
	{
		//Application.targetFrameRate=60;

		f_LastInterval = Time.realtimeSinceStartup;

		i_Frames = 0;
	}

	void OnGUI()
	{
		sb.Remove(0, sb.Length);
		sb.Append("MemoryAllocated:");				sb.Append(GetMemoryMB(UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()));				sb.Append("MB");
		sb.Append("\nMemoryReserved:");				sb.Append(GetMemoryMB(UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong()));				sb.Append("MB");
		sb.Append("\nMemoryUnusedReserved:"); sb.Append(GetMemoryMB(UnityEngine.Profiling.Profiler.GetTotalUnusedReservedMemoryLong()));	sb.Append("MB");
		sb.Append("\nusedHeapSize:");					sb.Append(GetMemoryMB(UnityEngine.Profiling.Profiler.usedHeapSizeLong));										sb.Append("MB");
		sb.Append("\nMonoHeapSize:");					sb.Append(GetMemoryMB(UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong()));								sb.Append("MB");
		sb.Append("\nMonoUsedSize:");					sb.Append(GetMemoryMB(UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong()));								sb.Append("MB");
#if UNITY_EDITOR
		sb.Append("\nBatches:");							sb.Append(UnityEditor.UnityStats.batches);
		sb.Append("  DrawCall:");							sb.Append(UnityEditor.UnityStats.drawCalls);
#endif

		GUI.color = new Color(0, 1, 0);
		GUI.Label(new Rect(Screen.width - 220, 0, 330, 300), sb.ToString());
		//version_str = System.String.Format("Ver. {0}", version_str);
		//GUI.Label(new Rect(0, 100, 200, 200), version_str);
		if (f_Fps > 50)
		{
			GUI.color = new Color(0, 1, 0);
		}
		else if (f_Fps > 25)
		{
			GUI.color = new Color(1, 1, 0);
		}
		else
		{
			GUI.color = new Color(1.0f, 0, 0);
		}

		GUI.Label(new Rect(0, 50, 300, 300), "FPS:" + f_Fps.ToString("f2"));

	}

	private string GetMemoryMB(long curSize)
	{
		float mbSize = curSize / (1024f * 1024f);
		return mbSize.ToString("f2");
	}

	void Update()
	{
		++i_Frames;

		if (Time.realtimeSinceStartup > f_LastInterval + f_UpdateInterval)
		{
			f_Fps = i_Frames / (Time.realtimeSinceStartup - f_LastInterval);

			i_Frames = 0;

			f_LastInterval = Time.realtimeSinceStartup;
		}
	}
}
