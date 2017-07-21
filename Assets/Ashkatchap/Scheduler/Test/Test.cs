using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace Ashkatchap.Updater {
	public class Test : MonoBehaviour {
		FrameUpdater.RecurrentReference[] nothingUpdate;
		FrameUpdater.RecurrentReference[] firstUpdate;
		FrameUpdater.RecurrentReference[] secondUpdate;
		FrameUpdater.JobReference[] jobs;

		public int arraySize = 10000;
		public ushort multithreadIterations = 100;
		public bool singleThread = false;

		Action DoNothingCached;
		Action UpdateMethod1Cached;
		Action UpdateMethod2Cached;
		FrameUpdater.Job MultithreadDoNothingCached;
		private void Awake() {
			DoNothingCached = DoNothing;
			UpdateMethod1Cached = UpdateMethod1;
			UpdateMethod2Cached = UpdateMethod2;
			MultithreadDoNothingCached = MultithreadDoNothing;
		}

		void OnEnable() {
			nothingUpdate = new FrameUpdater.RecurrentReference[arraySize];
			firstUpdate = new FrameUpdater.RecurrentReference[arraySize];
			secondUpdate = new FrameUpdater.RecurrentReference[arraySize];
			jobs = new FrameUpdater.JobReference[arraySize];
			for (int i = 0; i < firstUpdate.Length; i++) {
				nothingUpdate[i] = Scheduler.AddRecurrentUpdateCallback(DoNothingCached, QueueOrder.Update, 126);
				firstUpdate[i] = Scheduler.AddRecurrentUpdateCallback(UpdateMethod1Cached, QueueOrder.Update, 127);
				secondUpdate[i] = Scheduler.AddRecurrentUpdateCallback(UpdateMethod2Cached, QueueOrder.Update, 128);
			}
		}
		void OnDisable() {
			for (int i = 0; i < firstUpdate.Length; i++) {
				Scheduler.RemoveRecurrentUpdateCallback(nothingUpdate[i]);
				Scheduler.RemoveRecurrentUpdateCallback(firstUpdate[i]);
				Scheduler.RemoveRecurrentUpdateCallback(secondUpdate[i]);
			}
		}

		int i = 0;
		void UpdateMethod1() {
			FrameUpdater.FORCE_SINGLE_THREAD = singleThread;
			Profiler.BeginSample("Add Multithread");
			jobs[i] = Scheduler.QueueMultithreadJob(MultithreadDoNothingCached, multithreadIterations);
			i = (i + 1) % firstUpdate.Length;
			Profiler.EndSample();
		}
		
		void UpdateMethod2() {
			Profiler.BeginSample("Wait Multithread");

			jobs[i].WaitForFinish();

			i = (i + 1) % firstUpdate.Length;
			Profiler.EndSample();
		}

		void DoNothing() {
			Profiler.BeginSample("Nothing");
			Profiler.EndSample();
		}

		public int workPerIteration = 10000;

		void MultithreadDoNothing(int index) {
			
			int ignore = 1;
			for (int i = 0; i < workPerIteration; i++) ignore += i % ignore;
			
			//Thread.Sleep(1);
		}
	}
}
