using System.IO;
using UnityEditor;
using UnityEngine;

public class DictionaryEditor
{
    // TODO finish this
    private void GenerateNewDictionaries()
    {
        string directoryPath = Application.dataPath + "/Resources/WordPrediction";

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        Debug.Log("Building a new dictionaries. This can take a while depending on the corpus size.");

        TextAsset corpusFile = Resources.Load("Sample") as TextAsset;

        if (corpusFile == null)
        {
            throw new FileNotFoundException($"No text file found at:\n{corpusFile}");
        }

        string rawCorpus = corpusFile.ToString();
        NGramGenerator nGramGenerator = PunchKeyboardSettings.Load().NGramGenerator;

        string rawNGramDictionary = nGramGenerator.GenerateBiGrams(rawCorpus);
        File.WriteAllText(Application.dataPath + "/Resources/AutoCorrect/biGramDict.txt", rawNGramDictionary);
        AssetDatabase.Refresh();

        string rawLevenshteinDictionary = nGramGenerator.GenerateLevenshteinDictionary(rawCorpus);
        File.WriteAllText(Application.dataPath + "/Resources/AutoCorrect/levenshteinDict.txt",
            rawLevenshteinDictionary);
        AssetDatabase.Refresh();

        nGramGenerator.GenerateLevenshteinCorpus();
        Debug.Log("Dictionaries were successfully generated.");
    }
}
