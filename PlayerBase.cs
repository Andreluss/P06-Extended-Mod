using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using STHEngine;
using STHLua;
using UnityEngine;

public partial class PlayerBase : MonoBehaviour
{
	private void X_StateWaterSlideStart()
	{
		this.SetState("WaterSlide");
		XDebug.Comment("it's using sonic's lua but ok");
		this.X_WSpeed = Mathf.Min(this.CurSpeed, Sonic_New_Lua.c_run_speed_max * 1.5f);
		this.X_WSTime = 0f;
		this.X_WSDirection = base.transform.forward;
		this.X_WSSpline = this.GetSpline(this.X_LaunchMode);
		this.X_WSPositionShift = 0f;
		this.MaxRayLenght = 0.55f;
	}

	private void X_StateWaterSlide()
	{
		XDebug.Comment(" this.PlayerState = SonicNew.State.WaterSlide; i guess that's unnecessary");
		this.PlayAnimation("Water Slide", "On Water Slide");
		this.LockControls = true;
		if (this.X_WSpeed > 0f)
		{
			this.X_WSpeed -= 7.5f * Time.fixedDeltaTime;
		}
		this.CurSpeed = this.X_WSpeed;
		float num = Vector3.Dot(base.transform.forward, this.Camera.transform.forward);
		this.X_WSSmoothPos = Mathf.Lerp(this.X_WSSmoothPos, -Singleton<RInput>.Instance.P.GetAxis("Left Stick X") * ((num > 0f) ? 1f : (-1f)), Time.fixedDeltaTime * Common_Lua.c_waterslider_lr * 2f);
		this.X_WSPositionShift = Mathf.Clamp(this.X_WSPositionShift + this.X_WSSmoothPos * Time.fixedDeltaTime, -1f, 1f);
		this.X_WSTime += this.CurSpeed / this.X_WSSpline.Length() * Time.fixedDeltaTime;
		XDebug.Comment("tricky af part");
		if (this.X_WSTime > 1f || (this.X_WSTime > 0.25f && this.IsGrounded()))
		{
			MethodInfo method = base.GetType().GetMethod("StateGround", BindingFlags.Instance | BindingFlags.NonPublic);
			StateMachine.PlayerState playerState = Delegate.CreateDelegate(typeof(StateMachine.PlayerState), this, method) as StateMachine.PlayerState;
			this.StateMachine.ChangeState(playerState);
		}
		if (this.X_WSpeed <= 4f)
		{
			this.StateMachine.ChangeState((StateMachine.PlayerState)base.GetType().GetMethod("StateAir", BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(StateMachine.PlayerState), this));
		}
		this.X_WSDirection = this.X_WSSpline.GetTangent(this.X_WSTime, true).normalized;
		Vector3 normalized = Vector3.Cross(this.X_WSDirection, Vector3.up).normalized;
		this._Rigidbody.MovePosition(this.X_WSSpline.GetPosition(this.X_WSTime, true) + base.transform.up * 0.25f + normalized * this.X_WSPositionShift * 3f);
		this.GeneralMeshRotation = Quaternion.LookRotation(this.ForwardMeshRotation, this.UpMeshRotation);
		base.transform.rotation = Quaternion.LookRotation(this.X_WSDirection.MakePlanar());
		this._Rigidbody.velocity = Vector3.zero;
	}

	private void X_StateWaterSlideEnd()
	{
		this.MaxRayLenght = 0.75f;
	}

	private void X_StateFreeWaterSlideStart()
	{
		this.SetState("WaterSlide");
		this.LockControls = true;
		this.X_WSTime = Time.time;
		this.X_FWSpeedBegin = this.CurSpeed;
		this.X_FWSpeedTarget = Mathf.Min(Sonic_New_Lua.c_run_speed_max * 3f, this.CurSpeed * XDebug.Cfg.FWS.SpeedBoost);
		this.X_WSpeed = this.CurSpeed;
		XDebug.Comment("maybe also lerp offset");
	}

	private void X_StateFreeWaterSlide()
	{
		RaycastHit raycastHit;
		bool flag = this.X_HasWaterBelow(XDebug.Cfg.FWS.YMaxWaterRaycastDist, out raycastHit);
		Vector3 point = raycastHit.point;
		Vector3 normal = raycastHit.normal;
		bool flag2 = this.IsGrounded();
		Vector3 point2 = this.RaycastHit.point;
		if (!flag && !flag2)
		{
			this.X_SwitchToState("StateAir");
			return;
		}
		if (flag2 && !flag)
		{
			this.X_SwitchToState("StateGround");
			return;
		}
		if (flag2 && flag)
		{
			float num = Vector3.Distance(base.transform.position, point);
			if (Vector3.Distance(base.transform.position, point2) < num)
			{
				this.X_SwitchToState("StateGround");
				return;
			}
		}
		base.transform.position += new Vector3(0f, -base.transform.position.y + point.y + XDebug.Cfg.FWS.YWaterOffset, 0f);
		base.transform.rotation = Quaternion.FromToRotation(base.transform.up, normal) * base.transform.rotation;
		XDebug.Comment("if we somehow accelerated");
		if (this.CurSpeed > this.X_WSpeed)
		{
			this.X_FWSpeedTarget = (this.X_FWSpeedBegin = (this.X_WSpeed = this.CurSpeed));
		}
		if (Time.time - this.X_WSTime <= XDebug.Cfg.FWS.AccelTime)
		{
			float num2 = (Time.time - this.X_WSTime) / XDebug.Cfg.FWS.AccelTime;
			this.X_WSpeed = Mathf.Lerp(this.X_FWSpeedBegin, this.X_FWSpeedTarget, Mathf.Sqrt(num2));
		}
		else if (this.X_WSpeed > 0f)
		{
			this.X_WSpeed -= 2f * Time.fixedDeltaTime;
		}
		if (this.X_WSpeed <= 5f)
		{
			this.StateMachine.ChangeState((StateMachine.PlayerState)base.GetType().GetMethod("StateAir", BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(StateMachine.PlayerState), this));
			return;
		}
		XDebug.Comment("Extra slowdown when 'running'");
		if (this.X_WSpeed > XDebug.Cfg.FWS.MinRunAnimationSpeed && Singleton<RInput>.Instance.P.GetAxis("Left Stick Y") <= 0f)
		{
			this.X_WSpeed -= XDebug.Cfg.FWS.RunningBrakeSpeed * Time.fixedDeltaTime;
		}
		this.X_StateFreeWaterSlideSetAnimation();
		this.CurSpeed = this.X_WSpeed;
		this.GeneralMeshRotation = Quaternion.LookRotation(this.ForwardMeshRotation, this.UpMeshRotation);
		this._Rigidbody.velocity = base.transform.forward * this.X_WSpeed;
	}

	private void X_StateFreeWaterSlideEnd()
	{
		this.MaxRayLenght = 0.75f;
		this.LockControls = false;
	}

	private void X_StateFreeWaterSlideSetAnimation()
	{
		if (this.X_WSpeed <= 8f)
		{
			this.PlayAnimation("Edge Danger", "On Edge Danger");
			return;
		}
		if (this.X_WSpeed <= XDebug.Cfg.FWS.MinRunAnimationSpeed)
		{
			this.Animator.CrossFadeInFixedTime("Brake", 0.04f);
			return;
		}
		this.PlayAnimation("Movement (Blend Tree)", "On Ground");
	}

	private bool X_HasWaterBelow(float maxDist, ref Vector3 waterPosition)
	{
		RaycastHit[] array = Physics.RaycastAll(base.transform.position, -Vector3.up, maxDist);
		bool flag = false;
		foreach (RaycastHit raycastHit in array)
		{
			if (raycastHit.transform.tag == "Water")
			{
				flag = true;
				waterPosition = raycastHit.point;
				break;
			}
		}
		return flag;
	}

	protected void X_SwitchToState(string state)
	{
		MethodInfo method = base.GetType().GetMethod(state, BindingFlags.Instance | BindingFlags.NonPublic);
		StateMachine.PlayerState playerState = Delegate.CreateDelegate(typeof(StateMachine.PlayerState), this, method) as StateMachine.PlayerState;
		this.StateMachine.ChangeState(playerState);
	}

	private bool X_HasWaterBelow(float maxDist, out RaycastHit waterHit)
	{
		waterHit = default(RaycastHit);
		foreach (RaycastHit raycastHit in Physics.RaycastAll(base.transform.position, -Vector3.up, maxDist))
		{
			if (raycastHit.transform.tag == "Water")
			{
				waterHit = raycastHit;
				return true;
			}
		}
		return false;
	}

	protected void X_StateWallJumpStart()
	{
		this.X_WJTime = Time.time;
		this.X_WJIsWaiting = true;
		this.X_WJNormal = this.FrontalHit.normal;
		base.transform.up = Vector3.up;
		base.transform.forward = this.FrontalHit.normal;
		this.SetState("WallJump");
		if (this.GetPrefab("sonic_new") || this.GetPrefab("shadow") || this.GetPrefab("sonic_fast") || this.GetPrefab("princess"))
		{
			this.PlayAnimation("Chain Jump Wall Wait", "On Chain Jump Wall Wait");
			this.GeneralMeshRotation = Quaternion.LookRotation(base.transform.forward, base.transform.up) * Quaternion.Euler(XDebug.Cfg.WJ.MeshRotation);
		}
		else if (this.GetPrefab("rouge"))
		{
			this.PlayAnimation("Crouch", "On Crouch");
			this.GeneralMeshRotation = Quaternion.LookRotation(base.transform.forward, base.transform.up) * Quaternion.Euler(-90f, 180f, 0f);
		}
		else
		{
			this.PlayAnimation("Up Reel", "On Up Reel");
			this.GeneralMeshRotation = Quaternion.LookRotation(base.transform.forward, base.transform.up) * Quaternion.Euler(0f, 180f, 0f);
			this.X_WJOtherCharacter = true;
		}
		base.transform.position = this.FrontalHit.point + base.transform.up * XDebug.Cfg.WJ.UpOffset + this.FrontalHit.normal * ((!this.X_WJOtherCharacter) ? XDebug.Cfg.WJ.NormalOffset : 0f);
		XDebug.Instance.DrawVectorFast(base.transform.position, base.transform.position + base.transform.up, Color.blue, 3);
		this._Rigidbody.velocity = Vector3.zero;
		this.LockControls = true;
		this.Audio.PlayOneShot(XFiles.Instance.WallLand, this.Audio.volume * 0.4f);
	}

	protected void X_StateWallJump()
	{
		this.LockControls = true;
		if (Time.time - this.X_WJTime > XDebug.Cfg.WJ.MaxWaitTime)
		{
			if (this.X_WJOtherCharacter)
			{
				base.transform.position += this.X_WJNormal * XDebug.Cfg.WJ.NormalOffset;
			}
			this.X_SwitchToState("StateAir");
			return;
		}
	}

	protected void X_StateWallJumpEnd()
	{
		this.X_WJIsWaiting = false;
		this.LockControls = false;
		this.X_WJOtherCharacter = false;
	}

	protected void X_HandleBoost()
	{
		XDebug.Comment("EXPERIMENTAL");
		bool x_BIsBoosting = this.X_BIsBoosting;
		bool flag = (!this.GetPrefab("sonic_new") || (this as SonicNew).ActiveGem == SonicNew.Gem.None) && (Input.GetKey(KeyCode.LeftControl) || Singleton<RInput>.Instance.P.GetButton("Right Trigger")) && (!this.LockControls || this.GetPrefab("snow_board") || this.GetState().IsIn(new string[] { "Grinding", "DashPanel", "Path", "X_WaterSlide", "WaterSlide" }));
		if (!x_BIsBoosting && flag)
		{
			XDebug.Comment("Start");
			XSingleton<XEffects>.Instance.CreateDodgeFX();
			this.X_BTime = Time.time;
			this.X_BStartSpeed = this.X_GetActualSpeedForward();
			float num = this.X_BStartSpeed + XDebug.Instance.Boost_NextLevelThreshold.Value - XDebug.Instance.Boost_BaseSpeed.Value;
			if (XDebug.Instance.Boost_NextLevelDeltaSpeed.Value <= 0f || num <= 0f)
			{
				this.X_BTargetSpeed = XDebug.Instance.Boost_BaseSpeed.Value;
			}
			else
			{
				this.X_BTargetSpeed = XDebug.Instance.Boost_BaseSpeed.Value + Mathf.Ceil(num / XDebug.Instance.Boost_NextLevelDeltaSpeed.Value) * XDebug.Instance.Boost_NextLevelDeltaSpeed.Value;
			}
		}
		if (flag)
		{
			XDebug.Comment("Start or Continue");
			float num2 = Mathf.Lerp(this.X_BStartSpeed, this.X_BTargetSpeed, Mathf.Sqrt((Time.time - this.X_BTime) / XDebug.Instance.Boost_AccelTime.Value));
			float num3 = Vector3.Dot(base.transform.forward, this._Rigidbody.velocity);
			if (this.GetPrefab("snow_board"))
			{
				SnowBoard snowBoard = (SnowBoard)this;
				Vector3 vector = (Vector3)typeof(SnowBoard).GetField("Speed", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(snowBoard);
				this._Rigidbody.velocity += base.transform.forward * (num2 - num3);
				this.CurSpeed = this._Rigidbody.velocity.magnitude;
			}
			else if (!this.LockControls || this.GetState() == "DashPanel")
			{
				this._Rigidbody.velocity += base.transform.forward * (num2 - num3);
				this.CurSpeed = this._Rigidbody.velocity.magnitude;
			}
			else if (this.GetState() == "Path")
			{
				this.PathSpeed = num2;
			}
			else if (this.GetState() == "Grinding")
			{
				this.GrindSpeed = num2;
			}
			else if (this.GetState() == "X_WaterSlide")
			{
				this.X_WSpeed = num2;
			}
			else if (this.GetState() == "WaterSlide" && this.GetPrefab("sonic_new"))
			{
				XDebug.Comment("crazxy shit just to get it working without too much modifications");
				SonicNew sonicNew = (SonicNew)this;
				typeof(SonicNew).GetField("WSpeed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(sonicNew, num2);
			}
			this.AttackSphere(base.transform.position + base.transform.up * 0.25f, 1f, base.transform.forward * this._Rigidbody.velocity.magnitude, 1, "");
		}
		if (x_BIsBoosting && !flag)
		{
			if (this is SnowBoard)
			{
				float num4 = Mathf.Lerp(this.X_BStartSpeed, this.X_BTargetSpeed, Mathf.Sqrt((Time.time - this.X_BTime) / XDebug.Instance.Boost_AccelTime.Value));
				float num5 = Vector3.Dot(base.transform.forward, this._Rigidbody.velocity);
				this._Rigidbody.velocity += base.transform.forward * (num4 - num5);
				typeof(SnowBoard).GetField("Speed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, this._Rigidbody.velocity);
				this.CurSpeed = this._Rigidbody.velocity.magnitude;
			}
			XDebug.Comment("Stop");
			XSingleton<XEffects>.Instance.DestroyDodgeFX();
		}
		this.X_BIsBoosting = flag;
	}

	public bool X_CanWallJump()
	{
		XDebug.Comment("[CHANGE] added if for wall jumping transition");
		if (XDebug.Instance.Moveset_WallJumping.Value && this.GetState().IsIn(new string[] { "Jump", "Air", "AfterHoming", "Homing", "Fly", "Glide" }) && this.FrontalCollision && this.FrontalHit.transform != null && !this.X_BIsBoosting && !this.X_HasGroundBelow(XDebug.Cfg.WJ.MinHeightAboveGround))
		{
			if (((this.GetPrefab("knuckles") || this.GetPrefab("rouge")) && this.FrontalHit.transform && this.FrontalHit.transform.tag == "ClimbableWall") || this.GetPrefab("sonic_fast") || this.GetPrefab("snow_board"))
			{
				XDebug.Comment("don't switch to wall jump");
				XDebug.Comment("there's also CanClimb()");
				return false;
			}
			XDebug.Instance.DrawVectorFast(base.transform.position, base.transform.position + this.FrontalHit.normal, Color.red, 2);
			float num = Vector3.Dot(this.FrontalHit.normal, Vector3.up);
			if (XDebug.Cfg.WJ.MinDotNormal <= num && this._Rigidbody.velocity.y < 0f && num < XDebug.Cfg.WJ.MaxDotNormal)
			{
				return true;
			}
		}
		return false;
	}

	private bool X_HasGroundBelow(float maxDist)
	{
		RaycastHit raycastHit;
		return Physics.Raycast(base.transform.position, -base.transform.up, out raycastHit, maxDist, this.Collision_Mask);
	}

	public float X_GetActualSpeedForward()
	{
		return Vector3.Dot(this._Rigidbody.velocity, base.transform.forward.normalized);
	}

	public void X_HandleWallJump()
	{
		if (Singleton<GameManager>.Instance.GameState != GameManager.State.Paused && this.StageManager.StageState != StageManager.State.Event && !this.IsDead && this.GetState() != "Talk" && this.X_CanWallJump())
		{
			this.StateMachine.ChangeState(new StateMachine.PlayerState(this.X_StateWallJump));
		}
	}


	private string X_LaunchMode;

	protected float X_WSpeed;

	private float X_WSTime;

	private Vector3 X_WSDirection;

	private BezierCurve X_WSSpline;

	private float X_WSPositionShift;

	private float X_WSSmoothPos;

	private float X_FWSWaterPosition;

	protected float X_FWSpeedTarget;

	protected float X_FWSpeedBegin;

	protected float X_WJTime;

	protected bool X_WJIsWaiting;

	protected Vector3 X_WJNormal;

	protected bool X_WJOtherCharacter;

	protected bool X_BIsBoosting;

	private float X_BStartSpeed;

	private float X_BTargetSpeed;

	private float X_BTime;
}
