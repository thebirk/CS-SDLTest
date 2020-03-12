using System;
using static SDL2.SDL;

public class Time
{

	public static float DeltaTime { get; set; }
	public static float T { get; set; }

	private static ulong _freq = 0;
	private static ulong Freq
	{
		get
		{
			if (_freq != 0) {
				return _freq;
			} else {
				_freq = SDL_GetPerformanceFrequency();
				return _freq;
			}
		}
	}

	public static ulong Now()
	{
		return SDL_GetPerformanceCounter();
	}

	public static double Seconds(ulong start, ulong end)
	{
		return (double)((end - start)) / Freq;
	}

	public static double Millis(ulong start, ulong end)
	{
		return (double)((end - start)*1000.0) / Freq;
	}

	public static double NanoSeconds(ulong start, ulong end)
	{
		return (double)((end - start)*1000000000.0) / Freq;
	}
}