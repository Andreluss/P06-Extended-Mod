using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XUISection
{
	public XUISection(string name = "New P06X Section")
	{
		this.gameObject = DefaultControls.CreatePanel(default(DefaultControls.Resources));
		this.gameObject.name = "section: " + name;
		this.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(XUIConfig.item_width, 0f);
		VerticalLayoutGroup verticalLayoutGroup = this.gameObject.AddComponent<VerticalLayoutGroup>();
		verticalLayoutGroup.spacing = XUIConfig.section_spacing;
		verticalLayoutGroup.padding.bottom = (int)XUIConfig.section_spacing;
		verticalLayoutGroup.childControlHeight = false;
		TextMeshProUGUI component = TMP_DefaultControls.CreateText(default(TMP_DefaultControls.Resources)).GetComponent<TextMeshProUGUI>();
		this.TitleText = component;
		RectTransform component2 = DefaultControls.CreatePanel(default(DefaultControls.Resources)).GetComponent<RectTransform>();
		component2.name = "title panel";
		component2.sizeDelta = new Vector2(XUIConfig.item_width, XUIConfig.title_item_height);
		component2.transform.SetParent(this.gameObject.transform, false);
		component.transform.SetParent(component2.transform, false);
		RectTransform component3 = component.GetComponent<RectTransform>();
		component3.offsetMin = Vector2.zero;
		component3.offsetMax = Vector2.zero;
		component3.anchorMin = new Vector2(0f, 0f);
		component3.anchorMax = new Vector2(1f, 1f);
		component.text = name;
		component.margin = Vector4.one * XUIConfig.title_text_margin;
		component.alignment = TextAlignmentOptions.CenterGeoAligned;
		component.enableAutoSizing = true;
		Button button = component2.gameObject.AddComponent<Button>();
		button.transition = Selectable.Transition.None;
		button.onClick.AddListener(delegate()
		{
			this.Toggle();
		});
		this.items = new List<XUIItem>();
	}

	public XUISection AddItem(XUIItem item)
	{
		item.gameObject.transform.SetParent(this.gameObject.transform, false);
		this.items.Add(item);
		return this;
	}

	public List<XUIItem> Items
	{
		get
		{
			return this.items;
		}
	}

	public void Toggle()
	{
		this.Toggle(this.IsCollapsed);
	}

	public bool IsCollapsed
	{
		get
		{
			return this._isCollapsed;
		}
		set
		{
			this._isCollapsed = value;
			Transform transform = this.gameObject.transform;
			int childCount = transform.childCount;
			for (int i = 1; i < childCount; i++)
			{
				transform.GetChild(i).gameObject.SetActive(!this._isCollapsed);
			}
			if (!this._isCollapsed)
			{
				if (this.TitleText.text.Length > 3 && this.TitleText.text.Substring(this.TitleText.text.Length - 3, 3) == "  +")
				{
					this.TitleText.text = this.TitleText.text.Substring(0, this.TitleText.text.Length - 3);
					return;
				}
			}
			else
			{
				TextMeshProUGUI titleText = this.TitleText;
				titleText.text += "  +";
			}
		}
	}

	public void Toggle(bool Expand)
	{
		this.IsCollapsed = !Expand;
	}

	public GameObject gameObject;

	private readonly List<XUIItem> items;

	private TextMeshProUGUI TitleText;

	private bool _isCollapsed;
}
