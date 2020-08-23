using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;


/*
* Unity supports multithreading, but "Unity's types" can only be accessed or modified
* inside the main thread. Child threads should operate on primitive or custom types and
* then their results should be passed back to the main thread. 
* This class helps in the synchronization and distributes the background
* operations among a pool of threads;
*/

public class ThreadPoolExecutor : MonoBehaviour
{
    
    // the number of threads that runs in this pool
    public int NumberOfThreads = 2;
    
    
    // reference to the threads
    private Thread[] threads;
    
    // set to false to stop receiving actions and to end the threads in the pool
    private bool keepThreadExecuting;
    
    // A queue for the actions that should be executed in the main thread (at the 
    // end of some computation performed in the child thread). An action is any
    // function with no parameters and no return.
    private Queue<Action> actionsToRunInMainThread;
    
    // A queue for the actions that should be executed in a child thread.
    private Queue<Action> actionsToRunInBackground;
    
    
    // Adds an action to the background queue if the pool is still active and returns true;
    // it returns false otherwise (if the pool is not active anymore).
    public bool AddBackgroundAction(Action action){
        if(keepThreadExecuting){
            lock(actionsToRunInBackground){
                actionsToRunInBackground.Enqueue(action);
            }
            return true;
        }
        return false;
    }
    
    
    // Adds an action to the main thread queue.
    // This can be called inside a child thread to apply some 
    // modifications to some unity types, after some calculations.
    public void AddMainThreadAction(Action action){
        lock(actionsToRunInMainThread){
            actionsToRunInMainThread.Enqueue(action);
        }
    }
    
    public void StopReceiving(){
        keepThreadExecuting = false;
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        // initialize the queues
        actionsToRunInBackground = new Queue<Action>();
        actionsToRunInMainThread = new Queue<Action>();
        keepThreadExecuting = true;
        
        // initialize the threads
        threads = new Thread[NumberOfThreads];
        for(int i=0;i<NumberOfThreads;i++){
            threads[i] = new Thread(new ThreadStart(runningThread));
            threads[i].Start();
        }
    }
    
    // Stop receiving actions on pool's destroy
    void OnDestroy(){
        StopReceiving();
        for(int i=0;i<NumberOfThreads;i++){
            threads[i].Abort();
        }
    }

    // Executes all the last actions in actionsToRunInMainThread
    void Update()
    {
        int actionsToExecute = 0;
        lock(actionsToRunInMainThread){
            actionsToExecute = actionsToRunInMainThread.Count;
        }
        while(actionsToExecute>0){
            Action nextAction;
            lock(actionsToRunInMainThread){
                nextAction = actionsToRunInMainThread.Dequeue();
                actionsToExecute = actionsToRunInMainThread.Count;
            }
            nextAction();
        }
    }
    
    // This function is executed in each thread and should execute 
    // the actions in actionsToRunInBackground.
    // If there is no action to perform, the threads stay in busy wait.
    void runningThread(){
        while(keepThreadExecuting){
            Action nextAction = ()=>{};// check for actions..
            bool found = false;
            lock(actionsToRunInBackground){
                if(actionsToRunInBackground.Count>0){
                    found = true;
                    nextAction = actionsToRunInBackground.Dequeue();
                }
            }
            if(found){
                nextAction(); // execute the action in the current thread
            }else{
                Thread.Sleep(1);
            }
        }
    }
}
