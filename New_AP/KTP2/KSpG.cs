using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using UnityEngine;
using KSP.IO;
//using Tac;

//Why the hell do I still have this class? it's more useless than Jeremy Clarkson withOUT a hammer.

namespace KSpG
{
	public class KSpG : PartModule
	{

		public static Vessel ThisGoddamnVessel;

		public enum axis
		{
			roll,
			pitch,
			yaw,
			mainThrottle
		}


		private void OnGUI()
		{
			ThisGoddamnVessel = vessel;
		}

	}
}

