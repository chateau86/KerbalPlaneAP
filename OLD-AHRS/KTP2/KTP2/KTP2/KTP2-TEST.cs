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
		//private string textAreaString = "text area";
		protected Rect windowPos;
		//public int dispmode=0;
		private bool started=false;
		private AHRS thisAHRS;
		private GUIStuff thisGUIStuff;

		public static Vessel ThisGoddamnVessel;

		public enum axis
		{
			roll,
			pitch,
			yaw,
			mainThrottle
		}


		public override void OnStart(StartState state)
		{

		}

		public override void OnUpdate(){
			ThisGoddamnVessel = vessel;
			if (!started) {
				startup ();
			}
			//print ("thisAHRS.updateAHRS");
			thisAHRS.updateAHRS ();//vessel);
		}


		public void startup(){
			print ("CRANK IT OVER!");
			ThisGoddamnVessel = vessel;
			started = true;
			//RenderingManager.AddToPostDrawQueue (3, new Callback (drawGUI));
			thisGUIStuff = new GUIStuff (vessel);
			thisAHRS = new AHRS (vessel);
		}


		private void OnGUI()
		{
			if (!started) {
				return;
			}

			thisGUIStuff.updateGUI ();
			thisGUIStuff.SetTextAreaString (thisAHRS.debugAHRS (5));
		}

	}
}

