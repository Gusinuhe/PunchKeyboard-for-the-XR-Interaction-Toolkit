using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;

public class PunchKeyboard : MonoBehaviour
{
    [SerializeField]
    private XRRig rig;
    
    [SerializeField]
    private float appearanceDistance = -1f;
    
    [SerializeField]
    private float appearanceHeight = 1.6f;

    public bool IsActive => isActive;
    
    private readonly List<Key> keys = new List<Key>();
    private readonly List<AutocompleterInteractable> autocompleters = new List<AutocompleterInteractable>();
    
    private readonly Dictionary<int, InputField> inputFields = new Dictionary<int, InputField>();
    private readonly Dictionary<int, TMP_InputField> tmpInputFields = new Dictionary<int, TMP_InputField>();
    
    private bool isActive;
    private InputField activeInputField;
    private NGramGenerator nGramGenerator;
    private TMP_InputField activeTMPInputField;

    private NGramGenerator NGramGenerator
    {
        get
        {
            if (nGramGenerator == null)
            {
                PunchKeyboardSettings settings = PunchKeyboardSettings.Load();
                nGramGenerator = settings.NGramGenerator;
            }

            return nGramGenerator;
        }
    }
    
    private readonly Levenshtein levenshtein = new Levenshtein();

    private void Start()
    {
        if (rig == null)
            rig = FindObjectOfType<XRRig>();
        
        RegisterInputFields();
        transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        if (IsActive == false && EventSystem.current.currentSelectedGameObject != null && (inputFields.Count > 0 || tmpInputFields.Count > 0))
        {
            GameObject activeSelectable = EventSystem.current.currentSelectedGameObject;
            int selectableID = activeSelectable.GetInstanceID();

            if (inputFields.ContainsKey(selectableID) && inputFields[selectableID].isFocused)
            {
                activeTMPInputField = null;
                activeInputField = inputFields[selectableID];
                
                activeInputField.onValueChanged.AddListener(OnInputFieldValueChanged);
                activeInputField.onEndEdit.AddListener(OnInputFieldEndEditing);
                
                Show();
            }
            else if (tmpInputFields.ContainsKey(selectableID) && tmpInputFields[selectableID].isFocused)
            {
                activeInputField = null;
                activeTMPInputField = tmpInputFields[selectableID];
                
                activeTMPInputField.onValueChanged.AddListener(OnInputFieldValueChanged);
                activeTMPInputField.onEndEdit.AddListener(OnInputFieldEndEditing);
                
                Show();
            }
        }
    }

    private void Show()
    {
        Transform rigLocation = rig.rig.transform;

        Vector3 position = rigLocation.position + (rigLocation.forward * appearanceDistance);
        Quaternion rotation = new Quaternion(0.0f, rigLocation.rotation.y, 0.0f, rigLocation.rotation.w);
        position.y = appearanceHeight;
        
        transform.SetPositionAndRotation(position, rotation);
        transform.localScale = Vector3.one;
        
        isActive = true;
    }

    private void Hide()
    {
        transform.localScale = Vector3.zero;
        
        isActive = false;
    }

    public void RegisterInputFields()
    {
        foreach (InputField inputField in FindObjectsOfType<InputField>())
        {
            int inputFieldID = inputField.gameObject.GetInstanceID();

            if (inputFields.ContainsKey(inputFieldID) == false)
                inputFields.Add(inputFieldID, inputField);
        }

        foreach (TMP_InputField inputField in FindObjectsOfType<TMP_InputField>())
        {
            int inputFieldID = inputField.gameObject.GetInstanceID();
            
            if (tmpInputFields.ContainsKey(inputFieldID) == false)
                tmpInputFields.Add(inputFieldID, inputField);
        }
    }

    public void RegisterKey(Key key)
    {
        if (keys.Contains(key))
            return;

        key.OnKeyPressed.AddListener(OnKeyPressed);
        keys.Add(key);
    }

    public void UnregisterKey(Key key)
    {
        if (keys.Contains(key) == false)
            return;
        
        key.OnKeyPressed.RemoveListener(OnKeyPressed);
        keys.Remove(key);
    }

    public void RegisterAutocompleter(AutocompleterInteractable autocompleter)
    {
        if (autocompleters.Contains(autocompleter))
            return;

        autocompleter.OnAutoCompleterSelected.AddListener(OnAutoCompleterSelected);
        autocompleters.Add(autocompleter);

        int index = autocompleters.Count - 1;
        autocompleters[index].SetText(GetLevenshteinCorpus()[index]);
    }

    public void UnregisterAutocompleter(AutocompleterInteractable autocompleter)
    {
        if (autocompleters.Contains(autocompleter) == false)
            return;
        
        autocompleter.OnAutoCompleterSelected.RemoveListener(OnAutoCompleterSelected);
        autocompleters.Remove(autocompleter);
    }

    private void OnKeyPressed(Key key)
    {
        string inputText = String.Empty;
        
        if (activeInputField != null)
            inputText = activeInputField.text;
        else if (activeTMPInputField != null)
            inputText = activeTMPInputField.text;

        switch (key.KeyCode)
        {
            case KeyCode.Backspace:
                if (inputText.Length > 0)
                    inputText = inputText.Remove(inputText.Length - 1);
                break;
            case KeyCode.Return:
                if (activeInputField != null)
                    activeInputField.DeactivateInputField();
                else if (activeTMPInputField != null)
                    activeTMPInputField.DeactivateInputField();
                break;
            default:
                inputText += key.KeyCap;
                break;
        }

        if (activeInputField != null)
        {
            activeInputField.text = inputText;
            activeInputField.caretPosition = inputText.Length;
        }

        if (activeTMPInputField != null)
        {
            activeTMPInputField.text = inputText;
            activeTMPInputField.caretPosition = inputText.Length;
        }
    }
    
    private void OnAutoCompleterSelected(AutocompleterInteractable autocompleter)
    {
        string suggestedWord = autocompleter.GetText();
        List<string> inputText = new List<string>();
        StringBuilder builder = new StringBuilder();
        string input = activeInputField != null ? activeInputField.text : activeTMPInputField.text;
        string[] parts = input.Split(' ');
        parts = parts.Take(parts.Length - 1).ToArray();

        for(int i = 0; i < parts.Length; i++)
            inputText.Add(parts[i]);


        inputText.Add(suggestedWord);

        foreach (string w in inputText)
            builder.Append(w).Append(" ");


        if (activeInputField != null)
        {
            activeInputField.text = builder.ToString();
            activeInputField.ActivateInputField();
        }

        if (activeTMPInputField != null)
        {
            activeTMPInputField.text = builder.ToString();
            activeTMPInputField.ActivateInputField();
        }

        PredictNextWords(suggestedWord);
    }

    private void OnInputFieldValueChanged(string text)
    {
        levenshtein.RunAutoComplete(text, GetLevenshteinCorpus(), autocompleters.ToArray());
    }
    
    private void OnInputFieldEndEditing(string text)
    {
        if (activeInputField != null)
        {
            activeInputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
            activeInputField.onEndEdit.RemoveListener(OnInputFieldEndEditing);
        }
        else if (activeTMPInputField != null)
        {
            activeTMPInputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
            activeTMPInputField.onEndEdit.RemoveListener(OnInputFieldEndEditing);
        }
        
        Hide();
    }
    
    private List<string> GetLevenshteinCorpus()
    {
        return NGramGenerator.LevenshteinCorpus;
    }
    
    private void PredictNextWords(string suggestedWord)
    {
        nGramGenerator.PredictNextWords(suggestedWord, autocompleters.ToArray());
    }
}
