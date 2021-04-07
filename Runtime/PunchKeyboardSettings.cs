using System.IO;
using UnityEditor;
using UnityEngine;

public class PunchKeyboardSettings : ScriptableObject
{
    [SerializeField]
    private string biGramDictionary = "WordPrediction/biGramDict";

    [SerializeField] 
    private string levenshteinDictionary = "WordPrediction/levenshteinDict";
    
    private NGramGenerator nGramGenerator;

    public NGramGenerator NGramGenerator => nGramGenerator ?? (nGramGenerator = new NGramGenerator(biGramDictionary, levenshteinDictionary));

    public static PunchKeyboardSettings Load()
    {
        PunchKeyboardSettings settings = Resources.Load<PunchKeyboardSettings>(nameof(PunchKeyboardSettings));
        
#if UNITY_EDITOR
        if (settings == null)
        {
            if (!Directory.Exists("Assets/Resources"))
            {
                Directory.CreateDirectory("Assets/Resources");
            }
            
            // Create an instance
            settings = CreateInstance<PunchKeyboardSettings>();
            AssetDatabase.CreateAsset(settings, $"Assets/Resources/{nameof(PunchKeyboardSettings)}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return settings;
        }
#endif
        
        return settings;
    }
}
