using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;

public class XFiles : MonoBehaviour
{
	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	public static XFiles Instance
	{
		get
		{
			if (XFiles._instance == null)
			{
				XFiles._instance = UnityEngine.Object.FindObjectOfType<XFiles>();
				if (XFiles._instance == null)
				{
					XFiles._instance = new GameObject("XFiles").AddComponent<XFiles>();
				}
			}
			return XFiles._instance;
		}
	}

	public void Load()
	{
		base.StartCoroutine(this.LoadAll_C());
	}

	private void Start()
	{
		if (XFiles._instance != this)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
	}

	public bool Check()
	{
		if (!Directory.Exists(this.Mods))
		{
			Directory.CreateDirectory(this.Mods);
		}
		string text = this.Mods + "xconfig.ini";
		if (!File.Exists(text))
		{
			File.WriteAllLines(text, new string[]
			{
				"0",
				"P06X" + XDebug.P06X_VERSION
			});
		}
		long num = 0L;
		try
		{
			string[] files = Directory.GetFiles(Application.dataPath, "*.resource");
			for (int i = 0; i < files.Length; i++)
			{
				num += new FileInfo(files[i]).Length;
			}
		}
		catch
		{
			XDebug.Instance.Log("There's a problem with checking your P-06 version - couldn't access the file!", 100f, 14f);
			return false;
		}
		if (num != XFiles.VersionHash && XDebug.Instance.Other_CheckP06Version.Value)
		{
			XDebug.Instance.Log("P-06 eXtended " + XDebug.P06X_VERSION + " <color=#ee0000>version error</color>(most likely)\n This mod release isn't comp. with your P-06 ver.", 100f, 12.5f);
			XDebug.Comment("\n the game <color=#ee0000>may crash</color>");
			File.WriteAllLines(text, new string[]
			{
				"0",
				string.Concat(new object[]
				{
					"P06X",
					XDebug.P06X_VERSION,
					" version error at ",
					DateTime.Now
				})
			});
			return false;
		}
		XDebug.Comment("Always CheckModFiles()");
		string text2 = null;
		try
		{
			text2 = File.ReadAllLines(text)[0];
		}
		catch (Exception ex)
		{
			if (text2 == null)
			{
				XDebug.Instance.Log("P-06 eXtended " + XDebug.P06X_VERSION + " <color=#ee0000>critical file error</color>\nexception" + ex.Message, 100f, 14f);
				return false;
			}
		}
		string text3 = null;
		foreach (string text4 in this.Required)
		{
			if (!File.Exists(Application.dataPath + "/mods/" + text4))
			{
				text3 = text4;
				break;
			}
		}
		XDebug.Comment("1 -> 1");
		if (text2 == "1" && text3 == null)
		{
			XDebug.Instance.Log("P-06<color=#00ee00>X</color>" + XDebug.P06X_VERSION + " by 4ndrelus for Version 4.6", 3f, 13.8f);
		}
		XDebug.Comment("0 -> 1");
		if (text2 == "0" && text3 == null)
		{
			File.WriteAllLines(text, new string[]
			{
				"1",
				string.Concat(new object[]
				{
					"P06X",
					XDebug.P06X_VERSION,
					" installed ",
					DateTime.Now
				})
			});
			XDebug.Instance.Log("P-06 eXtended " + XDebug.P06X_VERSION + " <color=#00ee00>installed correctly</color>", 60f, 14f);
			XDebug.Instance.LogExtra("Thanks for using the mod!\n\n1) Press F12 to open the Mod Menu\n\n2) Have fun :D", 60f, 14f);
		}
		XDebug.Comment("0/1 -> 0");
		if (text3 != null)
		{
			File.WriteAllLines(text, new string[]
			{
				"0",
				string.Concat(new object[]
				{
					"P06X",
					XDebug.P06X_VERSION,
					" error at ",
					DateTime.Now
				})
			});
			XDebug.Instance.Log(string.Concat(new string[]
			{
				"P-06 eXtended ",
				XDebug.P06X_VERSION,
				" <color=#ee0000>files error</color>\n",
				text3,
				" missing in the mods folder!"
			}), 100f, 14f);
			return false;
		}
		try
		{
			this.ConfigFile = File.ReadAllLines(text);
		}
		catch (Exception ex2)
		{
			XDebug.Instance.Log("P-06 eXtended " + XDebug.P06X_VERSION + " <color=#ee0000>weird file error</color>\nexception" + ex2.Message, 100f, 14f);
			return false;
		}
		return true;
	}

	public static Texture2D LoadPNG(string filePath)
	{
		Texture2D texture2D = null;
		if (File.Exists(filePath))
		{
			byte[] array = File.ReadAllBytes(filePath);
			texture2D = new Texture2D(2, 2);
			texture2D.LoadImage(array);
		}
		return texture2D;
	}

	private IEnumerator LoadOGG2(Action<AudioClip> action, string filename)
	{
		using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(this.Mods + filename, AudioType.OGGVORBIS))
		{
			yield return www.SendWebRequest();
			action(DownloadHandlerAudioClip.GetContent(www));
		}
		UnityWebRequest www = null;
		yield break;
		yield break;
	}

	public XFiles()
	{
		this.Mods = Application.dataPath + "/mods/";
		this.Required = new string[] { "Particle.png", "custom_music.ogg", "stomp_land.ogg", "ring.ogg" };
	}

	private IEnumerator LoadAll_C()
	{
		this.Particle = XFiles.LoadPNG(this.Mods + "Particle.png");
		yield return base.StartCoroutine(this.LoadOGG2(delegate(AudioClip clip)
		{
			this.Gandalf = clip;
		}, "custom_music.ogg"));
		yield return base.StartCoroutine(this.LoadOGG2(delegate(AudioClip clip)
		{
			this.StompLand = clip;
		}, "stomp_land.ogg"));
		yield return base.StartCoroutine(this.LoadOGG2(delegate(AudioClip clip)
		{
			this.RingSound = clip;
		}, "ring.ogg"));
		yield return base.StartCoroutine(this.LoadOGG2(delegate(AudioClip clip)
		{
			this.WallLand = clip;
		}, "land_concrete.ogg"));
		if (XDebug.COMMENT)
		{
			XDebug.Instance.Log("All P06<color=#00ee00>X</color>" + XDebug.P06X_VERSION + " files <color=#00ee00>loaded correctly</color>", 1f, 14f);
		}
		yield break;
	}

	public IEnumerator LoadMp3(Action<AudioClip> action, string filename)
	{
		XDebug.Comment("THIS IS BROKEN currently");
		string text = string.Format("file://{0}", this.Mods + filename);
		WWW www = new WWW(text);
		yield return www;
		AudioClip audioClip = www.GetAudioClip(false, false, AudioType.MPEG);
		action(audioClip);
		yield break;
	}

	private static string CalculateMD5(string filename)
	{
		string text;
		using (MD5 md = MD5.Create())
		{
			using (FileStream fileStream = File.OpenRead(filename))
			{
				text = BitConverter.ToString(md.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();
			}
		}
		return text;
	}

	public string Mods;

	public string[] Required;

	private static XFiles _instance;

	public AudioClip Gandalf;

	public Texture2D Particle;

	public AudioClip StompLand;

	public static long VersionCode;

	public string[] ConfigFile;

	public AudioClip RingSound;

	public AudioClip WallLand;

	public static long VersionHash = 564254852L;
}
