using Ashkatchap.Updater;
using System;
using System.Diagnostics;

public class Program {
	static Updater updater;
	public static void Main(string[] args) {
		updater = new Updater();
		Scheduler.MultithreadingStart(updater);
		
		Program p = new Program();
		p.Awake();
		p.Start();
		for (int i = 0; true; i++) {
			var w = Stopwatch.StartNew();
			for (int j = 0; j < 10; j++) {
				updater.Execute();
			}
			Console.WriteLine((w.Elapsed.TotalMilliseconds / 10f) + "ms");
		}
	}

	UpdateReference[] nothingUpdate;
	UpdateReference[] firstUpdate;
	UpdateReference[] secondUpdate;
	JobReference[] jobs;

	public int arraySize = 100;
	public ushort multithreadIterations = 1000;
	public bool singleThread = false;
	public int NUM_THREADS = 8;
	public ushort minimumRangeToSteal = 0;

	public int workPerIteration = 0;

	Action DoNothingCached;
	Action UpdateMethod1Cached;
	Action UpdateMethod2Cached;
	Job MultithreadDoNothingCached;
	private void Awake() {
		DoNothingCached = DoNothing;
		UpdateMethod1Cached = UpdateMethod1;
		UpdateMethod2Cached = UpdateMethod2;
		MultithreadDoNothingCached = MultithreadDoNothing;
	}
	
	void Start() {
		nothingUpdate = new UpdateReference[arraySize];
		firstUpdate = new UpdateReference[arraySize];
		secondUpdate = new UpdateReference[arraySize];
		jobs = new JobReference[arraySize];
		for (int i = 0; i < firstUpdate.Length; i++) {
			nothingUpdate[i] = updater.AddUpdateCallback(DoNothingCached, 126);
			firstUpdate[i] = updater.AddUpdateCallback(UpdateMethod1Cached, 127);
			secondUpdate[i] = updater.AddUpdateCallback(UpdateMethod2Cached, 128);
		}
	}
	void End() {
		for (int i = 0; i < firstUpdate.Length; i++) {
			updater.RemoveUpdateCallback(nothingUpdate[i]);
			updater.RemoveUpdateCallback(firstUpdate[i]);
			updater.RemoveUpdateCallback(secondUpdate[i]);
		}
	}

	int i = 0;
	void UpdateMethod1() {
		Scheduler.FORCE_SINGLE_THREAD = singleThread;
		Scheduler.DESIRED_NUM_CORES = NUM_THREADS;
		jobs[i] = Scheduler.QueueMultithreadJob(MultithreadDoNothingCached, multithreadIterations, 127, minimumRangeToSteal);
		i = (i + 1) % firstUpdate.Length;
	}

	void UpdateMethod2() {
		jobs[i].WaitForFinish();

		i = (i + 1) % firstUpdate.Length;
	}

	void DoNothing() {
	}

	void MultithreadDoNothing(int index) {

		int ignore = 1;
		for (int i = 0; i < workPerIteration; i++) ignore += i % ignore;

		//Thread.Sleep(1);
	}
}
