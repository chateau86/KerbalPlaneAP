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
		private Vector3d headingvec, normvec, position, rollvec, eastUnit, upUnit, northUnit, leftunit;
		public float hdg, ptch, roll;
		private float northcomponent, eastcomponent, upcomponent, rollcomponentup, rollcomponentleft;

		public AHRS(){

		}

		public void updateAHRS(Vessel vessel){//working like a charm
			//vessel = thisvessel;
			if (vessel == null) {print ("WAT? NullVessel");return;}
			print (vessel.ToString());
			headingvec = (Vector3d)vessel.transform.up;//point forward
			normvec = (Vector3d)vessel.transform.forward;//point down
			position = vessel.findWorldCenterOfMass();
			rollvec = Vector3d.Cross (normvec, headingvec);//point to left
			//unit vectors in the up (normal to planet surface), east, and north (parallel to planet surface) directions
			eastUnit = vessel.mainBody.getRFrmVel(position).normalized; //uses the rotation of the body's frame to determine "east"
			upUnit = (position - vessel.mainBody.position).normalized;
			northUnit = Vector3d.Cross(upUnit, eastUnit); //north = up cross east

			leftunit = Vector3d.Cross (headingvec, upUnit);
			northcomponent = (float)Vector3d.Dot (headingvec, northUnit);
			eastcomponent = (float)Vector3d.Dot (headingvec, eastUnit);
			upcomponent = (float)Vector3d.Dot (headingvec, upUnit);

			rollcomponentup = (float)Vector3d.Dot (rollvec, upUnit);
			rollcomponentleft = (float)Vector3d.Dot (rollvec, leftunit);

			hdg = (float)(Math.Atan2(eastcomponent,northcomponent)*(180/Math.PI));// atan return rad!
			ptch = (float)(Math.Atan2(upcomponent, Math.Sqrt (1 - upcomponent * upcomponent))*(180/Math.PI));
			roll = (float)(Math.Atan2(rollcomponentup,rollcomponentleft)*(180/Math.PI));// atan return rad!                                             


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
			case 5:textAreaString = "HDG:" + hdg + "PTCH:" + ptch + "ROLL:" + roll;break;
			case 6:textAreaString = "Normal:"+((Vector3)normvec).ToString();break;
			case 7:textAreaString = "PRESS MODE TO RESET";break;
			default:
				textAreaString = "";
				break;	
			}

			return textAreaString;
		}
	}
}

