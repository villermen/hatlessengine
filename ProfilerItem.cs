using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HatlessEngine
{
	/// <summary>
	/// Class for measuring and processing profiler data.
	/// </summary>
	public class ProfilerItem
	{
		/// <summary>
		/// Starting tick of measurement, also used to check if measurement is running (if -1L).
		/// </summary>
		private long _startTick = -1L;
		private readonly Stopwatch _stopwatch;

		public readonly string Id;

		public readonly ProfilerItem Parent;
		public readonly List<ProfilerItem> Children = new List<ProfilerItem>();
		
		private long _totalDuration;

		public int TimesCompleted { get; private set; }
		
		public float PercentageOfParent
		{
			get
			{
				if (Parent == null)
					return 100f;

				return (float)Math.Round(100f / Parent._totalDuration * _totalDuration, 2);
			}
		}

		internal ProfilerItem(string id, Stopwatch stopwatch, ProfilerItem parent = null)
		{
			Id = id;
			_stopwatch = stopwatch;
			Parent = parent;
		}

		public float GetTotalDuration(bool inMs = false)
		{
			float result = _totalDuration;

			if (!inMs) 
				return result;

			result /= Stopwatch.Frequency / 1000f;
			result = (float)Math.Round(result, 2);
			return result;
		}

		public float GetAverageDuration(bool inMs = false)
		{
			if (TimesCompleted == 0)
				return _totalDuration;

			float result = _totalDuration / TimesCompleted;

			if (!inMs) 
				return result;

			result /= Stopwatch.Frequency / 1000f;
			result = (float)Math.Round(result, 2);
			return result;
		}

		/// <summary>
		/// Starts a measurement. Will stop a previous measurement if in progress.
		/// </summary>
		internal void Start()
		{
			Stop();

			_startTick = _stopwatch.ElapsedTicks;
		}

		/// <summary>
		/// Completes a measurement, setting lastDuration.
		/// </summary>
		internal void Stop()
		{
			if (_startTick == -1L) 
				return;

			_totalDuration += _stopwatch.ElapsedTicks - _startTick;
			TimesCompleted++;
			_startTick = -1L;
		}

		/// <summary>
		/// Returns the child if it exists, or null if not.
		/// </summary>
		public ProfilerItem GetChildById(string childId, bool recursively)
		{
			ProfilerItem directMatch = Children.Find(child => child.Id == childId);

			if (directMatch != null)
				return directMatch;
			
			if (!recursively) 
				return null;

			//search recursively if still not found
			foreach (ProfilerItem child in Children)
			{
				ProfilerItem indirectMatch = child.GetChildById(childId, true);
				if (indirectMatch != null)
					return indirectMatch;
			}

			return null;
		}

		public override string ToString()
		{
			return String.Format("{0}: {1}/s {2}ms/s ~{3}ms/r {4}%", Id, TimesCompleted, GetTotalDuration(true), GetAverageDuration(true), PercentageOfParent);
		}
	}
}