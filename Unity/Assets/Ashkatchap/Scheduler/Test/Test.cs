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
		public int NUM_THREADS = 8;

		public int workPerIteration = 10000;

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

		private bool started = false;
		void Start() {
			nothingUpdate = new FrameUpdater.RecurrentReference[arraySize];
			firstUpdate = new FrameUpdater.RecurrentReference[arraySize];
			secondUpdate = new FrameUpdater.RecurrentReference[arraySize];
			jobs = new FrameUpdater.JobReference[arraySize];
			for (int i = 0; i < firstUpdate.Length; i++) {
				nothingUpdate[i] = Scheduler.AddRecurrentUpdateCallback(DoNothingCached, QueueOrder.Update, 126);
				firstUpdate[i] = Scheduler.AddRecurrentUpdateCallback(UpdateMethod1Cached, QueueOrder.Update, 127);
				secondUpdate[i] = Scheduler.AddRecurrentUpdateCallback(UpdateMethod2Cached, QueueOrder.Update, 128);
			}
			started = true;
		}
		void End() {
			for (int i = 0; i < firstUpdate.Length; i++) {
				Scheduler.RemoveRecurrentUpdateCallback(nothingUpdate[i]);
				Scheduler.RemoveRecurrentUpdateCallback(firstUpdate[i]);
				Scheduler.RemoveRecurrentUpdateCallback(secondUpdate[i]);
			}
			started = false;
		}

		private void OnGUI() {
			GUILayout.Label("array Size");
			arraySize = int.Parse(GUILayout.TextField(arraySize.ToString()));
			if (!started && GUILayout.Button("START")) {
				Start();
			}
			if (started && GUILayout.Button("END")) {
				End();
			}
			GUILayout.Label("multithread Iterations");
			multithreadIterations = (ushort) int.Parse(GUILayout.TextField(multithreadIterations.ToString()));
			
			singleThread = GUILayout.Toggle(singleThread, "single Thread");

			GUILayout.Label("NUM_THREADS");
			NUM_THREADS = int.Parse(GUILayout.TextField(NUM_THREADS.ToString()));

			GUILayout.Label("work Per Iteration");
			workPerIteration = int.Parse(GUILayout.TextField(workPerIteration.ToString()));
		}

		int i = 0;
		void UpdateMethod1() {
			FrameUpdater.FORCE_SINGLE_THREAD = singleThread;
			FrameUpdater.NUM_THREADS = NUM_THREADS;
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

		void MultithreadDoNothing(int index) {
			
			int ignore = 1;
			for (int i = 0; i < workPerIteration; i++) ignore += i % ignore;
			
			//Thread.Sleep(1);
		}
	}
}
