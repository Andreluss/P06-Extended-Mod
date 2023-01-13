using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class XUIMenu
{
	public List<XUISection> Sections
	{
		get
		{
			return this.sections;
		}
	}

	public XUISection AddSection(XUISection section)
	{
		section.gameObject.transform.SetParent(this.container.transform, false);
		this.sections.Add(section);
		return section;
	}

	public XUIMenu(string title, KeyCode toggleKey)
	{
		if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
		{
			UnityEngine.Object.DontDestroyOnLoad(new GameObject("EventSystem").AddComponent<EventSystem>().gameObject.AddComponent<StandaloneInputModule>());
		}
		this.gameObject = DefaultControls.CreateScrollView(default(DefaultControls.Resources));
		this.gameObject.transform.SetParent(XDebug.Instance.XCanvas.transform, false);
		RectTransform component = this.gameObject.GetComponent<RectTransform>();
		ScrollRect component2 = this.gameObject.GetComponent<ScrollRect>();
		component.pivot = XUIConfig.menu_pivot;
		component.anchorMin = XUIConfig.menu_anchor_min;
		component.anchorMax = XUIConfig.menu_anchor_max;
		component.sizeDelta = XUIConfig.menu_size;
		component.anchoredPosition = XUIConfig.menu_pos;
		component2.scrollSensitivity = 40f;
		component2.movementType = ScrollRect.MovementType.Clamped;
		component2.horizontal = false;
		component2.verticalScrollbar.handleRect.sizeDelta = new Vector2(10f, 1f);
		this.container = this.gameObject.transform.GetChild(0).GetChild(0).gameObject;
		RectTransform component3 = this.container.GetComponent<RectTransform>();
		component3.anchorMin = new Vector2(0f, 0f);
		component3.anchorMax = new Vector2(1f, 1f);
		ContentSizeFitter contentSizeFitter = this.container.AddComponent<ContentSizeFitter>();
		VerticalLayoutGroup verticalLayoutGroup = this.container.AddComponent<VerticalLayoutGroup>();
		contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
		verticalLayoutGroup.childControlWidth = false;
		verticalLayoutGroup.padding = new RectOffset(XUIConfig.menu_padding, XUIConfig.menu_padding, XUIConfig.menu_padding, XUIConfig.menu_padding);
		verticalLayoutGroup.spacing = XUIConfig.menu_spacing;
		this.sections = new List<XUISection>();
		Image component4 = this.gameObject.transform.GetChild(2).GetComponent<Image>();
		component4.enabled = false;
		component4.transform.GetChild(0).GetChild(0).GetComponent<Image>()
			.color = Color.white;
		XDebug.Comment("new Color(0.4283019f, 0.7813739f, 1f, 0.5f);");
		XDebug.Instance.XCanvas.gameObject.AddComponent<XUIMenu.XMenuController>().Init(this, toggleKey);
	}

	public event Action OnClose;

	public event Action OnOpen;

	private readonly GameObject container;

	private readonly List<XUISection> sections;

	public readonly GameObject gameObject;

	public class XMenuController : MonoBehaviour
	{
		private void Update()
		{
			if (Input.GetKeyDown(this.KeyToggle))
			{
				Cursor.visible = !this.menu.gameObject.activeSelf;
				this.menu.gameObject.SetActive(!this.menu.gameObject.activeSelf);
				if (this.menu.gameObject.activeSelf && this.menu.OnOpen != null)
				{
					this.menu.OnOpen();
				}
				if (!this.menu.gameObject.activeSelf && this.menu.OnClose != null)
				{
					this.menu.OnClose();
				}
			}
		}

		public void Init(XUIMenu menu, KeyCode KeyToggle)
		{
			this.menu = menu;
			this.KeyToggle = KeyToggle;
			menu.gameObject.SetActive(false);
		}

		public XUIMenu menu;

		public KeyCode KeyToggle;
	}
}
