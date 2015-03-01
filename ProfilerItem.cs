using System;
using System.Diagnostics;

namespace HatlessEngine
{
	/// <summary>
	/// Class for measuring and processing profiler data.
	/// </summary>
	internal class ProfilerItem
	{
		private long _startTick;
		private readonly Stopwatch _stopwatch;
		private readonly ProfilerItem _parent;
		public int TimesCompleted { get; private set; }
		private long _totalDuration;

		public ProfilerItem(Stopwatch stopwatch, ProfilerItem parent = null)
		{
			_stopwatch = stopwatch;
			_parent = parent;
		}

		/// <summary>
		/// Starts a measurement. Only works if none is currently running.
		/// </summary>
		public void StartMeasurement()
		{
			if (_startTick != 0L)
				throw new ProfilerException("A measurement on this item is already in progress.");

			_startTick = _stopwatch.ElapsedTicks;
		}

		/// <summary>
		/// Completes a measurement, setting lastDuration.
		/// </summary>
		public void StopMeasurement()
		{
			_totalDuration += _stopwatch.ElapsedTicks - _startTick;
			TimesCompleted++;
			_startTick = 0L;
		}

		public float GetPercentageOfParent()
		{
			if (_parent == null)
				return 100f;

			return 100f / _parent._totalDuration * _totalDuration;
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
			float result = _totalDuration / TimesCompleted;

			if (inMs)
			{
				result /= Stopwatch.Frequency / 1000f;
				result = (float)Math.Round(result, 2);
			}

			return result;
		}
	}
}