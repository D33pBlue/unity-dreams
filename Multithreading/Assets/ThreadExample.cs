using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class ThreadExample : MonoBehaviour
{
    
    public ThreadPoolExecutor threadPool;
    
    private bool hasToLoadTerrain;
    
    // Start is called before the first frame update
    void Start()
    {
        hasToLoadTerrain = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(hasToLoadTerrain){
            hasToLoadTerrain = false;
            
            // if loadTerrain is called here in the main thread, the scene will
            // freeze for almost 10 seconds and then all the logs are showed.
            //loadTerrain();
            
            // on the other hand, if loadTerrain is called inside the threadPool
            // the scene will not freeze and the logs will be printed meanwhile.
            threadPool.AddBackgroundAction(loadTerrain);
            
            // this thread never stops
            threadPool.AddBackgroundAction(keepChanginColor);
            // if keepChanginColor was called directly, the Update function never ends
            // and the scene freezes
        }
        gameObject.transform.eulerAngles = gameObject.transform.eulerAngles + new Vector3(0,1.0f,0);
    }
    
    // Very slow function that should be executed in a separated thread.
    void loadTerrain(){
        Debug.Log("Start load terrain");
        // here maybe you can load from file some data
        // and operate with it ...
        
        // For this example I introduce a sleep in order to 
        // made the function slow.
        for(int i=0;i<10;i++){
            Thread.Sleep(1000);
            Debug.Log("load terrain :: loading ...");
        }
        
        Debug.Log("load terrain :: end load resources");
        
        // now maybe you want to modify the current scene with the data
        // you operated with before, but you can't execute Unity functions
        // directly in a child thread. You can use the ThreadPoolExecutor.
        threadPool.AddMainThreadAction(
            ()=>{ // this add an anonymous function to the threas pool                
                // for this example, this function adds a plane as a terrain
                GameObject plane  = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.transform.position = new Vector3(0,-0.6f,0);
                plane.transform.parent = gameObject.transform.parent;
            }
        );
    }
    
    
    // a function that never ends
    void keepChanginColor(){
        int i = 0;
        while(true){
            if(i==0){
                threadPool.AddMainThreadAction(
                    () => {
                        GetComponent<Renderer>().material.SetColor("_Color",Color.blue);
                    }
                );
            }else{
                threadPool.AddMainThreadAction(
                    () => {
                        GetComponent<Renderer>().material.SetColor("_Color",Color.red);
                    }
                );
            }
            i = (i+1)%2;
            Thread.Sleep(1500);
        }
    }
}
