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
	public class KrAP140:GUIStuff
	{
		string textAreaString="";

		//float fltHDGknob;//deg
		//float fltVSknob;//deg
		float spdknob;
		float hdgknob;
		float altknob;
		float vsknob;
		bool APon;
		bool ATon;
		bool YAWon;
		bool SPDon;
		bool ALTon;
		bool ALTarm;
		bool VSon;
		bool HDGon;


		public Vessel localcurrentvessel;
		//public Vessel localcurrentvessel;
		public APBrain actroll;
		public APBrain actptch;
		public APBrain actyaw;
		public APBrain actthr;
		private AHRS GuiAHRS;
		public int dispmode=0;


		//public C005AP (Vessel ThisVessel) : base(ThisVessel)
		//public void StartAP() 
		public override void APInit (Vessel ThisGoddamnVessel)
		{
			base.OnStart();
			print ("KrAP140 INIT");
			ThisGoddamnVessel = base.vessel;
			/*if (ThisVessel == null) {
				print ("C005AP INIT INPUT NULL VESSEL");
			}*/

			localcurrentvessel = ThisGoddamnVessel;
			localcurrentvessel=ThisGoddamnVessel;
			if (localcurrentvessel == null) {
				print ("KrAP140 INIT NULL VESSEL");
			} else {
				print (localcurrentvessel.ToString ());
			}

			APon = ALTon = false;
			//localcurrentvessel = thisvessel;
			actroll = new APBrain (APBrain.mode.off, localcurrentvessel);
			actptch = new APBrain (APBrain.mode.off, localcurrentvessel);
			actyaw = new APBrain (APBrain.mode.off, localcurrentvessel);
			actthr = new APBrain (APBrain.mode.off, localcurrentvessel);
			GuiAHRS = new AHRS (localcurrentvessel);
			GuiAHRS.updateAHRS ();
			hdgknob = (float)(int)GuiAHRS.hdg;
			vsknob = 0;
		}

		public override void GuiCallbackWrapper ()
		{
			if (isDisp) {
				windowPos = GUILayout.Window (1337, windowPos, APGUI, "Bendit KrAP140 AP", GUILayout.MinWidth (250));
				//print ("GuiCallbackWrapper at C005AP");
			}
		}

		public override void APGUI(int windowID)
		{

			//localcurrentvessel = ThisGoddamnVessel;
			if (localcurrentvessel == null) {
				print ("KrAP140 Null Vessel");
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
			//Switch
			GUILayout.BeginHorizontal ();

			if (GUILayout.Button ("A/P:" + APon.ToString ())) {
				APon = !APon;
				if (APon) {

					if (actroll.armmode != APBrain.mode.off) {
						actroll.ActivateArm ();
					} else {
						actroll.setmode (APBrain.mode.RollLever);
						actroll.setTgt (APBrain.mode.RollLever, 0f);
					}

					if (actptch.armmode != APBrain.mode.off) {
						actptch.ActivateArm ();
					} else {
						actptch.setmode (APBrain.mode.VSHold);
						//actptch.setTgt (APBrain.mode.VSHold, 0f);
					}

				} else {

					actroll.setarmmode (actroll.currmode);
					actroll.setmode (APBrain.mode.off);

					actptch.setarmmode (actptch.currmode);
					actptch.setmode (APBrain.mode.off);
				}

			}
			if (GUILayout.Button ("A/T:" + ATon.ToString ())) {
				ATon = !ATon;
			}

			if (GUILayout.Button ("YAW:" + ATon.ToString ())) {
				YAWon = !YAWon;
				if (YAWon) {
					actyaw.setmode (APBrain.mode.NoSlip);
				} else {
					actyaw.setmode (APBrain.mode.off);
				}
			}
			GUILayout.EndHorizontal ();
			//SPD--TAS for now
			GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("SPD:" + actthr.Statustxt(APBrain.mode.SpdHold))) {
				SPDon = !SPDon;

				if (SPDon && ATon) {
					actthr.setmode (APBrain.mode.SpdHold);
				} else if ((!SPDon) && ATon) {
					actthr.setmode (APBrain.mode.clmp);
					actthr.setTgt (APBrain.mode.clmp, 0.95f);
				} else if (SPDon && (!ATon)) {
					actthr.setarmmode (APBrain.mode.SpdHold);
				} else if ((!SPDon) && (!ATon)) {
					actthr.setarmmode (APBrain.mode.off);
				}

				}
				if (GUILayout.Button ("-")) {
				spdknob=actthr.chgTgt (APBrain.mode.SpdHold, -1f);
				}
				GUILayout.TextArea (spdknob.ToString());
				if (GUILayout.Button ("0")) {
				spdknob=actthr.setTgt (APBrain.mode.SpdHold, (float)(int)GuiAHRS.Ias);
				}
				if (GUILayout.Button ("+")) {
				spdknob=actthr.chgTgt (APBrain.mode.SpdHold, +1f);
				}
			GUILayout.EndHorizontal ();
			//HDG
			GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("HDG:" + actroll.Statustxt(APBrain.mode.HDGHold))) 
				{
					HDGon = !HDGon;
					if (HDGon){
						actroll.setmode (APBrain.mode.HDGHold);
					//fltHDGknob = GuiAHRS.hdg;
					} else {
						actroll.setmode (APBrain.mode.RollLever);
					actroll.setTgt (APBrain.mode.RollLever,0f);
					}


				}
				if (GUILayout.Button ("-")) {
				hdgknob=actroll.chgTgt (APBrain.mode.HDGHold, -1f);
				}

			GUILayout.TextArea (hdgknob.ToString());

				if (GUILayout.Button ("0")) {
				hdgknob=actroll.setTgt (APBrain.mode.HDGHold, (float)(int)GuiAHRS.hdg);
				}
				if (GUILayout.Button ("+")) {
				hdgknob=actroll.chgTgt (APBrain.mode.HDGHold, +1f);
				}
			GUILayout.EndHorizontal ();
			//ALT
			GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("ALT:" + actptch.Statustxt(APBrain.mode.AltSel))) {

				}
				if (GUILayout.Button ("-")) {
				altknob=actptch.chgTgt (APBrain.mode.AltSel, -10f);
				}

			GUILayout.TextArea (altknob.ToString());

				if (GUILayout.Button ("0")) {
				altknob=actptch.setTgt (APBrain.mode.AltSel,(float)(10*(int)(GuiAHRS.BaroAlt/10)));
				}
				if (GUILayout.Button ("+")) {
				altknob=actptch.chgTgt (APBrain.mode.AltSel, +10f);
				}
			GUILayout.EndHorizontal ();
			//VS
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("VS:" + actptch.Statustxt(APBrain.mode.VSHold))) {
					ATon = !ATon;
				}
				if (GUILayout.Button ("-")) {
				vsknob = actptch.chgTgt (APBrain.mode.VSHold, -10);
				}

			GUILayout.TextArea (vsknob.ToString());

				if (GUILayout.Button ("0")) {
				vsknob = actptch.setTgt (APBrain.mode.VSHold, 0f);
				}
				if (GUILayout.Button ("+")) {
				vsknob = actptch.chgTgt (APBrain.mode.VSHold, +10);
				}
			GUILayout.EndHorizontal ();
			GUILayout.EndVertical ();

			this.SetTextAreaString (" BARO:"+GuiAHRS.BaroAlt+" Rad:"+GuiAHRS.RadAlt+" BVS:"+GuiAHRS.BaroVS+" RVS:"+GuiAHRS.RadVS);

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

