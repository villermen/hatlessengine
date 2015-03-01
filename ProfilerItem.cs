using System;
using System.Diagnostics;

namespace HatlessEngine
{
	/// <summary>
	/// Class for measuring and processing profiler data.
	/// </summary>
	public class ProfilerItem
	{
		private long _startTick;
		private readonly Stopwatch _stopwatch;
		internal readonly string ParentId;
		private long _totalDuration;

		public int TimesCompleted { get; private set; }

		public float PercentageOfParent
		{
			get
			{
				if (ParentId == "")
					return 100f;

				return (float)Math.Round(100f / Profiler.GetItem(ParentId)._totalDuration * _totalDuration, 2);
			}
		}

		internal ProfilerItem(Stopwatch stopwatch, string parentId = "")
		{
			_stopwatch = stopwatch;
			ParentId = parentId;
		}

		public float GetTotalDuration(bool inMs = false)
		{
			float result = _totalDuration;

			if (inMs)
			{
				result /= Stopwatch.Frequency / 1000f;
				result = (float)Math.Round(result, 2);
			}

			return result;
		}

		public float GetAverageDuration(bool inMs = false)
		{
			if (TimesCompleted == 0)
				return _totalDuration;

			float result = _totalDuration / TimesCompleted;

			if (inMs)
			{
				result /= Stopwatch.Frequency / 1000f;
				result = (float)Math.Round(result, 2);
			}

			return result;
		}

		/// <summary>
		/// Starts a measurement. Only works if none is currently running.
		/// </summary>
		internal void StartMeasurement()
		{
			if (_startTick != 0L)
				throw new ProfilerException("A measurement on this item is already in progress.");

			_startTick = _stopwatch.ElapsedTicks;
		}

		/// <summary>
		/// Completes a measurement, setting lastDuration.
		/// </summary>
		internal void StopMeasurement()
		{
			_totalDuration += _stopwatch.ElapsedTicks - _startTick;
			TimesCompleted++;
			_startTick = 0L;
		}
	}
}