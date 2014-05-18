using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using UnityEngine;
using KSP;
using KSP.IO;
//using ferram4;


namespace KSpG
{
	public class AHRS //: KSpG
	{
		private Vector3d headingvec, normvec, position, rollvec, eastUnit, upUnit, northUnit, leftunit, rotrate, rolrate;
		public float hdg, ptch, roll, hdgRate, ptchRate, rollRate, slipRate;
		private float northcomponent, eastcomponent, upcomponent, rollcomponentup, rollcomponentleft;//, northcomponentrate, eastcomponentrate, upcomponentrate, rollcomponentuprate, rollcomponentleftrate;
		private float lasthdg, lastptch, lastroll, lastRadAlt, lastBaroAlt, lastslip;
		private Vessel ThisVessel;
		public float RadAlt, BaroAlt, TerrAlt, RadVS, BaroVS, density, reldensity;

		private Vector3d spdvec, slipvec;
		public float Tas, Ias, sideslip, aoa;
		//private ThisVessel ThisVessel;

		private Assembly thisAeroUtilAssembly;

		public AHRS(Vessel ThisVessel){
			this.ThisVessel = ThisVessel;

			//print ("AHRS INIT");
			/*
			print ("LOAD FERRAM");//http://www.csharp-examples.net/reflection-examples/
			thisAeroUtilAssembly = Assembly.LoadFile ("./FerramAerospaceResearch.dll");
			*/

		}

		public void updateAHRS(){//ThisVessel ThisVessel){//working like a charm
			//ThisVessel = thisThisVessel;
			KSpG.ThisGoddamnVessel = ThisVessel;
			if (KSpG.ThisGoddamnVessel == null) //{print ("WAT? Nullvessel");return;}
			//print (ThisVessel.ToString());
			headingvec = (Vector3d)ThisVessel.transform.up;//point forward
			normvec = (Vector3d)ThisVessel.transform.forward;//point down
			position = ThisVessel.findWorldCenterOfMass();
			rollvec = Vector3d.Cross (normvec, headingvec);//point to left
			spdvec = (Vector3d)ThisVessel.GetSrfVelocity ();

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


			slipvec = Vector3d.Normalize (spdvec) - headingvec;
			sideslip=(float)( (180/Math.PI)*Math.Asin(Vector3d.Dot(slipvec,rollvec)));
			aoa=(float)( (180/Math.PI)*Math.Asin(Vector3d.Dot(slipvec,normvec)));
			//--------------------------------------------------------------------
			/*northcomponentrate = (float)Vector3d.Dot (rotrate, northUnit);
			eastcomponentrate = (float)Vector3d.Dot (rotrate, eastUnit);
			upcomponentrate = (float)Vector3d.Dot (rotrate, upUnit);

			rollcomponentuprate = (float)Vector3d.Dot (rolrate, upUnit);
			rollcomponentleftrate = (float)Vector3d.Dot (rolrate, leftunit);*/
			if (TimeWarp.deltaTime > 0.0001f) {
				lasthdg = hdg;
				lastptch = ptch;
				lastroll = roll;
				lastRadAlt = RadAlt;
				lastBaroAlt = BaroAlt;
				lastslip = sideslip;
			}
			//---------------------------------------------------------------------------------------------------------------
			hdg = (float)(Math.Atan2(eastcomponent,northcomponent)*(180/Math.PI));// atan return rad!
			ptch = (float)(Math.Atan2(upcomponent, Math.Sqrt (1 - upcomponent * upcomponent))*(180/Math.PI));
			roll = (float)(Math.Atan2(rollcomponentup,rollcomponentleft)*(180/Math.PI));// atan return rad!                                             

			//Thanks, stupid_chris over at forum.kerbalspaceprogram.com
			BaroAlt = (float)FlightGlobals.getAltitudeAtPos (position);
			TerrAlt = (float)ThisVessel.pqsAltitude;
			if (ThisVessel.mainBody.ocean && TerrAlt < 0) { TerrAlt = 0; }
			RadAlt = BaroAlt - TerrAlt;

			//---------------------------------------------------------------------------------------------------------------
			if (TimeWarp.deltaTime > 0.0001f) {
				RadVS = (RadAlt - lastRadAlt) / TimeWarp.deltaTime; 
				BaroVS = (BaroAlt - lastBaroAlt) / TimeWarp.deltaTime;
				hdgRate = (hdg - lasthdg) / TimeWarp.deltaTime; 
				ptchRate = (ptch - lastptch) / TimeWarp.deltaTime;
				rollRate = (roll - lastroll) / TimeWarp.deltaTime;
				slipRate = (sideslip - lastslip) / TimeWarp.deltaTime;
			}

			Tas = (float) Vector3d.Dot (spdvec, Vector3d.Normalize (headingvec));

			//density = ferram4.FARAeroUtil.GetCurrentDensity(FlightGlobals.currentMainBody, BaroAlt); // TODO: Wrap my mind around reflection
			reldensity = getRelDensity (FlightGlobals.getMainBody(),BaroAlt);

			Ias = Tas * Mathf.Sqrt (reldensity);
			reldensity = density / 1.013f;



			}

		public static float GetCurrentDensity(CelestialBody body, float altitude) //Yanked from FAR..  Thank you, ferram4 over at http://forum.kerbalspaceprogram.com/  Also, FAR is release under CC-BY-SA but the legal stuff confused me :(
		{//-----------------------------------------TODO: Ask for permission (or forgiveness:) )
			//UpdateCurrentActiveBody(body);
			float density = 0;
			if (altitude > body.maxAtmosphereAltitude)
				return 0;
			// No Jool correction because why would you use this autopilot on Jool?
			float temp = FlightGlobals.getExternalTemperature(altitude, body);
			temp += 273.15f;
			float pressure = (float)FlightGlobals.getStaticPressure(altitude, body) * 101300f;     //Need to convert atm to Pa
			density = (pressure * getMolecularWeight (body)) / (8.3145f * 1000 * temp);

			return density;
		}

		public float getRelDensity(CelestialBody body,float altitude){
			return (GetCurrentDensity(body,altitude)/GetCurrentDensity(body,0f));
		}

		public static float getMolecularWeight(CelestialBody body){
			if (body.flightGlobalsIndex <= 4 ||
				body.flightGlobalsIndex == 7 ||
				body.flightGlobalsIndex >= 10) {
				return 28.96f;
			} else if (body.flightGlobalsIndex == 5 || body.flightGlobalsIndex == 6) {
				return 43.2102f;
			} else if (body.flightGlobalsIndex == 8) {
				return 2.21466f;
			} else if (body.flightGlobalsIndex == 9) {
				return 47.0465f;
			} else {
				return 28.96f;
			}
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

