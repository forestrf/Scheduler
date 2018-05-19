using Ashkatchap.Scheduler;
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
				updater.Execute(null);
			}
			Console.WriteLine((w.Elapsed.TotalMilliseconds / 10f) + "ms");
		}
	}

	UpdateReference[] nothingUpdate;
	UpdateReference AU;
	UpdateReference[] BU, CU;
	JobReference[] jobs;

	public int arraySize = 100;
	public ushort multithreadIterations = 100;
	public bool singleThread = false;
	public int NUM_THREADS = 8;
	
	
	Job MultithreadDoNothingCached;
	private void Awake() {
		MultithreadDoNothingCached = MultithreadDoNothing;
	}

	void Start() {
		nothingUpdate = new UpdateReference[arraySize];
		BU = new UpdateReference[arraySize];
		CU = new UpdateReference[arraySize];
		jobs = new JobReference[arraySize];
		AU = updater.AddUpdateCallback(A, 126);
		for (int i = 0; i < arraySize; i++) {
			nothingUpdate[i] = updater.AddUpdateCallback(DoNothing, 126);
			BU[i] = updater.AddUpdateCallback(B, 127);
			CU[i] = updater.AddUpdateCallback(C, 128);
		}
	}
	void End() {
		updater.RemoveUpdateCallback(AU);
		for (int i = 0; i < BU.Length; i++) {
			updater.RemoveUpdateCallback(nothingUpdate[i]);
			updater.RemoveUpdateCallback(BU[i]);
			updater.RemoveUpdateCallback(CU[i]);
		}
	}

	void A() {
		Scheduler.FORCE_SINGLE_THREAD = singleThread;
		Scheduler.DESIRED_NUM_CORES = NUM_THREADS;
	}

	int i = 0;
	void B() {
		Scheduler.QueueMultithreadJob(MultithreadDoNothingCached, multithreadIterations, out jobs[i], Scheduler.DEFAULT_PRIORITY, null);
		i = (i + 1) % BU.Length;
	}
	void C() {
		jobs[i].WaitForFinish();
		i = (i + 1) % BU.Length;
	}

	void DoNothing() { }
	void MultithreadDoNothing(int index) { }
}
