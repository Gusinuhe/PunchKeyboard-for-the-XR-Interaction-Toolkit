using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class AutocompleterInteractable : AbstractInteractable
{
    [Serializable]
    public class KeyEvent : UnityEvent<AutocompleterInteractable> {}
    
    [SerializeField]
    private AudioSource audioSource;
    
    [SerializeField] 
    private Text label;

    public KeyEvent OnAutoCompleterSelected = new KeyEvent();

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

    protected override void Start()
    {
        base.Start();
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        if (label == null)
            label = GetComponentInChildren<Text>();
    }

    public string GetText()
    {
        return label.text;
    }
    
    public void SetText(string text)
    {
        label.text = text;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        PunchKeyboard?.RegisterAutocompleter(this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        PunchKeyboard?.UnregisterAutocompleter(this);
    }

    protected override void OnHoverEntered(XRBaseInteractor interactor)
    {
        if (interactor is XRDirectInteractor)
        {
            AutoComplete();
        }
    }

    protected override void OnHoverExiting(XRBaseInteractor interactor)
    { }

    protected override void OnSelectEntering(XRBaseInteractor interactor)
    {
        AutoComplete();
    }

    protected override void OnSelectExiting(XRBaseInteractor interactor)
    { }
    
    private void AutoComplete()
    {
        audioSource.Play();
        OnAutoCompleterSelected.Invoke(this);
        Rigidbody.isKinematic = true;
    }

    // public static string ReverseString(string s)
    // {
    //     char[] charArray = s.ToCharArray ();
    //     Array.Reverse (charArray);
    //     return new string (charArray);
    // }
}
