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
			off = 0 ,
			RollLever,
			PtchLever,
			AltHold
		}
		public float[] PIDconst; //= { 1f, 0.2f, 0.05f }; // {0roll, 1pitch, 2yaw}:{{kForce, kDamp, kTrim}}

		float trimval;
		float tgt;
		float gain=0.5f;
		float PtchUpLim=45f;
		float PtchDownLim = -30f;
		float ctrlforce,lastCtrlforce;

		float tgtAxisHold;
		public mode currmode;
		public axis curraxis;
		//APServo thisServo;
		AHRS thisAHRS;
		private FlightInputCallback currfconcallback;

		public APBrain (mode newmode, Vessel vessel)
		{
			if (vessel == null) {
				print ("AP INIT NULL VESSEL");
			}

			print ("AP INIT AS "+ newmode);
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

		private void getPIDconst(APBrain.mode mode, out float[] PIDconst){
			switch(mode)//:{gain, kCube, kForce, kDamp, kTrim, CtrlRate/sec}
			{
			case mode.off		:PIDconst=new float[]{	1f,		1f,			1f,			0.2f, 		0.05f,	0.5f};break;
				//Pitch---------------------------------------------------
			case mode.AltHold	:PIDconst=new float[]{	1f,		0.1f,		1f,			1f, 		0.5f,	0.5f};break;
			case mode.PtchLever	:PIDconst=new float[]{	1f,		0.1f,		0.3f,		1f, 		0.1f,	0.5f};break;
				//Roll---------------------------------------------------
			case mode.RollLever	:PIDconst=new float[]{	1f,		0.5f,		0.3f,		1f, 		0.05f,	0.5f};break;

			default: 			 PIDconst=new float[]{	1f,		1f, 		1f, 		0.2f, 		0.05f,	0.5f};	break;
			}
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
				ctrlforce.roll = PIDctrl (tgt, thisAHRS.roll, thisAHRS.rollRate,currmode);
			}
			if (currmode == mode.PtchLever) {
				ctrlforce.pitch = PIDctrl (tgt, thisAHRS.ptch, thisAHRS.ptchRate,currmode);
			}

			if (currmode == mode.AltHold) {
				tgtAxisHold = PIDctrl (tgt, thisAHRS.BaroAlt, thisAHRS.BaroVS, currmode);
				tgtAxisHold = Mathf.Clamp (tgt, PtchDownLim, PtchUpLim);
				ctrlforce.pitch = PIDctrl ( tgtAxisHold, thisAHRS.ptch, thisAHRS.ptchRate,currmode);
				print ("AltHold at:"+tgt.ToString("0.00")+" displ:"+(thisAHRS.BaroAlt-tgt).ToString("0.00")+" TargetPtc:"+tgtAxisHold.ToString("0.00")+" Force:"+ctrlforce.pitch.ToString("0.00"));
			}
			//---TODO ALT
			/*print ("RECALCULATING"+curraxis
				+"FR"+ctrlforce.roll.ToString("0.00")
				+"Dsp:"+(thisAHRS.roll-tgt).ToString("0.00")
				+"Roll:"+thisAHRS.roll.ToString("0.00")
				+"FP"+ctrlforce.pitch.ToString("0.00")
				+"Dsp:"+(thisAHRS.ptch-tgt).ToString("0.00")
				+"Ptch:"+thisAHRS.ptch.ToString("0.00"));*/
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
			if (currmode == APBrain.mode.AltHold && tgt == 0) {
				tgt = thisAHRS.BaroAlt;
			}
		}

		public void setgain(float newgain){
			gain = newgain;
		}

		public void disconnect(){
			/*vessel.OnFlyByWire -= currfconcallback;
			currmode = APBrain.mode.off;
			currfconcallback = null;*/
			//this = null;//self destruct: AP Disc when sh*t hit the fan, no need to keep trim val
		}

		private float PIDctrl(float tgt, float value, float rate, mode mode){
			//float ctrlforce;
			float displ = value - tgt;
			lastCtrlforce = ctrlforce;
			getPIDconst (mode,out PIDconst);

			ctrlforce = -PIDconst[0] * ( PIDconst[1]*(displ*displ*displ)+ PIDconst [2] * displ + PIDconst [3] * rate) + trimval;
			//ctrlforce= Mathf.Clamp (ctrlforce, -1, +1);
			if (Mathf.Abs (ctrlforce) > Mathf.Abs (lastCtrlforce) && Mathf.Abs (ctrlforce) > 0.05f) {
				ctrlforce = Mathf.Clamp (ctrlforce, (lastCtrlforce - (PIDconst [4] * TimeWarp.deltaTime)), (lastCtrlforce + (PIDconst [4] * TimeWarp.deltaTime)));
			} else {
				ctrlforce = Mathf.Clamp (ctrlforce, (lastCtrlforce - (PIDconst [4] * TimeWarp.deltaTime*2)), (lastCtrlforce + (PIDconst [4] * TimeWarp.deltaTime*2)));
			}
			trimval += PIDconst [3] * (ctrlforce-trimval);



			return ctrlforce;

		}


	}
}

