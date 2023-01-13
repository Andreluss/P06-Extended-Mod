using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XUIFunctionButton : XUIItem
{
	private static Color ColorFromHex(int hex)
	{
		return new Color((float)((hex & 16711680) >> 16) / 255f, (float)((hex & 65280) >> 8) / 255f, (float)(hex & 255) / 255f);
	}

	public void BuildVisuals(string name)
	{
		this.gameObject.name = "function button: " + name;
		Vector2 sizeDelta = this.gameObject.GetComponent<RectTransform>().sizeDelta;
		this.gameObject.GetComponent<Image>().enabled = false;
		GameObject gameObject = TMP_DefaultControls.CreateButton(default(TMP_DefaultControls.Resources));
		gameObject.transform.SetParent(this.gameObject.transform, false);
		gameObject.GetComponentInChildren<TextMeshProUGUI>().color = XUIFunctionButton.ColorFromHex(4050943);
		gameObject.GetComponentInChildren<TextMeshProUGUI>().text = name;
		RectTransform component = gameObject.GetComponent<RectTransform>();
		component.pivot = new Vector2(0.5f, 0.5f);
		component.anchorMin = new Vector2(0.5f, 0.5f);
		component.anchorMax = new Vector2(0.5f, 0.5f);
		component.sizeDelta = new Vector2(sizeDelta.x - 10f, XUIConfig.item_height);
		Color color = new Color(0.24f, 0.24f, 0.24f, 1f);
		ColorBlock colorBlock = default(ColorBlock);
		Color color2 = new Color(0.1f, 0.1f, 0.1f, 0f);
		colorBlock.pressedColor = color + color2;
		colorBlock.highlightedColor = colorBlock.pressedColor;
		colorBlock.selectedColor = colorBlock.pressedColor;
		colorBlock.normalColor = color;
		colorBlock.colorMultiplier = 1f;
		colorBlock.fadeDuration = 0.09f;
		this.button = gameObject.GetComponent<Button>();
		this.button.colors = colorBlock;
	}

	public XUIFunctionButton(string name, Action action)
	{
		this.BuildVisuals(name);
		this.button.onClick.AddListener(delegate()
		{
			action();
		});
		base.Name = name;
		this.Action = action;
	}

	private Button button;

	public readonly Action Action;
}
