using System;
using System.Collections;
using UnityEngine;

public class XEffects : XSingleton<XEffects>
{
	public void DestroyDodgeFX()
	{
		ParticleSystem.EmissionModule emission = this.DodgeFX.GetComponent<ParticleSystem>().emission;
		emission.rateOverTimeMultiplier = 0f;
		emission.burstCount = emission.burstCount;
	}

	public void CreateDodgeFX()
	{
		if (this.DodgeFX == null)
		{
			this.DodgeFX = new GameObject("X_DodgeFX");
			PlayerBase instance = XDebug.Finder<PlayerBase>.Instance;
			if (instance == null)
			{
				return;
			}
			GameObject gameObject = instance.gameObject;
			this.DodgeFX.transform.SetParent(gameObject.transform, false);
			string playerName = instance.PlayerName;
			SkinnedMeshRenderer skinnedMeshRenderer = null;
			try
			{
				skinnedMeshRenderer = gameObject.FindInChildren("Mesh").FindInChildren(playerName + "_Root").FindInChildren(playerName + "_Root")
					.GetComponent<SkinnedMeshRenderer>();
			}
			catch (Exception)
			{
				XDebug.Comment("[ERROR]");
				return;
			}
			ParticleSystem particleSystem = this.DodgeFX.AddComponent<ParticleSystem>();
			ParticleSystem.MainModule main = particleSystem.main;
			main.duration = 10f;
			main.startLifetime = new ParticleSystem.MinMaxCurve(0.1f, 0.35f);
			main.startSpeed = new ParticleSystem.MinMaxCurve(-0.3f, 0.8f);
			main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.05f);
			ParticleSystem.EmissionModule emission = particleSystem.emission;
			emission.enabled = true;
			emission.rateOverTime = XEffects.DodgeParams.Rate;
			XDebug.Comment("emission.SetBurst(0, new ParticleSystem.Burst(0f, new ParticleSystem.MinMaxCurve(200f), 1, 2f))");
			ParticleSystem.ShapeModule shape = particleSystem.shape;
			shape.enabled = true;
			shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
			shape.meshShapeType = ParticleSystemMeshShapeType.Triangle;
			shape.skinnedMeshRenderer = skinnedMeshRenderer;
			shape.useMeshColors = false;
			ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particleSystem.velocityOverLifetime;
			velocityOverLifetime.enabled = false;
			velocityOverLifetime.orbitalX = 0.1f;
			velocityOverLifetime.orbitalY = 0.15f;
			velocityOverLifetime.orbitalZ = 0.3f;
			ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
			colorOverLifetime.enabled = true;
			XDebug.Comment("//Colors BACKUP:\r\n\t\t\t\tColor startColor = new Color(0.34117648f, 0.2509804f, 0.9607843f) * this.GLOW;\r\n\t\t\t\tColor endColor = new Color(0.64705884f, 0.11764706f, 0.7921569f) * this.GLOW;\r\n\t\t\t");
			XDebug.Comment("new Color(0f, 0.09411765f, 0.9607843f) * this.GLOW; new Color(0.64705884f, 0.11764706f, 1f) * this.GLOW;");
			Color color = XEffects.DodgeParams.Color * XEffects.DodgeParams.Glow;
			if ((XDebug.Finder<SonicNew>.Instance && XDebug.Finder<SonicNew>.Instance.IsSuper) || (XDebug.Finder<SonicFast>.Instance && XDebug.Finder<SonicFast>.Instance.IsSuper))
			{
				color = XEffects.DodgeParams.ColorSuper * XEffects.DodgeParams.Glow;
			}
			else if (playerName == "shadow")
			{
				color = XEffects.DodgeParams.ColorShadow;
			}
			Gradient gradient = new Gradient();
			gradient.SetKeys(new GradientColorKey[]
			{
				new GradientColorKey(color, 0f),
				new GradientColorKey(color, 1f)
			}, new GradientAlphaKey[]
			{
				new GradientAlphaKey(1f, 0f),
				new GradientAlphaKey(0f, 1f)
			});
			colorOverLifetime.color = gradient;
			ParticleSystem.TrailModule trails = particleSystem.trails;
			trails.enabled = true;
			trails.minVertexDistance = 0.05f;
			trails.worldSpace = true;
			XDebug.Comment("Renderer");
			ParticleSystemRenderer component = this.DodgeFX.GetComponent<ParticleSystemRenderer>();
			component.renderMode = ParticleSystemRenderMode.None;
			component.trailMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
			component.trailMaterial.mainTexture = XFiles.Instance.Particle;
		}
		this.DodgeFX.GetComponent<ParticleSystem>().Emit(XEffects.DodgeParams.Burst);
		this.DodgeFX.GetComponent<ParticleSystem>().emissionRate = XEffects.DodgeParams.Rate;
	}

	public void DestroyStompFX(bool quickMode = false)
	{
		base.StartCoroutine(this.DestroyStompFX_Coroutine(quickMode));
	}

	private IEnumerator DestroyStompFX_Coroutine(bool quickMode)
	{
		GameObject reference = this.StompFX;
		reference.transform.parent = null;
		ParticleSystem[] particleSystems = this.StompFX.GetComponentsInChildren<ParticleSystem>();
		float startTime = Time.time;
		float[] emissionRate = new float[3];
		for (int i = 0; i < 3; i++)
		{
			emissionRate[i] = particleSystems[i].emissionRate;
		}
		Color fadingColor = XEffects.StompParams.Color;
		float duration = 0.33f;
		while (Time.time - startTime < duration)
		{
			if (quickMode)
			{
				reference.transform.position -= new Vector3(0f, XEffects.StompParams.SinkSpeed * Time.deltaTime, 0f);
			}
			float num = Mathf.Sqrt((Time.time - startTime) / duration);
			for (int j = 0; j < 3; j++)
			{
				fadingColor.a = Mathf.Lerp(XEffects.StompParams.Color.a, 0f, Mathf.Sqrt(num));
				particleSystems[j].startColor = fadingColor;
				particleSystems[j].emissionRate = Mathf.Lerp(emissionRate[j], emissionRate[j] / 2f, num);
			}
			yield return null;
		}
		UnityEngine.Object.Destroy(reference);
		yield break;
	}

	public void CreateStompFX()
	{
		if (this.StompFXPrefab == null)
		{
			GameObject gameObject = Resources.Load<GameObject>("defaultprefabs/effect/player/sonic/LightAttackFX");
			this.StompFXPrefab = UnityEngine.Object.Instantiate<GameObject>(gameObject, Vector3.zero, Quaternion.identity);
			XDebug.Comment("!!!!!!!!!!!!!!!!!!!!");
			this.StompFXPrefab.GetComponent<AudioSource>().clip = (XDebug.Instance.SonicNew ? XDebug.Instance.SonicNew.JumpDashKickback : null);
			this.StompFXPrefab.GetComponent<MonoBehaviour>().enabled = false;
			UnityEngine.Object.Destroy(this.StompFXPrefab.GetComponent<MonoBehaviour>());
			ParticleSystem[] componentsInChildren = this.StompFXPrefab.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < 3; i++)
			{
				componentsInChildren[i].loop = true;
				if (XDebug.Instance.Player.PlayerName == "shadow")
				{
					componentsInChildren[i].startColor = new Color(1f, 0f, 0f);
					componentsInChildren[i].GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1f, 0f, 0f));
				}
				else
				{
					componentsInChildren[i].startColor = XEffects.StompParams.Color;
					componentsInChildren[i].GetComponent<Renderer>().material.SetColor("_TintColor", XEffects.StompParams.TintColor);
				}
			}
			for (int j = 3; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].enableEmission = false;
				UnityEngine.Object.Destroy(componentsInChildren[j]);
			}
			this.StompFXPrefab.SetActive(false);
		}
		SonicNew instance = XDebug.Finder<SonicNew>.Instance;
		Transform transform = ((instance != null) ? instance.transform : null);
		if (!transform)
		{
			Shadow instance2 = XDebug.Finder<Shadow>.Instance;
			transform = ((instance2 != null) ? instance2.transform : null);
		}
		if (!transform)
		{
			SonicFast instance3 = XDebug.Finder<SonicFast>.Instance;
			transform = ((instance3 != null) ? instance3.transform : null);
		}
		if (transform == null)
		{
			throw new Exception("no shadow no sonic thass bad");
		}
		this.StompFX = UnityEngine.Object.Instantiate<GameObject>(this.StompFXPrefab, transform.position + transform.up * XEffects.StompParams.Offset, transform.rotation * XEffects.StompParams.Rotation);
		this.StompFX.SetActive(true);
		this.StompFX.transform.SetParent(transform);
	}

	private void CreateStompTornadoFX(RaycastHit where)
	{
		if (this.StompTornadoFXPrefab == null)
		{
			this.StompTornadoFXPrefab = new GameObject("X_StompTornadoFX");
			ParticleSystem particleSystem = this.StompTornadoFXPrefab.AddComponent<ParticleSystem>();
			XDebug.Comment("========= main =========");
			ParticleSystem.MainModule main = particleSystem.main;
			main.loop = false;
			main.duration = 1f;
			main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 1f);
			main.startSpeed = new ParticleSystem.MinMaxCurve(-0.25f, 2.5f);
			main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
			XDebug.Comment("new Color(0.1529412f, 0.2f, 1f, 1f) * 3.5f;");
			XDebug.Comment("new Color(0.4386792f, 0.9042454f, 1f, 1f);");
			XDebug.Comment("this makes no effect actually: ");
			main.startColor = new Color(0f, 23f, 255f, 1f) * XEffects.StompTornadoParams.Glow / 2f;
			XDebug.Comment("I guess.....");
			main.gravityModifier = -0.1f;
			main.gravityModifier = 0f;
			XDebug.Comment("========= emiss ========");
			ParticleSystem.EmissionModule emission = particleSystem.emission;
			emission.enabled = true;
			emission.rateOverTime = 0f;
			emission.SetBursts(new ParticleSystem.Burst[]
			{
				new ParticleSystem.Burst(0f, 40)
			});
			XDebug.Comment("========= shape ========");
			ParticleSystem.ShapeModule shape = particleSystem.shape;
			shape.enabled = true;
			shape.shapeType = ParticleSystemShapeType.Cone;
			shape.rotation = new Vector3(-90f, 0f, 0f);
			shape.angle = 15f;
			shape.radius = 2.5f;
			shape.radiusThickness = 0.9f;
			XDebug.Comment("========= vel o/lt ========");
			ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particleSystem.velocityOverLifetime;
			velocityOverLifetime.enabled = true;
			velocityOverLifetime.orbitalX = 0.1f;
			velocityOverLifetime.orbitalY = -5f;
			velocityOverLifetime.orbitalZ = 0.1f;
			velocityOverLifetime.radial = 0.1f;
			velocityOverLifetime.speedModifier = 2f;
			XDebug.Comment("======== size o/lt ========");
			ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
			sizeOverLifetime.enabled = true;
			AnimationCurve animationCurve = new AnimationCurve();
			animationCurve.AddKey(0f, 0f);
			animationCurve.AddKey(0.5f, 0.8f);
			animationCurve.AddKey(1f, 0f);
			sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, animationCurve);
			XDebug.Comment("========= trails =========");
			ParticleSystem.TrailModule trails = particleSystem.trails;
			trails.enabled = true;
			trails.lifetime = 0.1f;
			XDebug.Comment("========= rend =========");
			ParticleSystemRenderer component = this.StompTornadoFXPrefab.GetComponent<ParticleSystemRenderer>();
			Material material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
			material.mainTexture = XFiles.LoadPNG(Application.dataPath + "/mods/Particle.png");
			component.material = material;
			component.trailMaterial = material;
			Light light = new GameObject("The Light").AddComponent<Light>();
			light.transform.position = new Vector3(999999f, 0f, 0f);
			light.color = Color.blue;
			light.range = 5f;
			light.intensity = 3f;
			ParticleSystem.LightsModule lights = particleSystem.lights;
			lights.enabled = true;
			lights.light = light;
			lights.ratio = 1f;
			lights.sizeAffectsRange = true;
			lights.maxLights = 100;
		}
		ParticleSystem component2 = UnityEngine.Object.Instantiate<GameObject>(this.StompTornadoFXPrefab, where.point + where.normal * 0.1f, Quaternion.FromToRotation(Vector3.up, where.normal)).GetComponent<ParticleSystem>();
		ParticleSystem.MainModule main2 = component2.main;
		main2.loop = false;
		main2.startColor = XEffects.StompTornadoParams.Color * XEffects.StompTornadoParams.Glow;
		component2.Play();
		XDebug.Comment("shouldn't we destroy it later????");
	}

	public void CreateStompCrashFX(RaycastHit whereHit)
	{
		this.CreateStompTornadoFX(whereHit);
		if (this.StompCrashFXPrefab == null)
		{
			this.StompCrashFXPrefab = UnityEngine.Object.Instantiate<GameObject>(XDebug.Instance.SonicNew.SonicEffects.LightAttackFX, Vector3.zero, Quaternion.identity);
			ParticleSystem[] componentsInChildren = this.StompCrashFXPrefab.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i <= 10; i++)
			{
				if (i != 9 && i != 8)
				{
					componentsInChildren[i].enableEmission = false;
					UnityEngine.Object.Destroy(componentsInChildren[i]);
				}
				else
				{
					componentsInChildren[i].startColor = XEffects.StompCrashParams.Color;
					componentsInChildren[i].GetComponent<Renderer>().material.SetColor("_TintColor", XEffects.StompCrashParams.Color);
					componentsInChildren[i].startSize *= 0.45f;
					ParticleSystem.EmissionModule emission = componentsInChildren[i].emission;
					emission.burstCount = 1;
					if (i == 8)
					{
						emission.SetBurst(0, XEffects.StompCrashParams.Burst);
						componentsInChildren[i].startLifetime = XEffects.StompCrashParams.StartLifetime;
					}
					emission.enabled = true;
				}
			}
			this.StompCrashFXPrefab.GetComponent<MonoBehaviour>().enabled = false;
			UnityEngine.Object.Destroy(this.StompCrashFXPrefab.GetComponent<MonoBehaviour>());
			this.StompCrashFXPrefab.GetComponent<AudioSource>().enabled = false;
			UnityEngine.Object.Destroy(this.StompCrashFXPrefab.GetComponent<AudioSource>());
		}
		this.StompCrashFX = UnityEngine.Object.Instantiate<GameObject>(this.StompCrashFXPrefab, whereHit.point + whereHit.normal * XEffects.StompCrashParams.Offset, Quaternion.FromToRotation(Vector3.up, whereHit.normal) * XEffects.StompCrashParams.Rotation);
		ParticleSystem[] componentsInChildren2 = this.StompCrashFX.GetComponentsInChildren<ParticleSystem>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			componentsInChildren2[j].Play();
		}
	}

	public void CreateStompCrashShadowFX(RaycastHit whereHit)
	{
		this.CreateStompTornadoShadowFX(whereHit);
		if (this.StompCrashShadowFXPrefab == null)
		{
			this.StompCrashShadowFXPrefab = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("defaultprefabs/effect/player/sonic/LightAttackFX"), Vector3.zero, Quaternion.identity);
			ParticleSystem[] componentsInChildren = this.StompCrashShadowFXPrefab.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i <= 10; i++)
			{
				if (i != 9 && i != 8)
				{
					componentsInChildren[i].enableEmission = false;
					UnityEngine.Object.Destroy(componentsInChildren[i]);
				}
				else
				{
					componentsInChildren[i].startColor = new Color(1f, 0f, 0f);
					componentsInChildren[i].GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1f, 0f, 0f));
					componentsInChildren[i].startColor = this.DGBCOL2;
					componentsInChildren[i].GetComponent<Renderer>().material.SetColor("_TintColor", this.DGBCOL2);
					componentsInChildren[i].startColor = new Color(1f, 0f, 0f);
					componentsInChildren[i].GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1f, 0f, 0f));
					componentsInChildren[i].startSize *= 0.45f;
					ParticleSystem.EmissionModule emission = componentsInChildren[i].emission;
					emission.burstCount = 1;
					if (i == 8)
					{
						emission.SetBurst(0, XEffects.StompCrashParams.Burst);
						componentsInChildren[i].startLifetime = XEffects.StompCrashParams.StartLifetime;
					}
					emission.enabled = true;
				}
			}
			this.StompCrashShadowFXPrefab.GetComponent<MonoBehaviour>().enabled = false;
			UnityEngine.Object.Destroy(this.StompCrashShadowFXPrefab.GetComponent<MonoBehaviour>());
			this.StompCrashShadowFXPrefab.GetComponent<AudioSource>().enabled = false;
			UnityEngine.Object.Destroy(this.StompCrashShadowFXPrefab.GetComponent<AudioSource>());
		}
		this.StompCrashShadowFX = UnityEngine.Object.Instantiate<GameObject>(this.StompCrashShadowFXPrefab, whereHit.point + whereHit.normal * XEffects.StompCrashParams.Offset, Quaternion.FromToRotation(Vector3.up, whereHit.normal) * XEffects.StompCrashParams.Rotation);
		ParticleSystem[] componentsInChildren2 = this.StompCrashShadowFX.GetComponentsInChildren<ParticleSystem>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			componentsInChildren2[j].Play();
		}
	}

	private void CreateStompTornadoShadowFX(RaycastHit where)
	{
		if (this.StompTornadoShadowFXPrefab == null)
		{
			this.StompTornadoShadowFXPrefab = new GameObject("X_StompTornadoFX");
			ParticleSystem particleSystem = this.StompTornadoShadowFXPrefab.AddComponent<ParticleSystem>();
			XDebug.Comment("========= main =========");
			ParticleSystem.MainModule main = particleSystem.main;
			main.loop = false;
			main.duration = 1f;
			main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 1f);
			main.startSpeed = new ParticleSystem.MinMaxCurve(-0.25f, 2.5f);
			main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
			main.gravityModifier = -0.1f;
			main.gravityModifier = 0f;
			XDebug.Comment("this makes no effect actually: ");
			main.startColor = new Color(255f, 0f, 0f, 1f) * XEffects.StompTornadoParams.Glow / 2f;
			XDebug.Comment("I guess.....");
			XDebug.Comment("========= emiss ========");
			ParticleSystem.EmissionModule emission = particleSystem.emission;
			emission.enabled = true;
			emission.rateOverTime = 0f;
			emission.SetBursts(new ParticleSystem.Burst[]
			{
				new ParticleSystem.Burst(0f, 40)
			});
			XDebug.Comment("========= shape ========");
			ParticleSystem.ShapeModule shape = particleSystem.shape;
			shape.enabled = true;
			shape.shapeType = ParticleSystemShapeType.Cone;
			shape.rotation = new Vector3(-90f, 0f, 0f);
			shape.angle = 15f;
			shape.radius = 2.5f;
			shape.radiusThickness = 0.9f;
			XDebug.Comment("========= vel o/lt ========");
			ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particleSystem.velocityOverLifetime;
			velocityOverLifetime.enabled = true;
			velocityOverLifetime.orbitalX = 0.1f;
			velocityOverLifetime.orbitalY = -5f;
			velocityOverLifetime.orbitalZ = 0.1f;
			velocityOverLifetime.radial = 0.1f;
			velocityOverLifetime.speedModifier = 2f;
			XDebug.Comment("======== size o/lt ========");
			ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
			sizeOverLifetime.enabled = true;
			AnimationCurve animationCurve = new AnimationCurve();
			animationCurve.AddKey(0f, 0f);
			animationCurve.AddKey(0.5f, 0.8f);
			animationCurve.AddKey(1f, 0f);
			sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, animationCurve);
			XDebug.Comment("========= trails =========");
			ParticleSystem.TrailModule trails = particleSystem.trails;
			trails.enabled = true;
			trails.lifetime = 0.1f;
			XDebug.Comment("========= rend =========");
			ParticleSystemRenderer component = this.StompTornadoShadowFXPrefab.GetComponent<ParticleSystemRenderer>();
			Material material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
			material.mainTexture = XFiles.LoadPNG(Application.dataPath + "/mods/Particle.png");
			component.material = material;
			component.trailMaterial = material;
			Light light = new GameObject("The Light").AddComponent<Light>();
			light.transform.position = new Vector3(999999f, 0f, 0f);
			light.color = Color.yellow;
			light.range = 5f;
			light.intensity = 3f;
			ParticleSystem.LightsModule lights = particleSystem.lights;
			lights.enabled = true;
			lights.light = light;
			lights.ratio = 1f;
			lights.sizeAffectsRange = true;
			lights.maxLights = 100;
		}
		ParticleSystem component2 = UnityEngine.Object.Instantiate<GameObject>(this.StompTornadoShadowFXPrefab, where.point + where.normal * 0.1f, Quaternion.FromToRotation(Vector3.up, where.normal)).GetComponent<ParticleSystem>();
		ParticleSystem.MainModule main2 = component2.main;
		main2.loop = false;
		main2.startColor = new Color(2.75f, 0f, 0f, 0.3f);
		main2.startColor = this.DBGCOL1;
		main2.startColor = this.DGBCOL2;
		main2.startColor = new Color(2.75f, 0f, 0f, 0.3f);
		component2.Play();
		XDebug.Comment("shouldn't we destroy it later????");
	}

	public void CreateStompShadowFX()
	{
		if (this.StompShadowFXPrefab == null)
		{
			GameObject gameObject = Resources.Load<GameObject>("defaultprefabs/effect/player/sonic/LightAttackFX");
			this.StompShadowFXPrefab = UnityEngine.Object.Instantiate<GameObject>(gameObject, Vector3.zero, Quaternion.identity);
			XDebug.Comment("!!!!!!!!!!!!!!!!!!!!");
			this.StompShadowFXPrefab.GetComponent<AudioSource>().clip = (XDebug.Instance.SonicNew ? XDebug.Instance.SonicNew.JumpDashKickback : null);
			this.StompShadowFXPrefab.GetComponent<MonoBehaviour>().enabled = false;
			UnityEngine.Object.Destroy(this.StompShadowFXPrefab.GetComponent<MonoBehaviour>());
			ParticleSystem[] componentsInChildren = this.StompShadowFXPrefab.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < 3; i++)
			{
				componentsInChildren[i].loop = true;
				componentsInChildren[i].startColor = new Color(1f, 0f, 0f);
				componentsInChildren[i].GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1f, 0f, 0f));
			}
			for (int j = 3; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].enableEmission = false;
				UnityEngine.Object.Destroy(componentsInChildren[j]);
			}
			this.StompShadowFXPrefab.SetActive(false);
		}
		Transform transform = XDebug.Finder<Shadow>.Instance.transform;
		if (transform == null)
		{
			throw new Exception("no shadow thass bad");
		}
		this.StompShadowFX = UnityEngine.Object.Instantiate<GameObject>(this.StompShadowFXPrefab, transform.position + transform.up * XEffects.StompParams.Offset, transform.rotation * XEffects.StompParams.Rotation);
		this.StompShadowFX.SetActive(true);
		this.StompShadowFX.transform.SetParent(transform);
	}

	public void DestroyStompShadowFX(bool quickMode = false)
	{
		base.StartCoroutine(this.DestroyStompShadowFX_Coroutine(quickMode));
	}

	private IEnumerator DestroyStompShadowFX_Coroutine(bool quickMode)
	{
		GameObject reference = this.StompShadowFX;
		reference.transform.parent = null;
		ParticleSystem[] particleSystems = this.StompShadowFX.GetComponentsInChildren<ParticleSystem>();
		float startTime = Time.time;
		float[] emissionRate = new float[3];
		for (int i = 0; i < 3; i++)
		{
			emissionRate[i] = particleSystems[i].emissionRate;
		}
		Color fadingColor = XEffects.StompParams.ColorShadow;
		float duration = 0.33f;
		while (Time.time - startTime < duration)
		{
			if (quickMode)
			{
				reference.transform.position -= new Vector3(0f, XEffects.StompParams.SinkSpeed * Time.deltaTime, 0f);
			}
			float num = Mathf.Sqrt((Time.time - startTime) / duration);
			for (int j = 0; j < 3; j++)
			{
				fadingColor.a = Mathf.Lerp(XEffects.StompParams.ColorShadow.a, 0f, Mathf.Sqrt(num));
				particleSystems[j].startColor = fadingColor;
				particleSystems[j].emissionRate = Mathf.Lerp(emissionRate[j], emissionRate[j] / 2f, num);
			}
			yield return null;
		}
		UnityEngine.Object.Destroy(reference);
		yield break;
	}

	public void CreateSecondJumpFX()
	{
		if (this.SecondJumpFXPrefab == null)
		{
			this.SecondJumpFXPrefab = Resources.Load<GameObject>("defaultprefabs/effect/player/amy/DoubleJumpFX");
		}
		Transform transform = XDebug.Finder<SonicFast>.Instance.transform;
		GameObject gameObject;
		if (XDebug.Cfg.MachSpeedSecondJump.HueShift < 0f)
		{
			gameObject = UnityEngine.Object.Instantiate<GameObject>(this.SecondJumpFXPrefab, transform);
		}
		else
		{
			gameObject = UnityEngine.Object.Instantiate<GameObject>(this.SecondJumpFXPrefab, transform.position, Quaternion.identity);
		}
		gameObject.transform.position += -Vector3.up * XDebug.Cfg.MachSpeedSecondJump.Offset;
		ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			float num;
			float num2;
			float num3;
			Color.RGBToHSV(componentsInChildren[i].startColor, out num, out num2, out num3);
			num = ((num + XDebug.Cfg.MachSpeedSecondJump.HueShift) % 1f + 1f) % 1f;
			Color color = Color.HSVToRGB(num, num2, num3);
			componentsInChildren[i].startColor = color;
			componentsInChildren[i].playbackSpeed = XDebug.Cfg.MachSpeedSecondJump.FXPlaybackScale;
		}
		componentsInChildren[0].enableEmission = false;
		componentsInChildren[1].enableEmission = false;
		UnityEngine.Object.Destroy(gameObject, 2.5f);
	}

	private GameObject DodgeFX;

	private GameObject StompFX;

	private GameObject StompFXPrefab;

	private GameObject StompTornadoFXPrefab;

	private GameObject StompCrashFXPrefab;

	private GameObject StompCrashFX;

	private Color DBGCOL1 = new Color(0.9f, 0f, 0f, 1f);

	private Color DGBCOL2 = new Color(1f, 0.71f, 0.18f, 0.5f);

	private GameObject StompCrashShadowFXPrefab;

	private GameObject StompCrashShadowFX;

	private GameObject StompTornadoShadowFXPrefab;

	private GameObject StompShadowFXPrefab;

	private GameObject StompShadowFX;

	private GameObject SecondJumpFXPrefab;

	private struct DodgeParams
	{
		public static float Rate = 50f;

		public static int Burst = 100;

		public static float Glow = 2.75f;

		public static Color Color = new Color(0.1529412f, 0.2f, 1f, 1f);

		public static Color ColorSuper = new Color(0.921f, 0.686f, 0.0352f);

		public static Color ColorShadow = new Color(1f, 0.71f, 0.18f, 0.12f);

		public static Color ColorShadow2 = new Color(1f, 0.529f, 0.125f, 0.125f);
	}

	private struct StompParams
	{
		public static float Offset = 0.65f;

		public static Quaternion Rotation = Quaternion.Euler(90f, 0f, 0f);

		public static Color Color = new Color(0f, 23f, 255f, 0.3f);

		public static Color TintColor = new Color(0f, 23f, 255f, 0.004f);

		public static float SinkSpeed = 22f;

		public static Color ColorShadow = new Color(1f, 0f, 0f, 0.3f);
	}

	private struct StompTornadoParams
	{
		public static Color Color = new Color(0.1529412f, 0.2f, 1f, 1f);

		public static float Glow = 2.75f;
	}

	public struct StompCrashParams
	{
		public static float Offset = 1.8f;

		public static Quaternion Rotation = Quaternion.Euler(90f, 0f, 0f);

		public static Color Color = new Color(0f, 23f, 255f, 0.05f);

		public static ParticleSystem.Burst Burst = new ParticleSystem.Burst(0f, 1, 1, 2, 0.1f);

		public static float StartLifetime = 0.4f;
	}
}
