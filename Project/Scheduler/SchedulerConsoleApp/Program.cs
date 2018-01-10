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
				updater.Execute();
			}
			Console.WriteLine((w.Elapsed.TotalMilliseconds / 10f) + "ms");
		}
	}

	UpdateReference[] nothingUpdate;
	UpdateReference AU, DU;
	UpdateReference[] BU, CU;
	JobReference[] jobs;

	public int arraySize = 100;
	public ushort multithreadIterations = 1000;
	public bool singleThread = false;
	public int NUM_THREADS = 8;
	public ushort minimumRangeToSteal = 0;

	public int workPerIteration = 0;

	Action DoNothingCached;
	Action cachedA, cachedB, cachedC, cachedD;
	Job MultithreadDoNothingCached;
	private void Awake() {
		DoNothingCached = DoNothing;
		cachedA = A;
		cachedB = B;
		cachedC = C;
		cachedD = D;
		MultithreadDoNothingCached = MultithreadDoNothing;
	}

	public static int[] test;

	public int AllTrueInTest() {
		int fails = 0;
		for (int i = 0; i < test.Length; i++) {
			if (test[i] != arraySize) {
				Console.WriteLine(i);
				fails++;
			}
		}
		return fails;
	}

	void Start() {
		nothingUpdate = new UpdateReference[arraySize];
		BU = new UpdateReference[arraySize];
		CU = new UpdateReference[arraySize];
		jobs = new JobReference[arraySize];
		test = new int[multithreadIterations];
		AU = updater.AddUpdateCallback(cachedA, 126);
		for (int i = 0; i < BU.Length; i++) {
			nothingUpdate[i] = updater.AddUpdateCallback(DoNothingCached, 126);
			BU[i] = updater.AddUpdateCallback(cachedB, 127);
			CU[i] = updater.AddUpdateCallback(cachedC, 128);
		}
		DU = updater.AddUpdateCallback(cachedD, 129);
	}
	void End() {
		updater.RemoveUpdateCallback(AU);
		for (int i = 0; i < BU.Length; i++) {
			updater.RemoveUpdateCallback(nothingUpdate[i]);
			updater.RemoveUpdateCallback(BU[i]);
			updater.RemoveUpdateCallback(CU[i]);
		}
		updater.RemoveUpdateCallback(DU);
	}

	void A() {
		Scheduler.FORCE_SINGLE_THREAD = singleThread;
		Scheduler.DESIRED_NUM_CORES = NUM_THREADS;
		for (int i = 0; i < test.Length; i++) System.Threading.Interlocked.Exchange(ref test[i], 0);
	}

	int i = 0;
	void B() {
		jobs[i] = Scheduler.QueueMultithreadJob(MultithreadDoNothingCached, multithreadIterations, Scheduler.DEFAULT_PRIORITY, null, minimumRangeToSteal);
		i = (i + 1) % BU.Length;
	}
	void C() {
		jobs[i].WaitForFinish();
		i = (i + 1) % BU.Length;
	}

	void D() {
		if (AllTrueInTest() > 0) {
			Console.WriteLine("WTF");
		}
	}

	void DoNothing() {
	}

	void MultithreadDoNothing(int index) {
		int ignore = 1;
		for (int i = 0; i < workPerIteration; i++) ignore += i % ignore;

		System.Threading.Interlocked.Increment(ref test[index]);
	}
}
