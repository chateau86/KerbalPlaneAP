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
	public abstract class GUIStuff:KTP2
	{

		public Vessel currvessel;
		public FlightCtrlState fbwhook;
		public bool isDisp;
		public Rect windowPos;
		public bool APInitd = false;

		//public GUIStuff (Vessel thisvessel)
		public void OnStart()
		{

			isDisp = false;
			currvessel = base.vessel;
			print ("GUI INIT'd");
			if (currvessel == null) {
				print ("GUI INIT NULL VESSEL");
			}

		
		}

		private void OnGUI(){

		}

		public override void OnUpdate(){
			isDisp = true;
			if (APInitd == false) {
				APInit (vessel);
				APInitd = true;
				RenderingManager.AddToPostDrawQueue (3, new Callback (GuiCallbackWrapper));
			}
		}

		public abstract void APGUI (int windowID);
		public abstract void GuiCallbackWrapper ();

		//public abstract void APGUI ();
		public abstract void APInit (Vessel currentvessel);


	}
}

