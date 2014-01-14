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
	public class GUIStuff:KTP2
	{
		string textAreaString="";
		public int dispmode=0;
		public GUIStuff ()
		{

			if ((windowPos.x == 0) && (windowPos.y == 0))//windowPos is used to position the GUI window, lets set it in the center of the screen
			{
				windowPos = new Rect(Screen.width / 2, Screen.height / 2, 200, 50);
			}
		}

		public void updateGUI(){
			windowPos = GUILayout.Window(1337, windowPos, WindowGUI, "Toggle", GUILayout.MinWidth(1000));

		}
		private void WindowGUI(int windowID)
		{

			GUIStyle mySty = new GUIStyle(GUI.skin.button); 
			mySty.normal.textColor = mySty.focused.textColor = Color.white;
			mySty.hover.textColor = mySty.active.textColor = Color.yellow;
			mySty.onNormal.textColor = mySty.onFocused.textColor = mySty.onHover.textColor = mySty.onActive.textColor = Color.green;
			mySty.padding = new RectOffset(8, 8, 8, 8);

			GUILayout.BeginVertical();
			if (GUILayout.Button("Toggle",mySty,GUILayout.ExpandWidth(false)))//GUILayout.Button is "true" when clicked
			{	
				dispmode++;
			}
			if (GUILayout.Button("Mode",mySty,GUILayout.ExpandWidth(false)))//GUILayout.Button is "true" when clicked
			{	
				//switch
				dispmode = 0;
			}

			GUI.TextArea (new Rect (30, 25, 300, 30), textAreaString);
			//GUILayout.Label (textAreaString);

			GUILayout.EndVertical();

			//DragWindow makes the window draggable. The Rect specifies which part of the window it can by dragged by, and is 
			//clipped to the actual boundary of the window. You can also pass no argument at all and then the window can by
			//dragged by any part of it. Make sure the DragWindow command is AFTER all your other GUI input stuff, or else
			//it may "cover up" your controls and make them stop responding to the mouse.
			GUI.DragWindow(new Rect(0, 0, 10000, 20));

		}

		public void SetTextAreaString(string newstr){
			textAreaString = newstr;
		}


	}
}

