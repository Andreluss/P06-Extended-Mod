using System;

public class XValue<T>
{
	public XValue(Action<T> action, T initValue = default(T))
	{
		this.ChangeValue = action;
		this.Value = initValue;
	}

	public event Action<T> OnChangeValue;

	public T Value
	{
		get
		{
			return this._value;
		}
		set
		{
			this._value = value;
			this.ChangeValue(value);
			Action<T> onChangeValue = this.OnChangeValue;
			if (onChangeValue == null)
			{
				return;
			}
			onChangeValue(value);
		}
	}

	private Action<T> ChangeValue;

	private T _value;
}
