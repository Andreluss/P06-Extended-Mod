using System;
using TMPro;
using UnityEngine;

public class XModMenu : Singleton<XModMenu>
{
	private void Awake()
	{
		this.Menu = new XUIMenu("P-06X " + XDebug.P06X_VERSION, KeyCode.F12);
		this.Canvas = this.Menu.gameObject.transform.parent.GetComponent<Canvas>();
		if (XDebug.DBG)
		{
			XUISection xuisection = this.Menu.AddSection(new XUISection("Debug"));
			for (int i = 0; i < XDebug.Instance.dbg_toggles.Length; i++)
			{
				xuisection.AddItem(new XUIToggleButton("dbg_toggle_" + i.ToString(), XDebug.Instance.dbg_toggles[i]));
			}
			for (int j = 0; j < XDebug.Instance.dbg_floats.Length; j++)
			{
				xuisection.AddItem(new XUIFloatAdjuster("dbg_slider_" + j.ToString(), XDebug.Instance.dbg_floats[j], -0.1f, 0.5f, 3));
			}
		}
		XUISection xuisection2 = this.Menu.AddSection(new XUISection("Quick Settings"));
		xuisection2.AddItem(new XUIToggleButton("Ultra Smooth FPS", XDebug.Instance.UltraSmoothFPS));
		xuisection2.AddItem(new XUIToggleButton("Custom Music", XDebug.Instance.PlayCustomMusic));
		xuisection2.AddItem(new XUIFloatAdjuster("Global Speed Multiplier", XDebug.Instance.EverySpeedMultiplier, -0.1f, 0.2f, 3));
		xuisection2.AddItem(new XUIToggleButton("Free Water Sliding", XDebug.Instance.Moveset_FreeWaterSliding));
		xuisection2.AddItem(new XUIToggleButton("Wall Jump", XDebug.Instance.Moveset_WallJumping));
		xuisection2.AddItem(new XUIToggleButton("Climb All Walls", XDebug.Instance.Moveset_ClimbAll));
		xuisection2.AddItem(new XUIToggleButton("Boost", XDebug.Instance.Moveset_Boost));
		xuisection2.AddItem(new XUIToggleButton("Speedometer", XDebug.Instance.Extra_DisplaySpeedo));
		if (XDebug.COMMENT)
		{
			xuisection2.AddItem(new XUIToggleButton("<color=#4a4a4a>A</color>fter <color=#4a4a4a>H</color>oming <color=#4a4a4a>M</color>ovement", XDebug.Instance.Moveset_AHMovement));
			xuisection2.AddItem(new XUIFloatAdjuster("<color=#4a4a4a>AHM</color> Max Speed", XDebug.Instance.Moveset_AHMovementMaxSpeed, -1f, 2f, 1, 0f, float.PositiveInfinity));
		}
		else
		{
			xuisection2.AddItem(new XUIToggleButton("After Homing Movement", XDebug.Instance.Moveset_AHMovement));
			xuisection2.AddItem(new XUIFloatAdjuster("AHM Max Speed", XDebug.Instance.Moveset_AHMovementMaxSpeed, -1f, 2f, 1, 0f, float.PositiveInfinity));
		}
		XUISection xuisection3 = this.Menu.AddSection(new XUISection("Cheats"));
		xuisection3.AddItem(new XUIToggleButton("Always Invincible", XDebug.Instance.Invincible));
		xuisection3.AddItem(new XUIToggleButton("Infinite Gauge", XDebug.Instance.InfiniteGauge));
		xuisection3.AddItem(new XUIToggleButton("Maxed Out Gems", XDebug.Instance.MaxedOutGems));
		xuisection3.AddItem(new XUIFunctionButton("Get All Gems", delegate()
		{
			XDebug.Cheats.GetAllGems();
		}));
		xuisection3.AddItem(new XUIToggleButton("Infinite Rings", XDebug.Instance.InfiniteRings));
		xuisection3.AddItem(new XUIToggleButton("Infinite Lives", XDebug.Instance.InfiniteLives));
		xuisection3.AddItem(new XUIStringInput("Teleport location", XDebug.Instance.TeleportLocation, null, TMP_InputField.CharacterValidation.None));
		xuisection3.AddItem(new XUIFunctionButton("TELEPORT", delegate()
		{
			XDebug.Instance.TeleportToSection(XDebug.Instance.TeleportLocation.Value);
		}));
		xuisection3.AddItem(new XUIToggleButton("Water immunity", XDebug.Instance.Cheat_IgnoreWaterDeath));
		xuisection3.AddItem(new XUIToggleButton("Faster Chain Jump", XDebug.Instance.Cheat_ChainJumpZeroDelay));
		XUISection xuisection4 = this.Menu.AddSection(new XUISection("Advanced Speed Control"));
		xuisection4.AddItem(new XUIFloatAdjuster("Ground", XDebug.Instance.SMGround, -0.05f, 0.1f, 3));
		xuisection4.AddItem(new XUIFloatAdjuster("Air", XDebug.Instance.SMAir, -0.05f, 0.1f, 3));
		xuisection4.AddItem(new XUIFloatAdjuster("Spindash", XDebug.Instance.SMSpindash, -0.05f, 0.1f, 3));
		xuisection4.AddItem(new XUIFloatAdjuster("Flying", XDebug.Instance.SMFly, -0.05f, 0.1f, 3));
		xuisection4.AddItem(new XUIFloatAdjuster("Climbing", XDebug.Instance.SMClimb, -0.05f, 0.1f, 3));
		xuisection4.AddItem(new XUIFloatAdjuster("Homing", XDebug.Instance.SMHoming, -0.05f, 0.1f, 3));
		xuisection4.AddItem(new XUIFloatAdjuster("H. Attack", XDebug.Instance.SMHomingAttackFasterBy, -0.05f, 0.1f, 3));
		xuisection4.AddItem(new XUIFloatAdjuster("After H. Rotation", XDebug.Instance.SMAfterHomingRotation, -0.05f, 0.1f, 3));
		XUISection xuisection5 = this.Menu.AddSection(new XUISection("Boost"));
		xuisection5.AddItem(new XUIFloatAdjuster("Base Speed", XDebug.Instance.Boost_BaseSpeed, -10f, 10f, 0, 0f, float.PositiveInfinity));
		xuisection5.AddItem(new XUIFloatAdjuster("Delta Speed", XDebug.Instance.Boost_NextLevelDeltaSpeed, -10f, 10f, 0, 0f, float.PositiveInfinity));
		xuisection5.AddItem(new XUIFloatAdjuster("Turn speed", XDebug.Instance.Boost_RotSpeed, -1f, 1f, 1, 0f, float.PositiveInfinity));
		xuisection5.AddItem(new XUIFloatAdjuster("Acceleration time", XDebug.Instance.Boost_AccelTime, -0.1f, 0.25f, 2, 0.01f, float.PositiveInfinity));
		xuisection5.AddItem(new XUIFloatAdjuster("Level Up Threshold", XDebug.Instance.Boost_NextLevelThreshold, -2f, 3f, 1, 0f, float.PositiveInfinity));
		this.Menu.AddSection(new XUISection("Other")).AddItem(new XUIToggleButton("Old Camera Controls", XDebug.Instance.Other_OgCameraControls)).AddItem(new XUIToggleButton("FDT Fix (Beta)", XDebug.Instance.Other_UltraFPSFix))
			.AddItem(new XUIToggleButton("Version check", XDebug.Instance.Other_CheckP06Version));
		XUISection xuisection6 = this.Menu.AddSection(new XUISection("Saving"));
		xuisection6.AddItem(new XUIFunctionButton("Save Settings", delegate()
		{
			XDebug.Instance.SaveSettings();
		}));
		xuisection6.AddItem(new XUIFunctionButton("Load Settings", delegate()
		{
			XDebug.Instance.LoadSettings();
		}));
		xuisection6.AddItem(new XUIToggleButton("Load automatically", XDebug.Instance.Saving_AutoLoad));
		xuisection6.AddItem(new XUIToggleButton("Save automatically", XDebug.Instance.Saving_AutoSave));
	}

	private void Update()
	{
	}

	public Canvas Canvas;

	public XUIMenu Menu;
}
