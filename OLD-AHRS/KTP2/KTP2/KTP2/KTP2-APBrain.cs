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
	public class APBrain:KTP2
	{
		public enum mode {
			off,
			RollLever,
			PtchLever,
			AltHold
		}
		public float[] PIDconst = { 0.5f, 0.05f, 0.02f }; // {0roll, 1pitch, 2yaw}:{{kForce, kDamp, kTrim}}

		float trimval;
		float tgt;
		float gain=2f;
		public mode currmode;
		public axis curraxis;
		//APServo thisServo;
		AHRS thisAHRS;
		private FlightInputCallback currfconcallback;
		public APBrain (mode newmode, Vessel vessel)
		{
			print ("AP ON");
			//new all axis servo; only put correct force in output flightctrlstate
			currmode = newmode;
			if (currmode == mode.RollLever) {
				curraxis = axis.roll;
			}
			if (currmode == mode.PtchLever || currmode == mode.AltHold) {
				curraxis = axis.pitch;
			}
			thisAHRS = new AHRS (vessel);

			currfconcallback= new FlightInputCallback(fly);
			vessel.OnFlyByWire += currfconcallback;
		}

		public void fly(FlightCtrlState oldstate)
		{
			//print ("So fly like a "+oldstate);
			if (oldstate == null||currmode==APBrain.mode.off) {
				return;
			}
			FlightCtrlState ctrlforce=new FlightCtrlState();
			//print ("Put Yo' hands up");
			thisAHRS.updateAHRS ();//vessel);
			//print ("cheestick!");
			if (currmode == mode.RollLever) {
				ctrlforce.roll = PIDctrl (tgt, thisAHRS.roll, thisAHRS.rollRate);
			}
			if (currmode == mode.PtchLever) {
				ctrlforce.pitch = PIDctrl (tgt, thisAHRS.ptch, thisAHRS.ptchRate);
			}
			//---TODO ALT
			print ("RECALCULATING"+curraxis
				+"FR"+ctrlforce.roll.ToString("0.00")
				+"Dsp:"+(thisAHRS.roll-tgt).ToString("0.00")
				+"Roll:"+thisAHRS.roll.ToString("0.00"));
			oldstate.roll += ctrlforce.roll;
			oldstate.pitch += ctrlforce.pitch;
			oldstate.yaw += ctrlforce.yaw;

		}


		public float setTgt(float newtgt){
			tgt = newtgt;
			return tgt;
		}

		public float chgTgt(float delta){
			tgt+= delta;
			tgt = Mathf.Clamp (tgt, -90f, +90f);
			return tgt;
		}

		public void setmode(APBrain.mode newmode){
			currmode = newmode;
		}

		public void setgain(float newgain){
			gain = newgain;
		}

		public void disconnect(){
			vessel.OnFlyByWire -= currfconcallback;
			currmode = APBrain.mode.off;
			//this = null;//self destruct: AP Disc when sh*t hit the fan, no need to keep trim val
		}

		private float PIDctrl(float tgt, float value, float rate){
			float ctrlforce;
			float displ = value - tgt;
			ctrlforce = - gain*(PIDconst [0] * displ + PIDconst [1] * rate) + trimval;
			ctrlforce= Mathf.Clamp (ctrlforce, -1, +1);
			trimval += PIDconst [2] * ctrlforce;
			return ctrlforce;

		}


	}
}

