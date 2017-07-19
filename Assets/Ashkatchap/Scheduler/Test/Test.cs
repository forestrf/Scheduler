using Ashkatchap.Shared;
using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Ashkatchap.Updater {
	public class Test : MonoBehaviour {
		FrameUpdater.RecurrentReference[] firstUpdate;
		FrameUpdater.RecurrentReference[] secondUpdate;
		FrameUpdater.JobReference[] jobs;

		public int arraySize = 10000;
		public int multithreadIterations = 100;

		private void Awake() {
			UpdateMethod1Cached = UpdateMethod1;
			UpdateMethod2Cached = UpdateMethod2;
			MultithreadDoNothingCached = MultithreadDoNothing;
		}

		void OnEnable() {
			firstUpdate = new FrameUpdater.RecurrentReference[arraySize];
			secondUpdate = new FrameUpdater.RecurrentReference[arraySize];
			jobs = new FrameUpdater.JobReference[arraySize];
			for (int i = 0; i < firstUpdate.Length; i++) {
				firstUpdate[i] = Scheduler.AddRecurrentUpdateCallback(UpdateMethod1Cached, QueueOrder.Update, 127);
				secondUpdate[i] = Scheduler.AddRecurrentUpdateCallback(UpdateMethod2Cached, QueueOrder.Update, 128);
			}
		}
		void OnDisable() {
			for (int i = 0; i < firstUpdate.Length; i++) {
				Scheduler.RemoveRecurrentUpdateCallback(firstUpdate[i]);
				Scheduler.RemoveRecurrentUpdateCallback(secondUpdate[i]);
			}
		}

		int i = 0;
		Action UpdateMethod1Cached;
		void UpdateMethod1() {
			Profiler.BeginSample("1");
			jobs[i] = Scheduler.QueueMultithreadJob(MultithreadDoNothingCached, multithreadIterations);
			i = (i + 1) % firstUpdate.Length;
			Profiler.EndSample();
		}
		
		Action UpdateMethod2Cached;
		void UpdateMethod2() {
			Profiler.BeginSample("2");
			//var clock = StopwatchPool.StartClock();
			jobs[i].WaitForFinish();
			//TO DO: Stop watch end
			// And check min, max and mid
			i = (i + 1) % firstUpdate.Length;
			Profiler.EndSample();
		}

		FrameUpdater.Job MultithreadDoNothingCached;
		void MultithreadDoNothing(int index) {
			//Thread.Sleep(1);
		}
	}
}
