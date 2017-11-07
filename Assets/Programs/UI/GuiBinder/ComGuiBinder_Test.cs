using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using cs;

public class ComGuiBinder_Test : ComGuiBinderBase , IGuiBinder
{
	public GuiControl guicOpen;
	public GuiControl guicClose;
	public GuiControl guicIcon;


	public void Bind(GameObject a_objRoot)
	{
		 guicOpen = Utility.FindCompoent<GuiControl>(a_objRoot, "Open");
		 guicClose = Utility.FindCompoent<GuiControl>(a_objRoot, "Close");
		 guicIcon = Utility.FindCompoent<GuiControl>(a_objRoot, "Icon");

	}
}