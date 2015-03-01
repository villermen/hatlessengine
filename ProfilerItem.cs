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
		private readonly ProfilerItem _parent;
		private long _totalDuration;

		public int TimesCompleted { get; private set; }

		public float PercentageOfParent
		{
			get
			{
				if (_parent == null)
					return 100f;

				return 100f / _parent._totalDuration * _totalDuration;
			}
		}

		internal ProfilerItem(Stopwatch stopwatch, ProfilerItem parent = null)
		{
			_stopwatch = stopwatch;
			_parent = parent;
		}

		/// <summary>
		/// Copy contructor.
		/// </summary>
		internal ProfilerItem(ProfilerItem item)
			: this(item._stopwatch, item._parent) { }

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