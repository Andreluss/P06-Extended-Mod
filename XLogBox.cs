using System;
using TMPro;
using UnityEngine;

public class XLogBox
{
	public Vector2 Size
	{
		get
		{
			return this._size;
		}
		set
		{
			this._size = value;
			this.Container_Rect.sizeDelta = value;
		}
	}

	public Vector2 InnerSize
	{
		get
		{
			return this._innerSize;
		}
		set
		{
			this._innerSize = value;
			this.Text_Rect.sizeDelta = value;
		}
	}

	public float Font
	{
		get
		{
			return this._font;
		}
		set
		{
			if (value <= 0f)
			{
				this.Text.enableAutoSizing = true;
			}
			else
			{
				this.Text.enableAutoSizing = false;
				this.Text.fontSize = value;
			}
			this._font = value;
		}
	}

	public Vector3 Position
	{
		get
		{
			return this._position;
		}
		set
		{
			this._position = value;
			this.Container_Rect.localPosition = value;
		}
	}

	private void Update()
	{
		if (this.GameObject.activeSelf && Time.time > this.HideTime)
		{
			this.GameObject.SetActive(false);
		}
	}

	public XLogBox(Vector2 size, Vector2 innerSize, Vector2 anchor, Vector2 position)
	{
		this.GameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Defaultprefabs/UI/MessageBox_E3") as GameObject, Vector3.zero, Quaternion.identity);
		this.GameObject_Rect = this.GameObject.GetComponent<RectTransform>();
		this.GameObject.GetComponent<MessageBox>().Duration = 9999999f;
		this.GameObject.transform.SetParent(XDebug.Instance.XCanvas.transform, false);
		this.Text = this.GameObject.GetComponentInChildren<TextMeshProUGUI>();
		this.Text.overflowMode = TextOverflowModes.Linked;
		this.Text.alignment = TextAlignmentOptions.Center;
		this.Text.enableAutoSizing = true;
		this.Text.fontSizeMin = 2f;
		this.Text_Rect = this.Text.GetComponent<RectTransform>();
		this.Text_Rect.anchorMin = new Vector2(0.5f, 0.5f);
		this.Text_Rect.anchorMax = new Vector2(0.5f, 0.5f);
		this.Text_Rect.pivot = new Vector2(0.5f, 0.5f);
		this.Text_Rect.localPosition = Vector3.zero;
		this.Text_Rect.sizeDelta = innerSize;
		this.GameObject_Rect.anchorMin = anchor;
		this.GameObject_Rect.anchorMax = anchor;
		this.Container_Rect = this.GameObject.FindInChildren("Box").GetComponent<RectTransform>();
		this.Container_Rect.anchorMin = anchor;
		this.Container_Rect.anchorMax = anchor;
		this.Container_Rect.pivot = anchor;
		this.Container_Rect.sizeDelta = size;
		this.Container_Rect.localPosition = position;
	}

	public Vector2 Anchor
	{
		get
		{
			return this._anchor;
		}
		set
		{
			this._anchor = value;
			this.GameObject_Rect.anchorMin = value;
			this.GameObject_Rect.anchorMax = value;
			this.Container_Rect.anchorMin = value;
			this.Container_Rect.anchorMin = value;
			this.Container_Rect.pivot = value;
		}
	}

	public GameObject GameObject;

	public TextMeshProUGUI Text;

	public RectTransform Text_Rect;

	public RectTransform GameObject_Rect;

	public RectTransform Container_Rect;

	private Vector2 _size = new Vector2(175f, 45f);

	private Vector2 _innerSize = new Vector2(150f, 30f);

	private float _font;

	private Vector3 _position = new Vector3(0f, 100f, 0f);

	public float HideTime;

	private Vector2 _anchor = new Vector2(1f, 0f);
}
