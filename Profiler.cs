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

		private static Dictionary<string, ProfilerItem> _previousState = new Dictionary<string, ProfilerItem>();
		private static Dictionary<string, ProfilerItem> _currentState = new Dictionary<string, ProfilerItem>();

		/// <summary>
		/// Used for determining parent id.
		/// </summary>
		private static Stack<string> _activeMeasurements = new Stack<string>();

		static Profiler()
		{
			_stopwatch.Start();
		}

		public static void Start(string itemId)
		{
			EnsureUpdatedState();

			if (!_currentState.ContainsKey(itemId))
			{
				string parentId = _activeMeasurements.Count > 0 ? _activeMeasurements.Peek() : "";

				//create a new item
				ProfilerItem newItem = new ProfilerItem(_stopwatch, parentId);
				_currentState.Add(itemId, newItem);
			}

			//start measurement on item
			_currentState[itemId].StartMeasurement();

			//add it to the active measurements tree
			_activeMeasurements.Push(itemId);
		}

		/// <summary>
		/// Stops the most recently initiated measurement.
		/// </summary>
		public static void Stop()
		{
			EnsureUpdatedState();

			if (_activeMeasurements.Count <= 0) 
				return;

			string itemId = _activeMeasurements.Pop();
			_currentState[itemId].StopMeasurement();
		}

		/// <summary>
		/// Get a specific item from the last completed state.
		/// </summary>
		public static ProfilerItem GetItem(string itemId)
		{
			return _previousState.ContainsKey(itemId) ? _previousState[itemId] : null;
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
			if (elapsedSeconds <= _lastSecond) 
				return;

			//do that update thing
			UpdateState();

			_lastSecond = elapsedSeconds;
		}

		private static void UpdateState()
		{
			//move currentstate to previousstate, and create a new state with new items
			_previousState = _currentState;
			_currentState = new Dictionary<string, ProfilerItem>();

			_activeMeasurements.Clear();
		}
	}
}