using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using UnityEngine;
using KSP.IO;
//using Tac;

//GitHub app is fucking dumb

namespace KTP2
{
	public class KTP2 : PartModule
	{
		//private string textAreaString = "text area";

		//public int dispmode=0;
		//private bool started=false;
		//private AHRS thisAHRS;
		//private GUIStuff thisGUIStuff;
		//private C005AP thisAP;// duct tape
		public static Vessel ThisGoddamnVessel;

		public enum axis
		{
			roll,
			pitch,
			yaw,
			mainThrottle
		}



		/*public KTP2(){
			ThisGoddamnVessel = vessel;
			if (!started) {
				startup ();
			}

			//print ("thisAHRS.updateAHRS");
			//thisAHRS.updateAHRS ();//vessel);
		}*/


		/*public void startup(){
			print ("CRANK IT OVER!");
			ThisGoddamnVessel = vessel;
			if(ThisGoddamnVessel==null){
				print("starter fault");
			}
			started = true;
			//RenderingManager.AddToPostDrawQueue (3, new Callback (drawGUI));
			//thisAP = new C005AP;// (ThisGoddamnVessel);
			//thisGUIStuff = (GUIStuff)thisAP;
			//thisAHRS = new AHRS (vessel);
	}*/


		private void OnGUI()
		{
			ThisGoddamnVessel = vessel;
		}

	}
}

