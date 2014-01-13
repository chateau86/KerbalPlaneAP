using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using UnityEngine;
using KSP.IO;
//using Tac;

namespace KTP2
{
	public class KTP2 : PartModule
	{
		private string textAreaString = "text area";
		protected Rect windowPos;
		public int dispmode=0;
		private bool started=false;

		public override void OnStart(StartState state)
		{

		}

		public override void OnUpdate(){
			if (!started) {
				startup ();
			}
		}


		public void startup(){
			print ("CRANK IT OVER!");
			started = true;
			RenderingManager.AddToPostDrawQueue (3, new Callback (drawGUI));

			if ((windowPos.x == 0) && (windowPos.y == 0))//windowPos is used to position the GUI window, lets set it in the center of the screen
			{
				windowPos = new Rect(Screen.width / 2, Screen.height / 2, 200, 50);
			}



			vessel.OnFlyByWire += new FlightInputCallback(fly);

		}

		private void WindowGUI(int windowID)
		{
			GUIStyle mySty = new GUIStyle(GUI.skin.button); 
			mySty.normal.textColor = mySty.focused.textColor = Color.white;
			mySty.hover.textColor = mySty.active.textColor = Color.yellow;
			mySty.onNormal.textColor = mySty.onFocused.textColor = mySty.onHover.textColor = mySty.onActive.textColor = Color.green;
			mySty.padding = new RectOffset(8, 8, 8, 8);

			GUILayout.BeginVertical();
			if (GUILayout.Button("Toggle",mySty,GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
			{	
				dispmode++;
			}
			if (GUILayout.Button("Mode",mySty,GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
			{	
				//switch
				dispmode = 0;
			}
			//textAreaString = GUI.TextArea (new Rect (30, 25, 300, 30), textAreaString);
			GUILayout.Label (textAreaString);

			GUILayout.EndVertical();

			//DragWindow makes the window draggable. The Rect specifies which part of the window it can by dragged by, and is 
			//clipped to the actual boundary of the window. You can also pass no argument at all and then the window can by
			//dragged by any part of it. Make sure the DragWindow command is AFTER all your other GUI input stuff, or else
			//it may "cover up" your controls and make them stop responding to the mouse.
			GUI.DragWindow(new Rect(0, 0, 10000, 20));

		}

		private void drawGUI()
		//public override void OnUpdate()
		{
			if (!started) {
				return;
			}
			//GUI.skin = HighLogic.Skin;
			updateAHRS ();
			windowPos = GUILayout.Window(1337, windowPos, WindowGUI, "Toggle", GUILayout.MinWidth(1000));	 
		}

		public void updateAHRS(){//working like a charm
			//print ("GYRO SPININ'");
			Vector3d headingvec = (Vector3d)vessel.transform.up;//point forward
			Vector3d normvec = (Vector3d)vessel.transform.forward;//point down
			Vector3d position = vessel.findWorldCenterOfMass();
			Vector3d rollvec = Vector3d.Cross (normvec, headingvec);//point to left
			//unit vectors in the up (normal to planet surface), east, and north (parallel to planet surface) directions
			Vector3d eastUnit = vessel.mainBody.getRFrmVel(position).normalized; //uses the rotation of the body's frame to determine "east"
			Vector3d upUnit = (position - vessel.mainBody.position).normalized;
			Vector3d northUnit = Vector3d.Cross(upUnit, eastUnit); //north = up cross east

			Vector3d leftunit = Vector3d.Cross (headingvec, upUnit);
			//Vector3d upaxis = vessel.upAxis;
			float northcomponent = (float)Vector3d.Dot (headingvec, northUnit);
			float eastcomponent = (float)Vector3d.Dot (headingvec, eastUnit);
			float upcomponent = (float)Vector3d.Dot (headingvec, upUnit);

			float rollcomponentup = (float)Vector3d.Dot (rollvec, upUnit);
			float rollcomponentleft = (float)Vector3d.Dot (rollvec, leftunit);

			float hdg = (float)(Math.Atan2(eastcomponent,northcomponent)*(180/Math.PI));// atan return rad!
			float ptch = (float)(Math.Atan2(upcomponent, Math.Sqrt (1 - upcomponent * upcomponent))*(180/Math.PI));
			//float roll = (float)(Math.Atan2 (rollcomponent, Math.Sqrt (1 - rollcomponent * rollcomponent)) * (180 / Math.PI));//less than +-90      
			float roll = (float)(Math.Atan2(rollcomponentup,rollcomponentleft)*(180/Math.PI));// atan return rad!                                             
			switch (dispmode) {
			case 0:textAreaString = "Heading:" + ((Vector3)headingvec).ToString ();break;
			case 1:textAreaString = "pos:"+((Vector3)position).ToString();break;
				//case 2:textAreaString = "UpAx:"+((Vector3)upaxis).ToString();break;
			case 2:textAreaString = "UP:"+((Vector3)upUnit).ToString();break;
			case 3:textAreaString = "N:"+((Vector3)northUnit).ToString();break;
			case 4:textAreaString = "E:"+((Vector3)eastUnit).ToString();break;
			case 5:textAreaString = "HDG:" + hdg + "PTCH:" + ptch + "ROLL:" + roll;break;
			case 6:textAreaString = "Normal:"+((Vector3)normvec).ToString();break;
			default:
				textAreaString = "PRESS MODE TO RESET";
				break;
			
			}

		}

		private void fly(FlightCtrlState s){

		}

	}
}

