using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HatlessEngine
{
	/// <summary>
	/// Profiling works on a current state (changes every second), while retrieving data works on the most recently finished state.
	/// </summary>
	public static class Profiler
	{
		private static Stopwatch _stopwatch = new Stopwatch();
		private static long _lastSecond;

		private static List<ProfilerItem> _previousState = new List<ProfilerItem>();
		private static List<ProfilerItem> _currentState = new List<ProfilerItem>();

		private static ProfilerItem _currentlyMeasuringItem;

		static Profiler()
		{
			_stopwatch.Start();
		}

		public static void Start(string itemId)
		{
			EnsureUpdatedState();

			//obtain a list of items to check in/add to
			List<ProfilerItem> lookupList = _currentlyMeasuringItem == null ? _currentState : _currentlyMeasuringItem.Children;

			//get an existing item
			ProfilerItem item = lookupList.Find(lookupItem => lookupItem.Id == itemId);

			//the collection does not yet contain this item -> create it
			if (item == null)
			{
				//create a new item
				item = new ProfilerItem(itemId, _stopwatch, _currentlyMeasuringItem);

				//add reference to newly created child to parent (or state)
				if (_currentlyMeasuringItem == null)
					_currentState.Add(item);
				else
					_currentlyMeasuringItem.Children.Add(item);
			}

			//start measurement on item
			item.Start();

			//set is at last started measurement
			_currentlyMeasuringItem = item;
		}

		/// <summary>
		/// Stops the most recently initiated measurement.
		/// </summary>
		public static void Stop()
		{
			EnsureUpdatedState();
			
			//stop the currently measuring item and ascend it (or set to null if top)
			if (_currentlyMeasuringItem != null)
			{
				_currentlyMeasuringItem.Stop();
				_currentlyMeasuringItem = _currentlyMeasuringItem.Parent;
			}
		}

		/// <summary>
		/// Will try to get an item from the most recently completed state (searches the state recursively).
		/// </summary>
		public static ProfilerItem GetItem(string itemId)
		{
			ProfilerItem directMatch = _previousState.Find(item => item.Id == itemId);

			if (directMatch != null)
				return directMatch;

			//search recursively
			foreach (ProfilerItem item in _previousState)
			{
				ProfilerItem indirectMatch = item.GetChildById(itemId, true);
				if (indirectMatch != null)
					return indirectMatch;
			}

			return null;
		}

		/// <summary>
		/// Returns a (sort of) formatted string that shows info about the last completed state.
		/// </summary>
		public static string GetStateString()
		{
			//order by total duration (base profileritems)
			_previousState = _previousState.OrderByDescending(item => item.GetTotalDuration()).ToList();

			//start off with the previousstate profilers and the children will be rendered accordingly
			return GetItemsString(_previousState, 0);
		}

		/// <summary>
		/// Draw a part of the state string
		/// </summary>
		private static string GetItemsString(List<ProfilerItem> items, int depth)
		{
			string result = "";

			items.ForEach(item =>
			{
				//indent by depth
				for (int i = 0; i < depth; i++)
					result += "\t";

				result += item + "\n";

				//draw children below this one with a depth level down
				result += GetItemsString(item.Children, depth + 1);
			});

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
			_currentState = new List<ProfilerItem>();
		}
	}
}