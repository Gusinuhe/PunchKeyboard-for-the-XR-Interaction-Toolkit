using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Key : MonoBehaviour
{
	[Serializable]
	public class KeyEvent : UnityEvent<Key> {}

	[SerializeField] 
	private char keyCap;

	[SerializeField] 
	private char alternateKeyCap;

	[SerializeField] 
	private KeyCode keyCode;
	
	[SerializeField] 
	private Color pressedKeyCapColor;
	
	private Color originalKeyCapColor;
	
	public KeyEvent OnKeyPressed = new KeyEvent();

	public KeyCode KeyCode => keyCode;

	public string KeyCap
	{
		get
		{
			if (symbolSwitch)
				return uppercaseSwitch ? $"{alternateKeyCap}".ToLower() : $"{alternateKeyCap}".ToUpper();
			
			return uppercaseSwitch ? $"{keyCap}".ToLower() : $"{keyCap}".ToUpper();
		}
	}
	

	private PunchKeyboard punchKeyboard;
	protected PunchKeyboard PunchKeyboard
	{
		get
		{
			if (punchKeyboard == null)
				punchKeyboard = transform.root.GetComponent<PunchKeyboard>();

			return punchKeyboard;
		}
	}
	
	private new Renderer renderer;
	
	private Text keyCapText;
	private bool uppercaseSwitch = true;
	private bool symbolSwitch;
	
	private readonly Vector3 constrainedPosition = new Vector3(0f, -0.01f, 0f);
	
	private void Start()
	{
		renderer = GetComponent<Renderer>();
		keyCapText = GetComponentInChildren<Text> ();

		originalKeyCapColor = renderer.material.color;
		
		SwitchKeyCapCase();
	}

	private void OnEnable()
	{
		PunchKeyboard?.RegisterKey(this);
	}

	private void OnDisable()
	{
		PunchKeyboard?.UnregisterKey(this);
	}

	internal virtual void PressKey()
	{
		ChangeKeyColorOnPress(true);
		transform.localPosition = constrainedPosition;
		
		OnKeyPressed.Invoke(this);
	}

	internal virtual void ReleaseKey()
	{
		ChangeKeyColorOnPress(false);
		transform.localPosition = Vector3.zero;
	}

	private void ChangeKeyColorOnPress(bool keyPressed)
	{
		renderer.material.color = keyPressed ? pressedKeyCapColor : originalKeyCapColor;
	}

	private void SwitchKeyCapCase()
	{
		keyCapText.text = uppercaseSwitch ? $"{keyCap}".ToLower() : $"{keyCap}".ToUpper();
		uppercaseSwitch = !uppercaseSwitch;
	}

	public void SwitchToSymbols()
	{
		if (!symbolSwitch)
		{
			keyCapText.text =$"{alternateKeyCap}";
			symbolSwitch = true;
		}
		else
		{
			keyCapText.text = $"{keyCap}";
			keyCapText.text = $"{keyCap}".ToLower ();
			symbolSwitch = false;
		}
	}
}