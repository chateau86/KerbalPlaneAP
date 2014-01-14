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
			//RenderingManager.AddToPostDrawQueue (3, new Callback (drawGUI));
			thisGUIStuff = new GUIStuff ();
			thisAHRS = new AHRS ();
		}


		private void OnGUI()
		{
			if (!started) {
				return;
			}

			print ("AT OnGUI");
			print ("thisAHRS.updateAHRS");
			thisAHRS.updateAHRS (vessel);
			print ("thisGUIStuff.updateGUI");
			thisGUIStuff.updateGUI ();
			print ("thisGUIStuff.SetTextAreaString");
			thisGUIStuff.SetTextAreaString (thisAHRS.debugAHRS (thisGUIStuff.dispmode));
		}

	}
}

