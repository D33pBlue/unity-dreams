# Multithreading
This is an example of multithreading in Unity.


I implemented a simple thread pool, users can call to submit tasks that have to be executed in background. 
Child threads can operate on user-defined and primitive types but not on Unity ones. 
For this reason my pool gives the possibility to define callbacks that are executed in the main thread immediately after the background computations.
