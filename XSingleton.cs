using System;
using UnityEngine;

public class XSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	public static T Instance
	{
		get
		{
			if (XSingleton<T>._instance == null)
			{
				XSingleton<T>._instance = UnityEngine.Object.FindObjectOfType<T>();
				if (XSingleton<T>._instance == null)
				{
					XSingleton<T>._instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
				}
			}
			return XSingleton<T>._instance;
		}
	}

	private void Start()
	{
		if (this != XSingleton<T>._instance)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
	}

	private static T _instance;
}
