# Custom Update/LateUpdate for Unity

## What is it?

Frame Update Manager and Multithreaded Job Scheduler
For Unity it gives you more control that using Update/LateUpdate as it allows you to add/remove Update callbacks
to 6 different queues with a given priority between 0 (max) to 255 (min):

- PreFixedUpdate
- FixedUpdate
- PostFixedUpdate
- AfterFixedUpdate (In order, After FixedUpdate is executed and a physics step performed, this queue is executed)
- PreUpdate
- Update
- PostUpdate
- PreLateUpdate
- LateUpdate
- PostLateUpdate



## How to use it?

Add to your script `using Ashkatchap.Updater;`

### How to use the Frame Update Manager?

#### For Unity Users:

1. Paste the contents of https://github.com/forestrf/Scheduler/tree/master/Unity/Assets to your project
2. Inside any script that needs to be updated with this solution add:
    1. A variable `UpdateReferenceQ updateRef`
    2. A method `void MyUpdateFunc() { }`
    3. Inside `OnEnable`: `updateRef = UpdaterAPI.AddUpdateCallback(MyUpdateFunc, QueueOrder.Update, 127);`
    4. Inside `OnDisable`: `UpdaterAPI.RemoveUpdateCallback(updateRef);`
3. You can call `UpdaterAPI.QueueCallback(actionCallback, QueueOrder.Update)` to queue a callback to be executed
on the main thread
    

#### For Non-Unity Users:

1. Create a Updater `var updater = new Updater();`. You can create as many as you want
2. Follow points 2.3 and 2.4 to add and remove callbacks
3. Call `updater.Execute();` inside your Game Update Loop
4. You can call `updater.QueueCallback(actionCallback, QueueOrder.Update)` to queue a callback to be executed
on the main thread

### How to use the Multithreaded Job Scheduler?

For Non-Unity Users: Call `Scheduler.MultithreadingStart(updater);` being `updater` a Updater object that
will call `Execute();` inside the Game Update Loop.

To Add a new job, call `var job = Scheduler.QueueMultithreadJob(a, b, c)`; being:
* a = callback
* b = number of times that the callback has to be called (the calls will be executed in Any thread, even the main thread)
* c = priority

the variable job contains the methods `Destroy` (given that it is multithreaded it may still be able to be executed)
and `WaitForFinish` (Only works in the main thread, and blocks the thread until the job finishes executing, contributing to it)

The main thread is the thread where the Updater that uses the Scheduler was created
