using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XUIToggleButton : XUIItem
{
	private static Color ColorFromHex(int hex)
	{
		return new Color((float)((hex & 16711680) >> 16) / 255f, (float)((hex & 65280) >> 8) / 255f, (float)(hex & 255) / 255f);
	}

	public bool State
	{
		get
		{
			return this.state;
		}
		set
		{
			this.actualButton.GetComponentInChildren<TextMeshProUGUI>().text = (value ? "<color=#00ee00>ENABLED</color>" : "<color=#ee0000>DISABLED</color>");
			this.state = value;
		}
	}

	public void BuildVisuals(string name)
	{
		this.gameObject.name = "toggle button: " + name;
		Vector2 sizeDelta = this.gameObject.GetComponent<RectTransform>().sizeDelta;
		GameObject gameObject = TMP_DefaultControls.CreateText(default(TMP_DefaultControls.Resources));
		gameObject.transform.SetParent(this.gameObject.transform, false);
		TextMeshProUGUI component = gameObject.GetComponent<TextMeshProUGUI>();
		component.enableAutoSizing = true;
		component.fontSizeMin = 8f;
		component.text = name;
		component.margin = Vector4.one * 6f;
		component.alignment = TextAlignmentOptions.Center;
		component.enableWordWrapping = false;
		component.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x / 2f, XUIConfig.item_height);
		RectTransform component2 = gameObject.GetComponent<RectTransform>();
		component2.pivot = new Vector2(0f, 0.5f);
		component2.sizeDelta = new Vector2(sizeDelta.x * 0.5f, component2.sizeDelta.y);
		RectTransform rectTransform = component2;
		RectTransform rectTransform2 = component2;
		Vector2 vector = new Vector2(0f, 0.5f);
		rectTransform2.anchorMax = vector;
		rectTransform.anchorMin = vector;
		GameObject gameObject2 = TMP_DefaultControls.CreateButton(default(TMP_DefaultControls.Resources));
		gameObject2.transform.SetParent(this.gameObject.transform, false);
		RectTransform component3 = gameObject2.GetComponent<RectTransform>();
		component3.pivot = new Vector2(1f, 0.5f);
		component3.sizeDelta = new Vector2(sizeDelta.x * 0.5f, 0f);
		component3.anchorMin = new Vector2(1f, 0f);
		component3.anchorMax = new Vector2(1f, 1f);
		Color color = new Color(0.24f, 0.24f, 0.24f, 1f);
		ColorBlock colorBlock = default(ColorBlock);
		Color color2 = new Color(0.1f, 0.1f, 0.1f, 0f);
		colorBlock.pressedColor = color + color2;
		colorBlock.highlightedColor = colorBlock.pressedColor;
		colorBlock.selectedColor = colorBlock.pressedColor;
		colorBlock.normalColor = color;
		colorBlock.colorMultiplier = 1f;
		colorBlock.fadeDuration = 0.09f;
		this.actualButton = gameObject2.GetComponent<Button>();
		this.actualButton.colors = colorBlock;
		this.button = this.actualButton;
	}

	public XUIToggleButton(string name, XValue<bool> BindTo)
	{
		XUIToggleButton <>4__this = this;
		this.BindedXValue = BindTo;
		this.BuildVisuals(name);
		base.Name = name;
		this.State = BindTo.Value;
		this.button.onClick.AddListener(delegate()
		{
			BindTo.Value = !BindTo.Value;
		});
		BindTo.OnChangeValue += delegate(bool to)
		{
			<>4__this.State = to;
		};
	}

	private Button actualButton;

	private bool state;

	public Button button;

	public readonly XValue<bool> BindedXValue;
}
