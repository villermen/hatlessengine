using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MoreLinq;

namespace HatlessEngine
{
	public static class Profiler
	{
		private static Stopwatch _stopwatch = new Stopwatch();
		private static long _lastSecond;

		private static Dictionary<string, ProfilerItem> _previousState = new Dictionary<string, ProfilerItem>();
		private static Dictionary<string, ProfilerItem> _currentState = new Dictionary<string, ProfilerItem>();

		static Profiler()
		{
			_stopwatch.Start();
		}

		public static void StartMeasurement(string itemId, string parentId = "")
		{
			EnsureUpdatedState();

			if (!_currentState.ContainsKey(itemId))
			{
				//create a new item
				ProfilerItem newItem = new ProfilerItem(_stopwatch, parentId);
				_currentState.Add(itemId, newItem);
			}

			//start measurement on item
			_currentState[itemId].StartMeasurement();
		}

		public static void StopMeasurement(string itemId)
		{
			EnsureUpdatedState();

			//do not do anything if key is not present (it has been removed by changing states)
			if (_currentState.ContainsKey(itemId))
				_currentState[itemId].StopMeasurement();
		}

		/// <summary>
		/// Get a specific item from the last completed state.
		/// </summary>
		public static ProfilerItem GetItem(string itemId)
		{
			if (_previousState != null && _previousState.ContainsKey(itemId))
				return _previousState[itemId];

			return null;
		}

		/// <summary>
		/// Returns a (sort of) formatted string that shows info about the last completed state.
		/// </summary>
		public static string GetStateString()
		{
			if (_previousState == null)
				return "";

			var orderedState = _previousState.OrderByDescending(pair => pair.Value.GetTotalDuration());

			string result = "";

			foreach (var idAndItem in orderedState)
			{
				string id = idAndItem.Key;
				ProfilerItem item = idAndItem.Value;

				result += String.Format("{0}: {1}/s {2}ms/s ~{3}ms/r {4}%\n", id, item.TimesCompleted, item.GetTotalDuration(true), item.GetAverageDuration(true), item.PercentageOfParent);
			}

			return result;
		}

		private static void EnsureUpdatedState()
		{
			long elapsedSeconds = _stopwatch.ElapsedTicks / Stopwatch.Frequency;
			if (elapsedSeconds > _lastSecond)
			{
				//do that update thing
				UpdateState();

				_lastSecond = elapsedSeconds;
			}
		}

		private static void UpdateState()
		{
			//move currentstate to previousstate, and create a new state with new items
			_previousState = _currentState;
			_currentState = new Dictionary<string, ProfilerItem>();
			_previousState.ForEach(previousPair => _currentState.Add(previousPair.Key, new ProfilerItem(_stopwatch, previousPair.Value.ParentId)));
		}
	}
}