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
	public class C005AP:GUIStuff
	{
		string textAreaString="";

		float fltrollknob;//deg
		float fltptchknob;//deg
		bool APon;
		bool ALTon;

		public Vessel localcurrentvessel;
		//public Vessel localcurrentvessel;
		public APBrain actroll;
		public APBrain actptch;
		private AHRS GuiAHRS;
		public int dispmode=0;


		//public C005AP (Vessel ThisVessel) : base(ThisVessel)
		//public void StartAP() 
		public override void APInit (Vessel ThisGoddamnVessel)
		{
			base.OnStart();
			print ("C005AP INIT");
			ThisGoddamnVessel = base.vessel;
			/*if (ThisVessel == null) {
				print ("C005AP INIT INPUT NULL VESSEL");
			}*/

			localcurrentvessel = ThisGoddamnVessel;
			localcurrentvessel=ThisGoddamnVessel;
			if (localcurrentvessel == null) {
				print ("C005AP INIT NULL VESSEL");
			} else {
				print (localcurrentvessel.ToString ());
			}

			APon = ALTon = false;
			//localcurrentvessel = thisvessel;
			actroll = new APBrain (APBrain.mode.off, localcurrentvessel);
			actptch = new APBrain (APBrain.mode.off, localcurrentvessel);
			GuiAHRS = new AHRS (localcurrentvessel);
		}

		public override void GuiCallbackWrapper ()
		{
			if (isDisp) {
				windowPos = GUILayout.Window (1337, windowPos, APGUI, "Spurrry C-0.05 AP", GUILayout.MinWidth (250));
				//print ("GuiCallbackWrapper at C005AP");
			}
		}

		public override void APGUI(int windowID)
		{

			//localcurrentvessel = ThisGoddamnVessel;
			if (localcurrentvessel == null) {
				print ("C005AP Null Vessel");
				localcurrentvessel = ThisGoddamnVessel;
			}
			//--------------------------------------------------------------------------------------------------------------------------------------------------------
			if ((windowPos.x == 0) && (windowPos.y == 0))//windowPos is used to position the GUI window, lets set it in the center of the screen
			{
				windowPos = new Rect(Screen.width / 2, Screen.height / 2, 200, 50);
			}
			GuiAHRS.updateAHRS ();
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
					//actroll = new APBrain (APBrain.mode.RollLever,localcurrentvessel);
					actroll.setmode (APBrain.mode.RollLever);
					print ("WLV on");
					//ptch switch
					if (ALTon) {
						//actptch = new APBrain (APBrain.mode.PtchLever,localcurrentvessel);//placeholder
					} else {
						//actptch = new APBrain (APBrain.mode.PtchLever,localcurrentvessel);
						actptch.setmode (APBrain.mode.PtchLever);
					}
				} else {
					print("AP Off");
					/*actptch.disconnect ();
					//actptch = null;
					actroll.disconnect ();
					actroll = null;*/
					actroll.setmode (APBrain.mode.off);
					actptch.setmode (APBrain.mode.off);
				}

			}
			//print ("At APButton");
			if (GUILayout.Button ("ALT:"+ALTon.ToString())) {
				ALTon = !ALTon;
				if (APon) {
					if (ALTon) {
						actptch.setmode (APBrain.mode.AltHold);
						actptch.setTgt (APBrain.mode.AltHold,GuiAHRS.BaroAlt);
						actptch.setTgtTrim (GuiAHRS.ptch);
					} else {
						actptch.setmode (APBrain.mode.PtchLever);
						actptch.setTgt (APBrain.mode.PtchLever,fltptchknob);
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
				fltrollknob=actroll.chgTgt (APBrain.mode.RollLever,-1f);
			}
			if (GUILayout.Button ("0")) {//GUILayout.Button is "true" when clicked
				fltrollknob = actroll.setTgt (APBrain.mode.RollLever,0.0f);
			}
			if (GUILayout.Button (">")) {//GUILayout.Button is "true" when clicked
				fltrollknob=actroll.chgTgt (APBrain.mode.RollLever,+1f);
			}


			GUILayout.EndHorizontal ();
			//-------------------
			GUILayout.BeginHorizontal ();
			GUILayout.TextArea (fltptchknob.ToString());
			//try{fltptchknob = float.Parse(strptchknob);}

			if (GUILayout.Button ("UP")) {//GUILayout.Button is "true" when clicked
				if (!ALTon) {
					fltptchknob = actptch.chgTgt (APBrain.mode.PtchLever,+1f);
				} else {
					fltptchknob++;
				}
			}
			if (GUILayout.Button ("0")) {//GUILayout.Button is "true" when clicked
				fltptchknob = actptch.setTgt (APBrain.mode.PtchLever,(float)(int)GuiAHRS.ptch);
			}
			if (GUILayout.Button ("DN")) {//GUILayout.Button is "true" when clicked
				if (!ALTon) {
					fltptchknob=actptch.chgTgt (APBrain.mode.PtchLever,-1f);
				} else {
					fltptchknob++;
				}
			}
				
			//print ("At PtchKnob");
			GUILayout.EndHorizontal ();
			//-------------------
			GUILayout.EndVertical ();

			this.SetTextAreaString ("BARO:"+GuiAHRS.BaroAlt.ToString("F2")+" Rad:"+GuiAHRS.RadAlt.ToString("F2")+" Spd:"+GuiAHRS.Tas.ToString("f2")+" AOA:"+GuiAHRS.aoa.ToString("F2")+" Sideslip:"+GuiAHRS.sideslip.ToString("F2")+" rho:"+GuiAHRS.reldensity.ToString("f2"));

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

