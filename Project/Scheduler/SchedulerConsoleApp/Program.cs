using Ashkatchap.Scheduler;
using System;
using System.Diagnostics;

public class Program {
	static Updater updater;
	public static void Main(string[] args) {
		updater = new Updater();
		ThreadedJobs.MultithreadingStart();
		
		updater.QueueCallback(() => { Console.WriteLine("Hi 1"); }, 1);
		updater.QueueCallback(() => { Console.WriteLine("Hi 2"); }, 2);
		updater.QueueCallback(() => { Console.WriteLine("Hi 3"); }, 3);
		updater.QueueCallback(() => { Console.WriteLine("Hi 4"); }, 4);

		//Program.Start();
		for (int i = 0; true; i++) {
			var w = Stopwatch.StartNew();
			for (int j = 0; j < 10; j++) {
				updater.Execute(null);
			}
			//Console.WriteLine((w.Elapsed.TotalMilliseconds / 10f) + "ms");
		}
	}

	static UpdateReference[] nothingUpdate;
	static UpdateReference AU, DU;
	static UpdateReference[] BU, CU;
	static QueuedJob[] jobs;

	static public int arraySize = 1000;
	static public bool singleThread = false;
	static public int NUM_THREADS = 8;

	static public int loopsDoSomething = 10000;
	


	static void Start() {
		nothingUpdate = new UpdateReference[arraySize];
		BU = new UpdateReference[arraySize];
		CU = new UpdateReference[arraySize];
		jobs = new QueuedJob[arraySize];
		AU = updater.AddUpdateCallback(A, 126);
		DU = updater.AddUpdateCallback(D, 129);
		for (int i = 0; i < arraySize; i++) {
			nothingUpdate[i] = updater.AddUpdateCallback(DoNothing, 126);
			BU[i] = updater.AddUpdateCallback(B, 127);
			CU[i] = updater.AddUpdateCallback(C, 128);
			if (i % 100 == 0)
				Console.WriteLine(i);
		}
		Console.WriteLine("Start done");
	}
	static void End() {
		updater.RemoveUpdateCallback(AU);
		for (int i = 0; i < BU.Length; i++) {
			updater.RemoveUpdateCallback(nothingUpdate[i]);
			updater.RemoveUpdateCallback(BU[i]);
			updater.RemoveUpdateCallback(CU[i]);
		}
	}

	static void A() {
		ThreadedJobs.FORCE_SINGLE_THREAD = singleThread;
		ThreadedJobs.DESIRED_NUM_CORES = NUM_THREADS;
	}

	static int i = 0;
	static void B() {
		jobs[i] = ThreadedJobs.QueueMultithreadJob(DoSomething, null);
		i = (i + 1) % BU.Length;
	}
	static void C() {
		jobs[i].WaitForFinish();
		i = (i + 1) % BU.Length;
	}
	static void D() {
		if (finalCount != arraySize) Console.Write("Error!");
		finalCount = 0;
	}

	static void DoNothing() { }

	static int finalCount = 0;
	static void DoSomething() {
		System.Threading.Interlocked.Increment(ref finalCount);
		for (int i = 0; i < loopsDoSomething; i++) ;
	}
}
