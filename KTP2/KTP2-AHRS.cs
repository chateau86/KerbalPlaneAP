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
	public class AHRS : KTP2
	{
		private Vector3d headingvec, normvec, position, rollvec, eastUnit, upUnit, northUnit, leftunit, rotrate, rolrate;
		public float hdg, ptch, roll, hdgRate, ptchRate, rollRate;
		private float northcomponent, eastcomponent, upcomponent, rollcomponentup, rollcomponentleft;//, northcomponentrate, eastcomponentrate, upcomponentrate, rollcomponentuprate, rollcomponentleftrate;
		private float lasthdg, lastptch, lastroll, lastRadAlt, lastBaroAlt;
		private Vessel ThisVessel;
		public float RadAlt, BaroAlt, RadVS, BaroVS;
		//private ThisVessel ThisVessel;

		public AHRS(Vessel ThisVessel){
			this.ThisVessel = ThisVessel;
		}

		public void updateAHRS(){//ThisVessel ThisVessel){//working like a charm
			//ThisVessel = thisThisVessel;
			KTP2.ThisGoddamnVessel = ThisVessel;
			if (KTP2.ThisGoddamnVessel == null) {print ("WAT? Nullvessel");return;}
			//print (ThisVessel.ToString());
			headingvec = (Vector3d)ThisVessel.transform.up;//point forward
			normvec = (Vector3d)ThisVessel.transform.forward;//point down
			position = ThisVessel.findWorldCenterOfMass();
			rollvec = Vector3d.Cross (normvec, headingvec);//point to left

			rotrate = ThisVessel.angularVelocity;
			rolrate = Vector3d.Cross (rotrate, headingvec);
			//unit vectors in the up (normal to planet surface), east, and north (parallel to planet surface) directions
			eastUnit = ThisVessel.mainBody.getRFrmVel(position).normalized; //uses the rotation of the body's frame to determine "east"
			upUnit = (position - ThisVessel.mainBody.position).normalized;
			northUnit = Vector3d.Cross(upUnit, eastUnit); //north = up cross east

			leftunit = Vector3d.Cross (headingvec, upUnit);
			northcomponent = (float)Vector3d.Dot (headingvec, northUnit);
			eastcomponent = (float)Vector3d.Dot (headingvec, eastUnit);
			upcomponent = (float)Vector3d.Dot (headingvec, upUnit);

			rollcomponentup = (float)Vector3d.Dot (rollvec, upUnit);
			rollcomponentleft = (float)Vector3d.Dot (rollvec, leftunit);
			//--------------------------------------------------------------------
			/*northcomponentrate = (float)Vector3d.Dot (rotrate, northUnit);
			eastcomponentrate = (float)Vector3d.Dot (rotrate, eastUnit);
			upcomponentrate = (float)Vector3d.Dot (rotrate, upUnit);

			rollcomponentuprate = (float)Vector3d.Dot (rolrate, upUnit);
			rollcomponentleftrate = (float)Vector3d.Dot (rolrate, leftunit);*/
			lasthdg = hdg;
			lastptch = ptch;
			lastroll = roll;

			hdg = (float)(Math.Atan2(eastcomponent,northcomponent)*(180/Math.PI));// atan return rad!
			ptch = (float)(Math.Atan2(upcomponent, Math.Sqrt (1 - upcomponent * upcomponent))*(180/Math.PI));
			roll = (float)(Math.Atan2(rollcomponentup,rollcomponentleft)*(180/Math.PI));// atan return rad!                                             

			/*hdgRate = (float)(Vector3d.Dot (rotrate, upUnit));
			ptchRate=(float)(Vector3d.Dot(rotrate, leftunit));
			rollRate = (float)(Vector3d.Dot (rotrate, headingvec));*/
			hdgRate = (hdg - lasthdg) / TimeWarp.deltaTime; 
			ptchRate = (ptch - lastptch) / TimeWarp.deltaTime;
			rollRate = (roll - lastroll) / TimeWarp.deltaTime;

			//----------------------Now for ADC
			lastRadAlt = RadAlt;
			lastBaroAlt = BaroAlt;

			RadAlt = ThisGoddamnVessel.GetHeightFromTerrain ();
			BaroAlt = ThisVessel.GetHeightFromSurface ();
			//BaroAlt=(float)ThisGoddamnVessel.mainBody.GetAltitude (ThisGoddamnVessel.CoM);
			//CoM = this.vessel.findWolrdCenterOfMass();
			//BaroAlt =(float) FlightGlobals.getAltitudeAtPos(this.vessel.findWorldCenterOfMass());
			RadVS=(lastRadAlt-RadAlt)/TimeWarp.deltaTime; 
			BaroVS = (lastBaroAlt - BaroAlt) / TimeWarp.deltaTime;
			BaroVS = (float)ThisGoddamnVessel.verticalSpeed;

			}
		public string debugAHRS(int dispmode){
			string textAreaString;
			switch (dispmode) {
			case 0:textAreaString = "Heading:" + ((Vector3)headingvec).ToString ();break;
			case 1:textAreaString = "pos:"+((Vector3)position).ToString();break;
				//case 2:textAreaString = "UpAx:"+((Vector3)upaxis).ToString();break;
			case 2:textAreaString = "UP:"+((Vector3)upUnit).ToString();break;
			case 3:textAreaString = "N:"+((Vector3)northUnit).ToString();break;
			case 4:textAreaString = "E:"+((Vector3)eastUnit).ToString();break;
			case 5:textAreaString = "HDG:" + hdg.ToString("000.00") + 
				                    "PTCH:" + ptch.ToString("000.00") + 
				                    "ROLL:" + roll.ToString("000.00")+
				                    "HR:"+hdgRate.ToString("0.00")+
				                    "PR:"+ptchRate.ToString("0.00")+
				                    "RR:"+rollRate.ToString("0.00");break;
			case 6:textAreaString = "Normal:"+((Vector3)normvec).ToString();break;
			case 7:textAreaString = "Angular:"+rollRate.ToString();break;
			default:
				textAreaString = "";
				break;	
			}

			return textAreaString;
		}
	}
}

