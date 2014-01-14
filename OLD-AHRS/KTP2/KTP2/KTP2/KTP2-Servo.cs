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
	public class APServo:KTP2
	{
		private FlightCtrlState ctrlin;
		private bool ModeTrim;
		public APServo ()
		{

		}

		public void ServoInit(bool TrimOnly){
			ModeTrim = TrimOnly;
			//hook ctrlout to fcon
			if (ModeTrim) {
				vessel.OnFlyByWire +=new FlightInputCallback(ServoAct);
			} else {
				vessel.OnFlyByWire = new FlightInputCallback(ServoAct);
			}
		}

		public void ServoDisconnect(){//ALWAYS destroy/create servo on AP engage/disengage
			//unhook
			vessel.OnFlyByWire -=new FlightInputCallback(ServoAct);
		}
		public void ServoAct(FlightCtrlState ctrlout){

			//Sanity check
			ctrlout.roll = Mathf.Clamp(ctrlout.roll, -1.0F, +1.0F);
			ctrlout.pitch = Mathf.Clamp(ctrlout.pitch, -1.0F, +1.0F);
			ctrlout.yaw = Mathf.Clamp(ctrlout.yaw, -1.0F, +1.0F);
			ctrlout.mainThrottle = Mathf.Clamp(ctrlout.mainThrottle, 0F, +1.0F);
		}
	}
}

