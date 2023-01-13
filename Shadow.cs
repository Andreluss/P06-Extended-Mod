using System;
using System.Collections;
using System.Collections.Generic;
using STHLua;
using UnityEngine;

public partial class Shadow : PlayerBase
{
	public Shadow()
	{
		this.X_CanJumpDash = true;
		this.X_DodgeDist = 3f;
		this.X_DodgeDuration = 0.15f;
		this.X_DodgeVelMult = 0.4f;
		this.X_DodgeRotAngles = new Vector3(0f, 0f, -30f);
		this.X_DodgeRotDuration = 0.1f;
		this.X_DodgeRotBackDuration = 0.025f;
		this.X_DodgeOffset = 0.15f;
	}

	private void X_StateDodgeStart()
	{
		this.PlayerState = Shadow.State.Ground;
		this.X_DodgeTime = Time.time;
		this.LockControls = true;
		this.AirMotionVelocity = this._Rigidbody.velocity;
		this.X_PreDodgeCurSpeed = this.CurSpeed;
		this.X_DodgeCurrSpeed = Vector3.zero;
		this.X_DodgeBaseSpeed = this.X_DodgeDist / this.X_DodgeDuration * base.transform.right * this.X_DodgeDir;
		this._Rigidbody.velocity = this.AirMotionVelocity * this.X_DodgeVelMult + this.X_DodgeCurrSpeed / 4f;
		XSingleton<XEffects>.Instance.CreateDodgeFX();
		this.Animator.CrossFadeInFixedTime("Light Dash", 0.04f);
		this.X_DodgeRotA = this.GeneralMeshRotation;
		this.X_DodgeRotB = this.GeneralMeshRotation * Quaternion.Euler(this.X_DodgeDir * this.X_DodgeRotAngles);
		this.Camera.transform.position += this._Rigidbody.velocity * Time.deltaTime;
		this.X_NextDodgeTime = Time.time + 9999f;
		this.Audio.PlayOneShot(XDebug.Instance.DodgeClip, this.Audio.volume * 1.5f);
	}

	private void X_StateDodge()
	{
		float num = this.X_DodgeTime + this.X_DodgeDuration;
		if (Time.time - this.X_DodgeTime <= this.X_DodgeRotDuration)
		{
			this.GeneralMeshRotation = Quaternion.Slerp(this.X_DodgeRotA, this.X_DodgeRotB, (Time.time - this.X_DodgeTime) / this.X_DodgeRotDuration);
		}
		else if (num - Time.time <= this.X_DodgeRotBackDuration)
		{
			this.GeneralMeshRotation = Quaternion.Slerp(this.X_DodgeRotB, this.X_DodgeRotA, 1f - (num - Time.time) / this.X_DodgeRotBackDuration);
		}
		float num2 = (Time.time - this.X_DodgeTime) / this.X_DodgeDuration;
		if (num2 < this.X_DodgeOffset)
		{
			this.X_DodgeCurrSpeed = Vector3.Slerp(this.X_DodgeBaseSpeed * this.X_DodgeSlow, this.X_DodgeBaseSpeed, num2 / 0.1f);
		}
		else if (1f - this.X_DodgeOffset < num2)
		{
			this.X_DodgeCurrSpeed = Vector3.Slerp(this.X_DodgeBaseSpeed, this.X_DodgeBaseSpeed * this.X_DodgeSlow, (num2 - (1f - this.X_DodgeOffset)) / this.X_DodgeOffset);
		}
		else
		{
			this.X_DodgeCurrSpeed = this.X_DodgeBaseSpeed;
		}
		this._Rigidbody.velocity = this.AirMotionVelocity * this.X_DodgeVelMult + this.X_DodgeCurrSpeed;
		this.Camera.transform.position += this._Rigidbody.velocity * Time.deltaTime * 1f;
		if (XDebug.COMMENT)
		{
			base.PlayAnimation("Light Dash", "On Light Dash");
		}
		if (Time.time - this.X_DodgeTime >= this.X_DodgeDuration || this.ShouldEdgeDanger() || Vector3.Dot(base.transform.right * this.X_DodgeDir, this._Rigidbody.velocity) < 0.1f)
		{
			this.StateMachine.ChangeState(new StateMachine.PlayerState(this.StateGround));
		}
	}

	private void X_StateDodgeEnd()
	{
		this.LockControls = false;
		this._Rigidbody.velocity = base.transform.forward * this.AirMotionVelocity.magnitude;
		XDebug.Comment(" this.Camera.transform.position += this._Rigidbody.velocity * Time.deltaTime;");
		this.CurSpeed = this.X_PreDodgeCurSpeed;
		XSingleton<XEffects>.Instance.DestroyDodgeFX();
		this.X_NextDodgeTime = Time.time + this.X_NextDodgeDelay;
	}

	private void X_StateStompStart()
	{
		XDebug.Comment("Air????");
		this.PlayerState = Shadow.State.Air;
		this.AirMotionVelocity = this._Rigidbody.velocity;
		this.AirMotionVelocity.y = Sonic_New_Lua.c_boundjump_jmp * 1.5f;
		this._Rigidbody.velocity = this.AirMotionVelocity;
		this.Audio.PlayOneShot(this.SpinDashShoot, this.Audio.volume * 0.5f);
		XSingleton<XEffects>.Instance.CreateStompShadowFX();
		this.X_StompDestroyed = false;
		XDebug.Comment("Change this to stomp-state?");
		this.X_IsStomping = true;
	}

	private void X_StateStomp()
	{
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
			this.Audio.PlayOneShot(XFiles.Instance.StompLand, this.Audio.volume * 0.7f);
			base.AttackSphere_Dir(base.transform.position, Sonic_New_Lua.c_boundattack_collision.radius * 3f, 30f, 1);
			float axis = Singleton<RInput>.Instance.P.GetAxis("Left Stick Y");
			float axis2 = Singleton<RInput>.Instance.P.GetAxis("Left Stick X");
			if (axis != 0f || axis2 != 0f)
			{
				if (XDebug.FASTER_STOMPDASH)
				{
					float num = Mathf.Min(1f, Mathf.Abs(axis) + Mathf.Abs(axis2));
					this.CurSpeed *= num * this.X_Stomp_speed_mult;
				}
				this.StateMachine.ChangeState(new StateMachine.PlayerState(this.StateSpinDash));
				XSingleton<XEffects>.Instance.DestroyStompShadowFX(true);
				this.X_StompDestroyed = true;
			}
			else
			{
				Collider[] array = Physics.OverlapSphere(base.transform.position, 1.7f);
				bool flag = false;
				foreach (Collider collider in array)
				{
					if (collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
					{
						XDebug.Comment(string.Format("We collided with enemy {0} [tot colliders: {1}", collider, array.Length));
						flag = true;
						break;
					}
				}
				if (flag)
				{
					this.StateMachine.ChangeState(new StateMachine.PlayerState(this.StateJump));
				}
				else
				{
					this.StateMachine.ChangeState(new StateMachine.PlayerState(this.X_StateGetUpX));
				}
				XDebug.Comment("X_StunShpere");
				base.StunSphere(base.transform.position, 6f, false);
				XSingleton<XEffects>.Instance.CreateStompCrashShadowFX(this.RaycastHit);
			}
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
		this.X_IsStomping = false;
		if (!this.X_StompDestroyed)
		{
			XSingleton<XEffects>.Instance.DestroyStompShadowFX(false);
		}
	}

	private void X_StateGetUpXStart()
	{
		this.PlayerState = Shadow.State.Ground;
		this.GetUpTime = Time.time;
	}

	private void X_StateGetUpX()
	{
		base.PlayAnimation("Tornado Return", "On Tornado Return");
		this.LockControls = false;
		this._Rigidbody.velocity = Vector3.zero;
		this.GeneralMeshRotation = Quaternion.LookRotation(this.ForwardMeshRotation, this.UpMeshRotation);
		this.CurSpeed = 0f;
		if (!base.IsGrounded() || Time.time - this.GetUpTime > 0.55f)
		{
			this.StateMachine.ChangeState(new StateMachine.PlayerState(this.StateGround));
		}
	}

	private void X_StateGetUpXEnd()
	{
	}

	private float X_DodgeTime;

	private float X_PreDodgeCurSpeed;

	private Vector3 X_DodgeCurrSpeed;

	private Vector3 X_DodgeBaseSpeed;

	private float X_DodgeDist;

	private float X_DodgeDuration;

	private float X_DodgeVelMult;

	private Quaternion X_DodgeRotA;

	private Quaternion X_DodgeRotB;

	private Vector3 X_DodgeRotAngles;

	private float X_DodgeRotDuration;

	private float X_DodgeRotBackDuration;

	private float X_DodgeOffset;

	private float X_DodgeSlow;

	private float X_NextDodgeDelay;

	private float X_NextDodgeTime;

	private float X_DodgeDir;

	private bool X_StompDestroyed;

	private bool X_IsStomping;

	private bool X_CanJumpDash;

	private float X_Stomp_speed_mult = 2.5f;
}
