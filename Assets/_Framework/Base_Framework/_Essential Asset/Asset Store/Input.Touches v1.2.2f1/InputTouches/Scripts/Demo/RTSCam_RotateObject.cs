using UnityEngine;
using System.Collections;

public class RTSCam_RotateObject : MonoBehaviour {

	public Transform targetT;
	protected Transform targetP;

	protected float dist;
	
	protected float orbitSpeedX;
	protected float orbitSpeedY;
	protected float zoomSpeed;
	
	public float rotXSpeedModifier=0.25f;
	public float rotYSpeedModifier=0.25f;
	public float zoomSpeedModifier=5;
	
	public float minRotX=-60;
	public float maxRotX=60;
	
	//public float panSpeedModifier=1f;
	
	// Use this for initialization
	protected virtual void Start () {
		dist=transform.localPosition.z;
		
		//create a dummy transform as the parent of the target
		//we will rotate this parent along y-axis, so the target will only rotate along local x-axis
		GameObject obj=new GameObject("Target Parent");
		obj.transform.position=targetT.position;
		targetT.parent=obj.transform;
		targetP=obj.transform;
		
		
		DemoSceneUI.SetSceneTitle("Alternate RTS camera, the object in focus moves instead of the camera");
		
		string instInfo="";
			instInfo+="- swipe or drag on screen to rotate the camera\n";
			instInfo+="- pinch or using mouse wheel to zoom in/out\n";
			instInfo+="- swipe or drag on screen with 2 fingers to move around\n";
			instInfo+="- single finger interaction can be simulate using left mosue button\n";
			instInfo+="- two fingers interacion can be simulate using right mouse button";
		DemoSceneUI.SetSceneInstruction(instInfo);
	}
	
	protected virtual void OnEnable(){
		IT_Gesture.onDraggingE += OnDragging;
		//IT_Gesture.onMFDraggingE += OnMFDragging;
		
		IT_Gesture.onPinchE += OnPinch;
	}
	
	protected virtual void OnDisable(){
		IT_Gesture.onDraggingE -= OnDragging;
		//IT_Gesture.onMFDraggingE -= OnMFDragging;
		
		IT_Gesture.onPinchE -= OnPinch;
	}

	
	// Update is called once per frame
	protected virtual void Update () {
		//original code
		//targetT.Rotate(new Vector3(-orbitSpeedX, -orbitSpeedY, 0), Space.World);
		
		//calculate the x-axis rotation, this is applied to the target
		//get the current rotation
		float x=targetT.localRotation.eulerAngles.x;
		//make sure x is between -180 to 180 so we can clamp it propery later
		if(x>180) x-=360;
		//calculate the x and y rotation
		Quaternion rotationX=Quaternion.Euler(Mathf.Clamp(x+orbitSpeedX, minRotX, maxRotX), 0, 0);
		//apply the rotation
		targetT.localRotation=rotationX;
		
		//calculate the y-axis rotation, this is applied to the target's parent
		float y=targetP.localRotation.eulerAngles.y;
		Quaternion rotationY=Quaternion.Euler(0, y, 0)*Quaternion.Euler(0, orbitSpeedY, 0);
		targetP.rotation=rotationY;
		
		
		transform.parent.position=targetT.position;
		
		//calculate the zoom and apply it
		dist+=Time.deltaTime*zoomSpeed*0.01f;
		dist=Mathf.Clamp(dist, -15, -3);
		transform.localPosition=new Vector3(0, 0, dist);
		
		//reduce all the speed
		orbitSpeedX*=(1-Time.deltaTime*12);
		orbitSpeedY*=(1-Time.deltaTime*3);
		zoomSpeed*=(1-Time.deltaTime*4);
		
		//use mouse scroll wheel to simulate pinch, sorry I sort of cheated here
		zoomSpeed+=Input.GetAxis("Mouse ScrollWheel")*500*zoomSpeedModifier;
	}
	
	//called when one finger drag are detected
	protected virtual void OnDragging(DragInfo dragInfo){
		//apply the DPI scaling
		dragInfo.delta/=IT_Gesture.GetDPIFactor();
		//vertical movement is corresponded to rotation in x-axis
		orbitSpeedX=-dragInfo.delta.y*rotXSpeedModifier;
		//horizontal movement is corresponded to rotation in y-axis
		orbitSpeedY=dragInfo.delta.x*rotYSpeedModifier;
	}
	
	//called when pinch is detected
	protected void OnPinch(PinchInfo pinfo){
		zoomSpeed-=pinfo.magnitude*zoomSpeedModifier/IT_Gesture.GetDPIFactor();
	}
	
	//called when a dual finger or a right mouse drag is detected
	/*
	void OnMFDragging(DragInfo dragInfo){
		
		//make a new direction, pointing horizontally at the direction of the camera y-rotation
		Quaternion direction=Quaternion.Euler(0, transform.parent.rotation.eulerAngles.y, 0);
		
		//calculate forward movement based on vertical input
		Vector3 moveDirZ=transform.parent.InverseTransformDirection(direction*Vector3.forward*-dragInfo.delta.y);
		//calculate sideway movement base on horizontal input
		Vector3 moveDirX=transform.parent.InverseTransformDirection(direction*Vector3.right*-dragInfo.delta.x);
		
		//move the cmera 
		transform.parent.Translate(moveDirZ * panSpeedModifier * Time.deltaTime);
		transform.parent.Translate(moveDirX * panSpeedModifier * Time.deltaTime);
		
	}
	*/
	
	
	/*
	private bool instruction=false;
	void OnGUI(){
		string title="RTS camera, the camera will orbit around a pivot point but the rotation in z-axis is locked.";
		GUI.Label(new Rect(150, 10, 400, 60), title);
		
		if(!instruction){
			if(GUI.Button(new Rect(10, 55, 130, 35), "Instruction On")){
				instruction=true;
			}
		}
		else{
			if(GUI.Button(new Rect(10, 55, 130, 35), "Instruction Off")){
				instruction=false;
			}
			
			GUI.Box(new Rect(10, 100, 400, 100), "");
			
			string instInfo="";
			instInfo+="- swipe or drag on screen to rotate the camera\n";
			instInfo+="- pinch or using mouse wheel to zoom in/out\n";
			instInfo+="- swipe or drag on screen with 2 fingers to move around\n";
			instInfo+="- single finger interaction can be simulate using left mosue button\n";
			instInfo+="- two fingers interacion can be simulate using right mouse button";
			
			GUI.Label(new Rect(15, 105, 390, 90), instInfo);
		}
	}
	*/
	
}
