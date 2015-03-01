using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HatlessEngine
{
	public static class Profiler
	{
		private static Stopwatch _stopwatch = new Stopwatch();
		private static long _lastSecond;

		private static Dictionary<string, ProfilerItem> _previousState;
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
				ProfilerItem parent = _currentState.ContainsKey(parentId) ? _currentState[parentId] : null;
				ProfilerItem newItem = new ProfilerItem(_stopwatch, parent);
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

		private static void EnsureUpdatedState()
		{
			long elapsedSeconds = _stopwatch.ElapsedTicks / Stopwatch.Frequency;
			if (elapsedSeconds > _lastSecond)
			{
				//do that update thing
				_previousState = _currentState;
				_currentState = new Dictionary<string, ProfilerItem>();

				_lastSecond = elapsedSeconds;
			}
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

				result += String.Format("{0}: {1}/s {2}ms/s {3}ms/m {4}%\n", id, item.TimesCompleted, item.GetTotalDuration(true), item.GetAverageDuration(true), item.GetPercentageOfParent());
			}

			return result;
		}
	}
}