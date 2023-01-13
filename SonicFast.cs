using System;
using STHLua;
using UnityEngine;

public partial class SonicFast : PlayerBase
{
	// This is an example section of an Update() function, which (like many other functions) I also modified,
	// but I chose not to publish these changes on github, because they have large portions of the original code from the game
	public override void Update()
	{

		// ... original game code ...
		if (Singleton<GameManager>.Instance.GameState != GameManager.State.Paused && Singleton<GameManager>.Instance.GameState != GameManager.State.Result && this.StageManager.StageState != StageManager.State.Event && !this.IsDead && this.PlayerState != SonicFast.State.Talk)
		{
			if (this.PlayerState == SonicFast.State.Ground && base.CanJumpFromSink())
			{
				XDebug.Comment("[CHANGE] Variable-length dodging v3.1");
				if (Time.time >= this.X_DodgeEndTime)
				{
					bool flag = !XDebug.Instance.Other_OgCameraControls.Value || Singleton<RInput>.Instance.P.GetButton("Left Trigger");
					float num = Vector3.Dot(base.transform.forward, this.Camera.transform.forward);
					if (flag && Singleton<RInput>.Instance.P.GetButtonDown("Right Bumper"))
					{
						this.X_DodgeDir = ((num >= 0f) ? 1 : (-1));
						this.X_DodgeButton = "Right Bumper";
						this.StateMachine.ChangeState(new StateMachine.PlayerState(this.X_StateDodge));
						XDebug.Instance.JustUsedLeftTrigger = true;
					}
					else if (flag && Singleton<RInput>.Instance.P.GetButtonDown("Left Bumper"))
					{
						this.X_DodgeDir = ((num >= 0f) ? (-1) : 1);
						this.X_DodgeButton = "Left Bumper";
						this.StateMachine.ChangeState(new StateMachine.PlayerState(this.X_StateDodge));
						XDebug.Instance.JustUsedLeftTrigger = true;
					}
				}
				if (Singleton<RInput>.Instance.P.GetButtonDown("Button A"))
				{
					this.X_SecondJump = false;
					this.StateMachine.ChangeState(new StateMachine.PlayerState(this.StateJump));
				}

				// ... original code ...
			}
			// ... original code ...

			if (this.PlayerState == SonicFast.State.Air || this.PlayerState == SonicFast.State.Jump || (this.PlayerState == SonicFast.State.WideSpring && !this.LockControls))
			{
				// ... original code ...
				if (Singleton<RInput>.Instance.P.GetButtonDown("Button B"))
				{
					this.StateMachine.ChangeState(new StateMachine.PlayerState(this.X_StateStomp));
				}
			}
			// ... original code ...
		}
	}

	public SonicFast()
	{
		this.X_DodgeAutoFwdRotMode = 2;
		this.X_Dodge_fwdacc = true;
		this.X_DodgeVelMult = 0.5f;
		this.X_DodgeSpeed = 18f;
		this.X_DodgeRotDuration = 0.1f;
		this.X_DodgeRotBackDuration = 0.03f;
		this.X_DodgeAccDuration = 0.03f;
		this.X_Dodge_dmin = 0.15f;
		this.X_Dodge_dmax = 0.4f;
	}

	private void X_StateDodgeStart()
	{
		this.PlayerState = SonicFast.State.Ground;
		this.LockControls = true;
		this.X_DodgeTime = Time.time;
		this.X_DodgeHolding = true;
		this.X_DodgeStopped = false;
		this.X_DodgeEndTime = this.X_DodgeTime + 9999999f;
		this.X_PreDodgeCurSpeed = this.CurSpeed;
		if (this.StageManager._Stage == StageManager.Stage.csc)
		{
			if (this.X_DodgeAutoFwdRotMode == 1)
			{
				XDebug.Comment("Adjust forward rotation to camera automatically");
				XDebug.Comment("This works only in Crisis City E --> to prevent sudden flying off the road");
				bool flag = Vector3.Dot(this.Camera.transform.forward, this._Rigidbody.velocity) < 0f;
				Vector3 vector = Vector3.ProjectOnPlane(this.Camera.transform.forward * (float)(flag ? (-1) : 1), this.RaycastHit.normal);
				if (Vector3.Angle(vector, Vector3.ProjectOnPlane(base.transform.forward, this.RaycastHit.normal)) > 5f)
				{
					XDebug.Comment(string.Format("<color=#ee6600>Adjusted direction by {0} deg</color>", Vector3.Angle(vector, Vector3.ProjectOnPlane(base.transform.forward, this.RaycastHit.normal))));
				}
				base.transform.forward = vector;
			}
			else if (this.X_DodgeAutoFwdRotMode == 2)
			{
				base.transform.forward = Vector3.ProjectOnPlane(new Vector3(-1f, 0f, 0f), this.RaycastHit.normal).normalized;
			}
		}
		Vector3 normalized = Vector3.ProjectOnPlane(Vector3.Cross(this.UpMeshRotation, this.ForwardMeshRotation), this.RaycastHit.normal).normalized;
		if (XDebug.COMMENT)
		{
			this.X_DodgeMaxxVel = base.transform.right * (float)this.X_DodgeDir * this.X_DodgeSpeed;
		}
		else
		{
			this.X_DodgeMaxxVel = this.X_RealRight() * (float)this.X_DodgeDir * this.X_DodgeSpeed;
		}
		this.AirMotionVelocity = this._Rigidbody.velocity;
		if (!this.X_Dodge_fwdacc)
		{
			this._Rigidbody.velocity = this.AirMotionVelocity * this.X_DodgeVelMult;
		}
		if (!this._tmp)
		{
			this.Camera.transform.position += this._Rigidbody.velocity * Time.deltaTime;
		}
		this.X_DodgeRotA = this.GeneralMeshRotation;
		this.X_DodgeRotB = this.GeneralMeshRotation * Quaternion.Euler(0f, 0f, (float)(-30 * this.X_DodgeDir));
		this.Animator.CrossFadeInFixedTime("Light Dash", 0.04f);
		XSingleton<XEffects>.Instance.CreateDodgeFX();
		this.Audio.PlayOneShot(XDebug.Instance.DodgeClipFull, this.Audio.volume * 1.2f);
	}

	private void X_StateDodge()
	{
		if (!Singleton<RInput>.Instance.P.GetButton(this.X_DodgeButton))
		{
			this.X_DodgeHolding = false;
		}
		if (!this.X_DodgeStopped)
		{
			float num = Time.time - this.X_DodgeTime;
			if (num >= this.X_Dodge_dmax - this.X_DodgeAccDuration || (num >= this.X_Dodge_dmin - this.X_DodgeAccDuration && !this.X_DodgeHolding))
			{
				this.X_DodgeStopped = true;
				this.X_DodgeEndTime = Time.time + this.X_DodgeAccDuration;
			}
		}
		this.GeneralMeshRotation = Quaternion.LookRotation(this.ForwardMeshRotation, this.UpMeshRotation);
		this.X_DodgeRotA = this.GeneralMeshRotation;
		this.X_DodgeRotB = this.GeneralMeshRotation * Quaternion.Euler(0f, 0f, -30f * (float)this.X_DodgeDir);
		if (Time.time - this.X_DodgeTime <= this.X_DodgeRotDuration)
		{
			this.GeneralMeshRotation = Quaternion.Slerp(this.X_DodgeRotA, this.X_DodgeRotB, (Time.time - this.X_DodgeTime) / this.X_DodgeRotDuration);
		}
		else if (this.X_DodgeEndTime - Time.time <= this.X_DodgeRotBackDuration)
		{
			this.GeneralMeshRotation = Quaternion.Slerp(this.X_DodgeRotB, this.X_DodgeRotA, 1f - (this.X_DodgeEndTime - Time.time) / this.X_DodgeRotBackDuration);
		}
		else
		{
			this.GeneralMeshRotation = this.X_DodgeRotB;
		}
		if (this._tm2)
		{
			this.X_DodgeMaxxVel = this.X_RealRight() * (float)this.X_DodgeDir * this.X_DodgeSpeed;
		}
		float num2;
		if (Time.time - this.X_DodgeTime <= this.X_DodgeAccDuration)
		{
			this.X_DodgeCurrVel = Vector3.Slerp(Vector3.zero, this.X_DodgeMaxxVel, (Time.time - this.X_DodgeTime) / this.X_DodgeAccDuration);
			num2 = Mathf.Lerp(1f, this.X_DodgeVelMult, (Time.time - this.X_DodgeTime) / this.X_DodgeAccDuration);
		}
		else if (this.X_DodgeEndTime - Time.time <= this.X_DodgeAccDuration)
		{
			this.X_DodgeCurrVel = Vector3.Slerp(this.X_DodgeMaxxVel, Vector3.zero, 1f - (this.X_DodgeEndTime - Time.time) / this.X_DodgeAccDuration);
			num2 = Mathf.Lerp(this.X_DodgeVelMult, 1f, 1f - (this.X_DodgeEndTime - Time.time) / this.X_DodgeAccDuration);
		}
		else
		{
			this.X_DodgeCurrVel = this.X_DodgeMaxxVel;
			num2 = this.X_DodgeVelMult;
		}
		base.transform.rotation = Quaternion.FromToRotation(base.transform.up, this.RaycastHit.normal) * base.transform.rotation;
		this.AirMotionVelocity = Vector3.ProjectOnPlane(base.transform.forward, this.RaycastHit.normal) * this.X_PreDodgeCurSpeed;
		if (this.X_Dodge_fwdacc)
		{
			this._Rigidbody.velocity = this.AirMotionVelocity * num2 + this.X_DodgeCurrVel;
		}
		else
		{
			this._Rigidbody.velocity = this.AirMotionVelocity * this.X_DodgeVelMult + this.X_DodgeCurrVel;
		}
		if (!this._tmp)
		{
			this.Camera.transform.position += this._Rigidbody.velocity * Time.deltaTime;
		}
		else
		{
			this.Camera.transform.position += this.X_DodgeCurrVel * Time.deltaTime;
		}
		if (Time.time >= this.X_DodgeEndTime)
		{
			XDebug.Comment("|| Vector3.Dot(base.transform.right * (float)this.X_DodgeDir, this._Rigidbody.velocity) < 0.1f)");
			this.StateMachine.ChangeState(new StateMachine.PlayerState(this.StateGround));
		}
	}

	private void X_StateDodgeEnd()
	{
		this.X_DodgeEndTime = Time.time;
		this.LockControls = false;
		this.X_DodgeHolding = false;
		this.CurSpeed = this.X_PreDodgeCurSpeed;
		this._Rigidbody.velocity = base.transform.forward * this.AirMotionVelocity.magnitude;
		if (XDebug.COMMENT)
		{
			this.Animator.CrossFadeInFixedTime("Movement (Blend Tree)", 0.03f);
		}
		XSingleton<XEffects>.Instance.DestroyDodgeFX();
	}

	private Vector3 X_RealRight()
	{
		return Vector3.ProjectOnPlane(Vector3.Cross(this.RaycastHit.normal, base.transform.forward), this.RaycastHit.normal).normalized;
	}

	private void X_StateStompStart()
	{
		this.BoundState = 42;
		this.PlayerState = SonicFast.State.BoundAttack;
		this.AirMotionVelocity = this._Rigidbody.velocity;
		this.AirMotionVelocity.y = Sonic_New_Lua.c_boundjump_jmp * 1.35f;
		this._Rigidbody.velocity = this.AirMotionVelocity;
		this.Audio.PlayOneShot(this.BoundStart, this.Audio.volume * 0.5f);
		XSingleton<XEffects>.Instance.CreateStompFX();
		this.X_StompDestroyed = false;
	}

	private void X_StateStomp()
	{
		this.PlayerState = SonicFast.State.BoundAttack;
		this.GeneralMeshRotation = Quaternion.LookRotation(this.ForwardMeshRotation, this.UpMeshRotation);
		base.transform.rotation *= Quaternion.FromToRotation(base.transform.up, Vector3.up);
		Vector3 vector = new Vector3(this.AirMotionVelocity.x, 0f, this.AirMotionVelocity.z);
		if (this._Rigidbody.velocity.magnitude != 0f)
		{
			vector = base.transform.forward * this.CurSpeed;
			this.AirMotionVelocity = new Vector3(vector.x, this.AirMotionVelocity.y, vector.z);
		}
		base.PlayAnimation("Falling", "On Fall");
		if (base.IsGrounded() && base.ShouldAlignOrFall(false))
		{
			base.AttackSphere_Dir(base.transform.position, Sonic_New_Lua.c_boundattack_collision.radius * 2f, 30f, 1);
			this.StateMachine.ChangeState(new StateMachine.PlayerState(this.StateGround));
			XSingleton<XEffects>.Instance.DestroyStompFX(true);
			this.X_StompDestroyed = true;
		}
		else
		{
			base.AttackSphere_Dir(base.transform.position, Sonic_New_Lua.c_boundattack_collision.radius * 1.25f, 25f, 1);
			this.AirMotionVelocity.y = this.AirMotionVelocity.y - 5f * Time.deltaTime;
		}
		this._Rigidbody.velocity = this.AirMotionVelocity;
		base.DoWallNormal();
	}

	private void X_StateStompEnd()
	{
		if (!this.X_StompDestroyed)
		{
			XSingleton<XEffects>.Instance.DestroyStompFX(false);
		}
	}

	private float X_DodgeTime;

	private int X_DodgeDir;

	private float X_PreDodgeCurSpeed;

	private Vector3 X_DodgeCurrVel;

	private Vector3 X_DodgeMaxxVel;

	private float X_DodgeRotDuration;

	private float X_DodgeRotBackDuration;

	private float X_DodgeSpeed;

	private float X_DodgeVelMult;

	private bool X_DodgeHolding;

	private Quaternion X_DodgeRotA;

	private Quaternion X_DodgeRotB;

	private float X_DodgeEndTime;

	private float X_DodgeAccDuration;

	private bool X_DodgeStopped;

	private float X_Dodge_dmin;

	private float X_Dodge_dmax;

	private string X_DodgeButton;

	private bool X_Dodge_fwdacc;

	private bool _tm2 = true;

	private bool _tmp = true;

	private int X_DodgeAutoFwdRotMode;

	private bool X_SecondJump;

	private float X_StompSpeedMult = 1f;

	private bool X_StompDestroyed;
}
