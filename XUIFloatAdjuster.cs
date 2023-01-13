using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XUIFloatAdjuster : XUIItem
{
	public float Value
	{
		get
		{
			return this._value;
		}
		set
		{
			this.inputField.text = value.ToString(this.format);
			this._value = value;
		}
	}

	public void BuildVisuals(string name)
	{
		this.gameObject.name = "float adjuster: " + name;
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
		GameObject gameObject2 = TMP_DefaultControls.CreateInputField(default(TMP_DefaultControls.Resources));
		gameObject2.transform.SetParent(this.gameObject.transform, false);
		RectTransform component3 = gameObject2.GetComponent<RectTransform>();
		component3.pivot = new Vector2(1f, 0.5f);
		component3.sizeDelta = new Vector2(sizeDelta.x * 0.5f, 0f);
		component3.anchorMin = new Vector2(1f, 0f);
		component3.anchorMax = new Vector2(1f, 1f);
		Color color = new Color(0.24f, 0.24f, 0.24f, 1f);
		ColorBlock colorBlock = default(ColorBlock);
		Color color2 = new Color(0.1f, 0.1f, 0.1f, 0f);
		colorBlock.pressedColor = color + color2 / 2f;
		colorBlock.highlightedColor = colorBlock.pressedColor;
		colorBlock.selectedColor = colorBlock.pressedColor;
		colorBlock.normalColor = color;
		colorBlock.colorMultiplier = 1f;
		colorBlock.fadeDuration = 0.09f;
		this.inputField = gameObject2.GetComponent<TMP_InputField>();
		this.inputField.textComponent.alignment = TextAlignmentOptions.Center;
		this.inputField.textComponent.color = new Color(1f, 0.6039216f, 0f, 1f);
		this.inputField.colors = colorBlock;
		this.inputField.selectionColor = Color.black * 0.8f;
		this.inputField.characterValidation = TMP_InputField.CharacterValidation.Decimal;
		GameObject gameObject3 = TMP_DefaultControls.CreateButton(default(TMP_DefaultControls.Resources));
		gameObject3.transform.SetParent(this.inputField.transform, false);
		RectTransform component4 = gameObject3.GetComponent<RectTransform>();
		Vector2 vector2 = new Vector2(0f, 0.5f);
		component4.anchorMax = vector2;
		component4.anchorMin = vector2;
		component4.pivot = new Vector2(0f, 0.5f);
		component4.anchoredPosition = new Vector2(0f, 0f);
		component4.sizeDelta = new Vector2(XUIConfig.item_width * 0.5f * 0.2f, XUIConfig.item_height);
		component4.position += new Vector3(0f, 0f, -3f);
		TextMeshProUGUI componentInChildren = gameObject3.GetComponentInChildren<TextMeshProUGUI>();
		componentInChildren.color = Color.white;
		componentInChildren.text = "<";
		this.L = gameObject3.GetComponent<Button>();
		GameObject gameObject4 = TMP_DefaultControls.CreateButton(default(TMP_DefaultControls.Resources));
		gameObject4.transform.SetParent(this.inputField.transform, false);
		RectTransform component5 = gameObject4.GetComponent<RectTransform>();
		vector2 = new Vector2(1f, 0.5f);
		component5.anchorMax = vector2;
		component5.anchorMin = vector2;
		component5.pivot = new Vector2(1f, 0.5f);
		component5.anchoredPosition = new Vector2(0f, 0f);
		component5.sizeDelta = new Vector2(XUIConfig.item_width * 0.5f * 0.2f, XUIConfig.item_height);
		component5.position += new Vector3(0f, 0f, -3f);
		TextMeshProUGUI componentInChildren2 = gameObject4.GetComponentInChildren<TextMeshProUGUI>();
		componentInChildren2.color = Color.white;
		componentInChildren2.text = ">";
		this.R = gameObject4.GetComponent<Button>();
		ColorBlock colorBlock2 = default(ColorBlock);
		colorBlock2.normalColor = Color.black * 0f;
		colorBlock2.highlightedColor = (colorBlock2.pressedColor = (colorBlock2.selectedColor = colorBlock.highlightedColor));
		colorBlock2.fadeDuration = 0f;
		colorBlock2.colorMultiplier = 0f;
		this.L.colors = colorBlock2;
		this.R.colors = colorBlock2;
	}

	public XUIFloatAdjuster(string name, XValue<float> BindTo, float stepL = -0.25f, float stepR = 0.5f, int precision = 3)
		: this(name, BindTo, stepL, stepR, precision, float.NegativeInfinity, float.PositiveInfinity)
	{
	}

	public XUIFloatAdjuster(string name, XValue<float> BindTo, float stepL = -0.25f, float stepR = 0.5f, int precision = 3, float minValue = float.NegativeInfinity, float maxValue = float.PositiveInfinity)
	{
		XUIFloatAdjuster reference = this;
		this.MinValue = minValue;
		this.MaxValue = maxValue;
		this.BindedXValue = BindTo;
		this.format = "0." + new string('0', precision);
		this.BuildVisuals(name);
		base.Name = name;
		this.Value = BindTo.Value;
		this.inputField.onSubmit.AddListener(delegate(string s)
		{
			reference.ApplyChanges = true;
			XDebug.Instance.Invoke(delegate
			{
				reference.inputField.interactable = false;
				reference.inputField.interactable = true;
			}, 0f);
		});
		this.inputField.onEndEdit.AddListener(delegate(string s)
		{
			if (!reference.ApplyChanges || string.IsNullOrEmpty(s))
			{
				reference.Value = BindTo.Value;
			}
			else
			{
				BindTo.Value = reference.Clamped(float.Parse(s.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat));
			}
			reference.ApplyChanges = false;
		});
		this.L.onClick.AddListener(delegate()
		{
			BindTo.Value = reference.Clamped(BindTo.Value + stepL);
		});
		this.R.onClick.AddListener(delegate()
		{
			BindTo.Value = reference.Clamped(BindTo.Value + stepR);
		});
		BindTo.OnChangeValue += delegate(float newValue)
		{
			reference.Value = newValue;
		};
	}

	private float Clamped(float value)
	{
		int num = this.format.Length - 2;
		float num2 = Mathf.Pow(10f, (float)num);
		return Mathf.Round(Mathf.Clamp(value, this.MinValue, this.MaxValue) * num2) / num2;
	}

	private string format;

	private bool ApplyChanges;

	private Button L;

	private Button R;

	private TMP_InputField inputField;

	private float _value;

	public readonly XValue<float> BindedXValue;

	private float MinValue = float.NegativeInfinity;

	private float MaxValue = float.PositiveInfinity;
}
