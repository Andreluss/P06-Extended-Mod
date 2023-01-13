using System;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public static class XUtility
{
	public static void Invoke(this MonoBehaviour mb, Action f, float delay)
	{
		mb.StartCoroutine(XUtility.InvokeRoutine(f, delay));
	}

	private static IEnumerator InvokeRoutine(Action f, float delay)
	{
		yield return new WaitForSeconds(delay);
		f();
		yield break;
	}

	public static void SerializeObjectToXml<T>(T obj, string path)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
		using (StreamWriter streamWriter = new StreamWriter(path))
		{
			xmlSerializer.Serialize(streamWriter, obj);
		}
	}

	public static T DeserializeXmlToObject<T>(string path)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
		T t;
		using (StreamReader streamReader = new StreamReader(path))
		{
			t = (T)((object)xmlSerializer.Deserialize(streamReader));
		}
		return t;
	}

	public static bool IsIn<T>(this T value, params T[] set)
	{
		foreach (T t in set)
		{
			if (value.Equals(t))
			{
				return true;
			}
		}
		return false;
	}
}
