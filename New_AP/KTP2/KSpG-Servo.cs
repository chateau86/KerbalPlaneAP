using System;

namespace KSpG
{
	public class KSpG_Servo
	{
		public double[] TargetAtt;//{Roll,Ptch,Yaw}

		public KSpG_Servo ()
		{
			TargetAtt= new double[3];
		}
			



		public void SetRoll(double roll){
			TargetAtt[0]=roll;
		}
		public void SetPitch(double pitch){
			TargetAtt[1]=pitch;
		}
		public void SetYaw(double yaw){
			TargetAtt[2]=yaw;
		}


		public void fly(){

		}

	}
}

