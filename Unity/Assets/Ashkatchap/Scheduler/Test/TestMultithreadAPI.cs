﻿using Ashkatchap.UnityScheduler;
using System;
using UnityEngine;
using UnityEngine.Profiling;

public class TestMultithreadAPI : MonoBehaviour {
	UpdateReference firstUpdate;
	UpdateReference secondUpdate;
	Ashkatchap.Scheduler.QueuedJob[] jobs;

	public int arraySize = 10000;
	public ushort multithreadIterations = 100;
	public bool singleThread = false;
	public int NUM_THREADS = 8;

	public int workPerIteration = 10000;
	
	Action UpdateMethod1Cached;
	Action UpdateMethod2Cached;
	Ashkatchap.Scheduler.Job MultithreadDoNothingCached;
	private void Awake() {
		UpdateMethod1Cached = UpdateMethod1;
		UpdateMethod2Cached = UpdateMethod2;
		MultithreadDoNothingCached = MultithreadDoNothing;
	}

	private bool started = false;
	void OnEnable() {
		if (jobs == null || jobs.Length != arraySize) {
			jobs = new Ashkatchap.Scheduler.QueuedJob[arraySize];
		}
		firstUpdate = UpdaterAPI.AddUpdateCallback(UpdateMethod1Cached, QueueOrder.Update, 127);
		secondUpdate = UpdaterAPI.AddUpdateCallback(UpdateMethod2Cached, QueueOrder.Update, 128);
		started = true;
	}
	void OnDisable() {
		UpdaterAPI.RemoveUpdateCallback(firstUpdate);
		UpdaterAPI.RemoveUpdateCallback(secondUpdate);
		started = false;
	}

	private void OnGUI() {
		GUILayout.Label("array Size");
		arraySize = int.Parse(GUILayout.TextField(arraySize.ToString()));
		if (!started && GUILayout.Button("START")) {
			OnEnable();
		}
		if (started && GUILayout.Button("END")) {
			OnDisable();
		}
		GUILayout.Label("multithread Iterations");
		multithreadIterations = (ushort) int.Parse(GUILayout.TextField(multithreadIterations.ToString()));
			
		singleThread = GUILayout.Toggle(singleThread, "single Thread");

		GUILayout.Label("NUM_THREADS");
		NUM_THREADS = int.Parse(GUILayout.TextField(NUM_THREADS.ToString()));

		GUILayout.Label("work Per Iteration");
		workPerIteration = int.Parse(GUILayout.TextField(workPerIteration.ToString()));
	}
	
	void UpdateMethod1() {
		Ashkatchap.Scheduler.ThreadedJobs.FORCE_SINGLE_THREAD = singleThread;
		Ashkatchap.Scheduler.ThreadedJobs.DESIRED_NUM_CORES = NUM_THREADS;
		Profiler.BeginSample("Add Multithread");
		for (int i = 0; i < jobs.Length; i++) {
			Ashkatchap.Scheduler.ThreadedJobs.QueueMultithreadJob(MultithreadDoNothingCached, multithreadIterations, out jobs[i]);
		}
		Profiler.EndSample();
	}
		
	void UpdateMethod2() {
		Profiler.BeginSample("Wait Multithread");
		for (int i = 0; i < jobs.Length; i++) {
			jobs[i].WaitForFinish();
		}
		Profiler.EndSample();
	}

	void MultithreadDoNothing(int index) {
			
		int ignore = 1;
		for (int i = 0; i < workPerIteration; i++) ignore += i % ignore;
			
		//Thread.Sleep(1);
	}
}
