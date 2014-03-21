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
			HDGHold,

			PtchLever,
			AltHold,
			AltSel,
			VSHold,

			NoSlip,

			clmp,
			SpdHold
		}
		public float[] PIDconst; //= { 1f, 0.2f, 0.05f }; // {0roll, 1pitch, 2yaw}:{{kForce, kDamp, kTrim}}
		public float[] NPIDconst;
		float trimval;
		//float tgt;
		float[] tgt = new float[(int)mode.SpdHold+1];
		float tgtarm;
		float gain=0.5f;
		//float PtchUpLim=+45f;
		//float PtchDownLim = -30f;
		float ctrlforce,lastCtrlforce;

		float tgtAxisHold;
		float tgtAxisTrim;
		float timeconstant=20f;
		public mode currmode;
		public mode armmode;
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
			armmode = mode.off;
			currmode = newmode;
			if (currmode == mode.RollLever) {
				curraxis = axis.roll;
			}
			if (currmode == mode.PtchLever || currmode == mode.AltHold || currmode==mode.VSHold||currmode==mode.AltSel) {
				curraxis = axis.pitch;
			}
			if (currmode == mode.SpdHold) {
				curraxis = axis.mainThrottle;
			}

			thisAHRS = new AHRS (vessel);

			currfconcallback= new FlightInputCallback(fly);
			vessel.OnFlyByWire += currfconcallback;
		}

		private void getPIDconst(APBrain.mode mode, out float[] PIDconst){
			switch(mode)//:{gain, kCube, kForce, kDamp, kTrim, CtrlRate/sec}
			{
			case mode.off		:PIDconst=new float[]{	1f,		1f,			1f,			0.2f, 		0.05f,	0.5f};break;
				//Roll---------------------------------------------------
				//case mode.RollLever	:PIDconst=new float[]{	1f,		0.5f,		0.3f,		1f, 		0.05f,	0.5f};break;
			case mode.RollLever	:PIDconst=new float[]{	1f,		0.1f,		0.3f,		1f, 		0.01f,	0.5f};break;
			case mode.HDGHold	:PIDconst=new float[]{	-0.5f,		0.1f,		0.3f,		1f, 		0.01f,	0.5f};break;
				//Pitch---------------------------------------------------
			case mode.PtchLever	:PIDconst=new float[]{	1f,		0.1f,		0.3f,		1f, 		0.1f,	0.5f};break;
			case mode.AltHold	:PIDconst=new float[]{	1f,		0f,			0.1f,		1f, 		0.1f,	0.1f};break;
			case mode.AltSel	:PIDconst=new float[]{	1f,		0f,			0.1f,		1f, 		0.1f,	0.1f};break;
			case mode.VSHold	:PIDconst=new float[]{	0.1f,		0.001f,		0.2f,		1f, 		0.01f,	0.5f};break;
				//Yaw---------------------------------------------------
				//Spd---------------------------------------------------
			case mode.SpdHold	:PIDconst=new float[]{	0.01f,		0.1f,		0.3f,		1f, 		0.01f,	0.05f};break;
			default: 			 PIDconst=new float[]{	1f,		1f, 		1f, 		0.2f, 		0.05f,	0.5f};	break;
			}
		}
		private void getNPIDconst(APBrain.mode mode, out float[] NPIDconst){
			switch(mode)//:{gain, kCube, kForce, min, max}
			{
			case mode.off		:NPIDconst=new float[]{	0f,		0f,		0f,		0f, 	0f	};break;
				//Pitch---------------------------------------------------
			case mode.AltHold	:NPIDconst=new float[]{	1f,		0f,		0.1f,		-30f, 	+45f	};break;
			case mode.AltSel	:NPIDconst=new float[]{	10f,		0f,		0.1f,		-30f, 	+45f	};break;
			case mode.PtchLever	:NPIDconst=new float[]{	0f,		0f,		0f,		0f, 	0f	};break;
				//Roll---------------------------------------------------
				//case mode.RollLever	:PIDconst=new float[]{	1f,		0.5f,		0.3f,		1f, 		0.05f,	0.5f};break;
			case mode.RollLever	:NPIDconst=new float[]{	0f,		0f,		0f,		0f, 	0f	};break;
			default: 			 NPIDconst=new float[]{	0f,		0f, 	0f, 	0f, 	0f	};	break;
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

				//Roll---------------------------------------------------
			if (currmode == mode.RollLever) {
				ctrlforce.roll = PIDctrl (tgt [(int)mode.RollLever], thisAHRS.roll, thisAHRS.rollRate, currmode);
			
				//Ptch---------------------------------------------------
			} else if (currmode == mode.PtchLever) {
				ctrlforce.pitch = PIDctrl (tgt [(int)mode.PtchLever], thisAHRS.ptch, thisAHRS.ptchRate, currmode);
			} else if (currmode == mode.AltHold) {//Should be replaced with VS based hold
				tgtAxisHold = NPIDctrl (tgt [(int)mode.AltHold], tgtAxisTrim, thisAHRS.BaroAlt, currmode);
				ctrlforce.pitch = PIDctrl (tgtAxisHold, thisAHRS.ptch, thisAHRS.ptchRate, mode.PtchLever);
			}else if (currmode==mode.AltSel){
				tgtAxisHold = NPIDctrl (tgt [(int)mode.AltSel], 0, thisAHRS.BaroAlt, currmode);
				ctrlforce.pitch = PIDctrl (tgtAxisHold, thisAHRS.BaroVS, 0f, currmode);//TODO:Add VS Rate
			} else if (currmode == mode.VSHold) {
				ctrlforce.pitch = PIDctrl (tgt [(int)mode.VSHold], thisAHRS.BaroVS, 0f, currmode);//TODO:Add VS Rate
				if(armmode==mode.AltSel){
					if(thisAHRS.BaroVS>0&&((tgt [(int)mode.AltSel]-thisAHRS.BaroAlt)<(tgt [(int)mode.VSHold]*timeconstant))){
						this.ActivateArm();
					}else if(thisAHRS.BaroVS<0&&((thisAHRS.BaroAlt-tgt [(int)mode.AltSel])<(tgt [(int)mode.VSHold]*timeconstant))){
						this.ActivateArm();
					}
				}
				//Yaw---------------------------------------------------
			}else if(currmode==mode.NoSlip){
				ctrlforce.yaw = PIDctrl (0f, thisAHRS.sideslip, thisAHRS.slipRate, currmode);
				//Spd---------------------------------------------------
			} else if (currmode == mode.clmp) {
				ctrlforce.mainThrottle = tgt [(int)currmode];
			} else if (currmode == mode.SpdHold) {
				//TODO
				ctrlforce.mainThrottle = PIDctrl (tgt [(int)mode.SpdHold], thisAHRS.Ias, 0f, currmode);//TODO:Add IAS rate
			}

			oldstate.roll += ctrlforce.roll;
			oldstate.pitch += ctrlforce.pitch;
			oldstate.yaw += ctrlforce.yaw;
			oldstate.mainThrottle += ctrlforce.mainThrottle;

		}


		public float setTgt(mode tgtmode,float newtgt){
			tgt[(int)tgtmode] = newtgt;
			return tgt[(int)tgtmode];
		}
		public float setTgtTrim(float newtrim){
			tgtAxisTrim = newtrim;
			return newtrim;
		}

		public float chgTgt(mode tgtmode,float delta){
			tgt[(int)tgtmode]+= delta;
			if (tgtmode == mode.RollLever || tgtmode == mode.PtchLever) {
				tgt [(int)tgtmode] = Mathf.Clamp (tgt[(int)tgtmode], -90f, +90f);
			}else
				if (tgtmode == mode.HDGHold) {
					if (tgt [(int)tgtmode] >= 360f) {
						tgt [(int)tgtmode] -= 360f;
					}else if (tgt [(int)tgtmode] < 0f) {
						tgt [(int)tgtmode] += 360;
					}
			}
			return tgt[(int)tgtmode];
		}

		public void setmode(APBrain.mode newmode){
			currmode = newmode;
			if (currmode == APBrain.mode.AltHold && tgt[(int)mode.AltHold] == 0) {
				tgt[(int)mode.AltHold] = thisAHRS.BaroAlt;
			}
		}

		public void setarmmode(APBrain.mode newmode){
			armmode = newmode;
			if (armmode == APBrain.mode.AltHold && tgt[(int)mode.AltHold] == 0) {
				tgt[(int)mode.AltHold] = thisAHRS.BaroAlt;
			}
		}

		public void setgain(float newgain){
			gain = newgain;
		}

		public void ActivateArm(){
			currmode = armmode;
			//tgt = tgtarm;
			armmode = APBrain.mode.off;
		}
		public string Statustxt (mode dispmode){
			if (dispmode == currmode) {
				return "ON";
			} else if (dispmode == armmode) {
				return "ARM";
			} else {
				return "OFF";
			}
		}


		public void disconnect(){

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

		private float NPIDctrl(float tgt, float tgttrim, float value, mode mode){// for constant force-displacement curve
			float displ = value - tgt;
			getNPIDconst (mode, out NPIDconst);
			tgtAxisHold = -NPIDconst [0] * (NPIDconst [1] * (displ * displ * displ) + NPIDconst [2] * displ)+tgttrim;

			tgtAxisHold = Mathf.Clamp (tgtAxisHold, NPIDconst [3], NPIDconst [4]);
			return tgtAxisHold;
		}


	}
}

