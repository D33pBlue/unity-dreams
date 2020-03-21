/*
d33pblue (c) 2020

Simple Inverse Kinematics class that is used
to update the legs of a spider.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SimpleIK : MonoBehaviour
{

	[Header("IK Armature")]
	public int ChainLength = 2;
	public Transform target;
	public Transform pole;

	[Header("Solver parameters")]
	public int iterations = 10;
	public float delta = 0.001f;

	[Range(0,1)]
	public float SnapBackStrenght = 1;

	Bone[] bones;
	float completeLength;
	Quaternion startRotTarget;
	Quaternion startRotRoot;

	void Awake(){
		Init();
	}

    // Initialize the bones
	void Init(){
		startRotTarget = target.rotation;
		bones = new Bone[ChainLength+1];
		completeLength = 0;
		Transform cur = transform;
		for(int i=bones.Length-1;i>=0;i--){
			bool leaf = (i==bones.Length-1);
			Quaternion rot = cur.rotation;
			Vector3 dir = target.position-cur.position;//for leaf
			float boneLen = 0;
			if(!leaf){
				dir = bones[i+1].transf.position-cur.position;
				boneLen = (bones[i+1].transf.position-cur.position).magnitude;
				completeLength += boneLen;
			}
			bones[i] = new Bone(boneLen,cur,leaf,dir,rot);
			cur = cur.parent;
		}
		startRotRoot = bones[0].transf.rotation;
	}

    // Set the target towards the ground
	void Update(){
		RaycastHit hit;
        if (Physics.Raycast(target.position+Vector3.up*10,new Vector3(0,-1,0), out hit)){
            target.position = hit.point-Vector3.up*0.5f;
        }
	}

    void LateUpdate(){
    	ResolveIK();
    }

    // Method to apply Inverse Kinematics
    void ResolveIK(){
    	if(target==null){return;}
    	if(bones.Length!=ChainLength){
    		Init();
    	}

    	for(int i=0;i<bones.Length;i++){
    		bones[i].resetPos();
    	}

    	Quaternion rootRot = (bones[0].transf.parent!=null)?
    		bones[0].transf.parent.rotation : Quaternion.identity;
    	Quaternion rootRotDiff = rootRot*Quaternion.Inverse(startRotRoot);

    	// IK calculations
    	if((target.position-bones[0].transf.position).sqrMagnitude
    								>= completeLength*completeLength){
            // The target is unreachable
    		Vector3 dir = (target.position-bones[0].pos).normalized;
    		for(int i=1;i<bones.Length;i++){
    			bones[i].pos = bones[i-1].pos+dir*bones[i-1].length;
    		}
    	}else{
        // The target is reachable
    		for(int it=0;it<iterations;it++){

    			// backward phase to set positions
    			for(int i=bones.Length-1;i>0;i--){
    				if(i==bones.Length-1){
    					bones[i].pos = target.position;
    				}else{
    					bones[i].pos = bones[i+1].pos+
    						(bones[i].pos-bones[i+1].pos).normalized*bones[i].length;
    				}
    			}

    			// forward phase to set positions
    			for(int i=1;i<bones.Length;i++){
    				bones[i].pos = bones[i-1].pos+
    					(bones[i].pos-bones[i-1].pos).normalized*bones[i-1].length;
    			}

    			// move towards pole
    			if(pole!=null){
    				for(int i=1;i<bones.Length-1;i++){
    					Plane plane = new Plane(
    						bones[i+1].pos-bones[i-1].pos,bones[i-1].pos);
    					Vector3 projPole = plane.ClosestPointOnPlane(pole.position);
    					Vector3 projBone = plane.ClosestPointOnPlane(bones[i].pos);
    					float angle = Vector3.SignedAngle(
    						projBone-bones[i-1].pos,
    						projPole-bones[i-1].pos,
    						plane.normal);
    					bones[i].pos = Quaternion.AngleAxis(angle,plane.normal)*
    						(bones[i].pos-bones[i-1].pos)+bones[i-1].pos;
    				}
    			}

    			// check positions and exit iteration
    			if((bones[bones.Length-1].pos-
    					target.position).sqrMagnitude < delta*delta){
    				break;
    			}

    		}

    	}

        // set rotations and apply tranform
    	for(int i=0;i<bones.Length;i++){
    		if(i==bones.Length-1){
    			bones[i].transf.rotation = target.rotation *
    				Quaternion.Inverse(startRotTarget) * bones[i].startRot;
    		}else{
    			bones[i].transf.rotation = Quaternion.FromToRotation(
    				bones[i].startDir,bones[i+1].pos-bones[i].pos)*bones[i].startRot;
    		}
    		bones[i].applyPos();
    	}

    }

    // Draw the bones
    void OnDrawGizmos(){
    	var cur = this.transform;
    	for(int i=0;i<ChainLength&&cur!=null&&cur.parent!=null;i++){
    		float scale = Vector3.Distance(cur.position,cur.parent.position)*0.1f;
    		Handles.matrix = Matrix4x4.TRS(cur.position,
    			Quaternion.FromToRotation(Vector3.up,cur.parent.position-cur.position),
    			new Vector3(scale,Vector3.Distance(cur.parent.position,cur.position),scale));
    		Handles.color = Color.green;
    		Handles.DrawWireCube(Vector3.up*0.5f,Vector3.one);
    		Gizmos.color = Color.red;
    		Gizmos.DrawSphere(cur.position,0.4f);
    		cur = cur.parent;
    	}
    	Gizmos.color = Color.red;
    	Gizmos.DrawSphere(cur.position,0.4f);
    }
}

// bones data structure
public struct Bone{
	public float length;
	public Transform transf;
	public bool leaf;
	public Vector3 pos;
	public Vector3 startDir;
	public Quaternion startRot;

	public Bone(float l,Transform t,bool lf,Vector3 d,Quaternion r){
		length = l;
		transf = t;
		leaf = lf;
		pos = Vector3.zero;
		startDir = d;
		startRot = r;
	}

	public void resetPos(){
		pos = transf.position;
	}

	public void applyPos(){
		transf.position = pos;
	}
}
