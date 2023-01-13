using System;
using UnityEngine;
using UnityEngine.UI;

public class XUIItem
{
	public XUIItem()
	{
		this.gameObject = DefaultControls.CreatePanel(default(DefaultControls.Resources));
		this.gameObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.25f);
		this.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(XUIConfig.item_width, XUIConfig.item_height);
	}

	public string Name { get; set; }

	public GameObject gameObject;
}
