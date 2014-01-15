using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using UnityEngine;
using KSP.IO;


namespace KTP2
{
	public class GUIStuff:KTP2
	{
		string textAreaString="";
		//string strrollknob;
		float fltrollknob;//deg
		//string strptchknob;
		float fltptchknob;//deg
		bool APon;
		bool ALTon;
		//List <APBrain> activeap=new List<APBrain>();
		private APBrain actroll;
		private APBrain actptch;
		public int dispmode=0;
		Vessel currvessel;

		public GUIStuff (Vessel thisvessel)
		{

			if ((windowPos.x == 0) && (windowPos.y == 0))//windowPos is used to position the GUI window, lets set it in the center of the screen
			{
				windowPos = new Rect(Screen.width / 2, Screen.height / 2, 200, 50);
			}
			//strrollknob = fltrollknob.ToString ();
			//strptchknob = fltptchknob.ToString ();
			APon = ALTon = false;
			currvessel = thisvessel;
		}

		public void updateGUI(){
			windowPos = GUILayout.Window(1337, windowPos, SpurrryGUI, "Spurrry C-0.05 AP", GUILayout.MinWidth(250));

		}
		private void SpurrryGUI(int windowID)
		{

			GUIStyle mySty = new GUIStyle(GUI.skin.button); 
			mySty.normal.textColor = mySty.focused.textColor = Color.white;
			mySty.hover.textColor = mySty.active.textColor = Color.yellow;
			mySty.onNormal.textColor = mySty.onFocused.textColor = mySty.onHover.textColor = mySty.onActive.textColor = Color.green;
			mySty.padding = new RectOffset(8, 8, 8, 8);
			GUILayout.BeginVertical ();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("MASTER:"+APon.ToString())) {//GUILayout.Button is "true" when clicked
				APon = !APon;
				if (APon) {
					//roll default
					print ("Turning WLV on");
					actroll = new APBrain (APBrain.mode.RollLever,currvessel);
					print ("WLV on");
					//ptch switch
					if (ALTon) {
						actptch = new APBrain (APBrain.mode.PtchLever,currvessel);//placeholder
					} else {
						actptch = new APBrain (APBrain.mode.PtchLever,currvessel);
					}
				} else {
					print("AP Off");
						actptch.disconnect ();
						actroll.disconnect ();

				}

			}
			//print ("At APButton");
			if (GUILayout.Button ("ALT:"+ALTon.ToString())) {
				ALTon = !ALTon;
				if (APon) {
					if (ALTon) {
						//actptch = new APBrain (APBrain.mode.AltHold);
					} else {
						actptch.setmode (APBrain.mode.PtchLever);
					}
				}

			}
			//print ("At ALTButton");
			GUILayout.EndHorizontal ();
			//-------------------
			GUILayout.BeginHorizontal ();
			GUILayout.TextArea (fltrollknob.ToString());
			//try{fltrollknob = float.Parse(strrollknob);}

			if (GUILayout.Button ("<")) {//GUILayout.Button is "true" when clicked
				fltrollknob=actroll.chgTgt (+1f);
			}
			if (GUILayout.Button ("0")) {//GUILayout.Button is "true" when clicked
				fltrollknob = actroll.setTgt (0.0f);
			}
			if (GUILayout.Button (">")) {//GUILayout.Button is "true" when clicked
				fltrollknob=actroll.chgTgt (-1f);
			}


			GUILayout.EndHorizontal ();
			//-------------------
			GUILayout.BeginHorizontal ();
			GUILayout.TextArea (fltptchknob.ToString());
			//try{fltptchknob = float.Parse(strptchknob);}

			if (GUILayout.Button ("UP")) {//GUILayout.Button is "true" when clicked
				/*if (fltptchknob <= +90f) {
					fltptchknob += 1;.
				}*/
				fltptchknob=actptch.chgTgt (+1f);
			}
			if (GUILayout.Button ("0")) {//GUILayout.Button is "true" when clicked
				fltptchknob = actptch.setTgt (0f);
			}
			if (GUILayout.Button ("DN")) {//GUILayout.Button is "true" when clicked
				/*if (fltptchknob >= -90f) {
					fltptchknob -= 1;
				}*/
				fltptchknob=actptch.chgTgt (-1f);
			}

			/*if (actptch!=null){//&&actptch.currmode == APBrain.mode.PtchLever) {
				actptch.setTgt (fltptchknob);
				print ("PtchKnob:" + fltptchknob.ToString ("0.00"));
			}*/
			//print ("At PtchKnob");
			GUILayout.EndHorizontal ();
			//-------------------
			GUILayout.EndVertical ();

			this.AddTextAreaString (" RCMD:" + fltrollknob + " PTCMD" + fltptchknob);

			GUILayout.TextArea (textAreaString);

			GUI.DragWindow();

		}


		public void AddTextAreaString(string addstr){
			textAreaString += addstr;
		}

		public void SetTextAreaString(string newstr){
			textAreaString = newstr;
		}


	}
}

