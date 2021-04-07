using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using System.IO;
using System;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class NGramGenerator
{
	private List<string> levenshteinCorpus;
	private readonly List<string> biGramPredictionCorpus = new List<string>();
	private Dictionary<string, int> biGramDictionary = new Dictionary<string, int>();
	private Dictionary<string, int> levenshteinDictionary = new Dictionary<string, int>();

	public List<string> LevenshteinCorpus => levenshteinCorpus;

	public NGramGenerator(string biGramDictionaryPath, string levenshteinDictionaryPath)
	{
		if (string.IsNullOrEmpty(biGramDictionaryPath))
			throw new ArgumentNullException(biGramDictionaryPath);
		
		if (string.IsNullOrEmpty(levenshteinDictionaryPath))
			throw new ArgumentNullException(levenshteinDictionaryPath);

		biGramDictionary = LoadDictionary(biGramDictionaryPath);
		levenshteinDictionary = LoadDictionary(levenshteinDictionaryPath);

		GenerateLevenshteinCorpus();
	}
	
	public void PredictNextWords(string input, AutocompleterInteractable[] autocompleters)
	{
		foreach(KeyValuePair<string, int> kvp in biGramDictionary)
		{
			if(kvp.Key.Contains(input.ToLower() + " "))
				biGramPredictionCorpus.Add(kvp.Key.Split(' ')[1]);
		}

		if(biGramPredictionCorpus.Count < autocompleters.Length)
		{
			for(int i = 0; i < biGramPredictionCorpus.Count; i++)
				autocompleters[i].SetText(biGramPredictionCorpus[i]);
		
			for(int i = biGramPredictionCorpus.Count; i < autocompleters.Length; i++)
				//Don't forget to filter repeating stuff like "to" "to" etc.
				autocompleters[i].SetText(levenshteinCorpus[i - biGramPredictionCorpus.Count]);
		}
		else
		{
			for(int i = 0; i < autocompleters.Length; i++)
				autocompleters[i].SetText(biGramPredictionCorpus[i]);
		}
		
		biGramPredictionCorpus.Clear();
	}

	internal string GenerateBiGrams(string corpus)
	{
		string[] nGrams = MakeNGrams(corpus, 2).ToArray();

		for(int i = 0; i < nGrams.Length; i++)
		{
			if (biGramDictionary.ContainsKey(nGrams.ElementAt(i)))
				biGramDictionary[nGrams.ElementAt(i)] += 1;
			else
				biGramDictionary.Add(nGrams.ElementAt(i), 1);
		}
		
		IOrderedEnumerable<KeyValuePair<string, int>> orderedEnum = from entry in biGramDictionary orderby entry.Value descending select entry;
		biGramDictionary = orderedEnum.ToDictionary(pair => pair.Key, pair => pair.Value);

		return GetDictionaryContent(biGramDictionary);
	}

	internal string GenerateLevenshteinDictionary(string corpus)
	{
		Regex wordPattern = new Regex("[\\w']+");

		foreach (Match match in wordPattern.Matches(corpus))
		{
			int currentCount;
			levenshteinDictionary.TryGetValue(match.Value, out currentCount);

			currentCount++;
			levenshteinDictionary[match.Value] = currentCount;
		}
		
		IOrderedEnumerable<KeyValuePair<string, int>> orderedEnum = from entry in levenshteinDictionary orderby entry.Value descending select entry;
		levenshteinDictionary = orderedEnum.ToDictionary(pair => pair.Key, pair => pair.Value);

		return GetDictionaryContent(levenshteinDictionary);
	}
	
	internal void GenerateLevenshteinCorpus()
	{
		levenshteinCorpus = levenshteinDictionary.Keys.ToList();
	}

	// N-gram creator by Jake Drew bit.ly/N-grams
	private IEnumerable<string> MakeNGrams(string text, byte nGramSize)
	{
		StringBuilder nGram = new StringBuilder();
		Queue<int> wordLengths = new Queue<int>();
		int wordCount = 0;
		int lastWordLen = 0;

		if(text != "" && char.IsLetterOrDigit(text[0]))
		{
			nGram.Append(text[0]);
			lastWordLen++;
		}

		for(int i = 1; i < text.Length - 1; i++)
		{
			char before = text[i - 1];
			char after = text[i + 1];

			if(char.IsLetterOrDigit(text[i]) || (text[i] != ' '
			   && (char.IsSeparator(text[i]) || char.IsPunctuation(text[i]))
				&& (char.IsLetterOrDigit(before) && char.IsLetterOrDigit(after))))
			{
				nGram.Append(text[i]);
				lastWordLen++;
			}
			else if(lastWordLen > 0)
			{
				wordLengths.Enqueue(lastWordLen);
				lastWordLen = 0;
				wordCount++;
			
				if(wordCount >= nGramSize)
				{
					yield return nGram.ToString();
					nGram.Remove(0, wordLengths.Dequeue() + 1);
					wordCount -= 1;
				}

				nGram.Append(" ");
			}
		}
	}
	
	private Dictionary<string, int> OrderDictionary(string filePath)
	{
		TextAsset dictionaryFile = Resources.Load (filePath) as TextAsset;
		
		if (dictionaryFile == null)
			throw new FileNotFoundException($"No text file found at:\n{filePath}");
		
		string rawDictionary = dictionaryFile.ToString();
		Dictionary<string, int> dictionary = GetDictionary(rawDictionary);
		IOrderedEnumerable<KeyValuePair<string, int>> orderedEnum = from entry in dictionary orderby entry.Value descending select entry;
		
		return orderedEnum.ToDictionary(pair => pair.Key, pair => pair.Value);
	}

	private Dictionary<string, int> LoadDictionary(string filePath)
	{
		TextAsset dictionaryFile = Resources.Load (filePath) as TextAsset;
		
		if (dictionaryFile == null)
			throw new FileNotFoundException($"No text file found at:\n{filePath}");
		
		string rawDictionary = dictionaryFile.ToString();
		return GetDictionary(rawDictionary);
	}

	private string GetDictionaryContent(Dictionary<string, int> dictionary)
	{
		StringBuilder builder = new StringBuilder();
		foreach (KeyValuePair<string, int> pair in dictionary)
			builder.Append(pair.Key).Append(":").Append(pair.Value).Append(',');
		
		string result = builder.ToString();
		result = result.TrimEnd(',');
		
		return result;
	}

	Dictionary<string, int> GetDictionary(string dictionaryContent)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		string[] tokens = dictionaryContent.Split(new char[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
		
		for (int i = 0; i < tokens.Length; i += 2)
		{
			string name = tokens[i];
			string freq = tokens[i + 1];

			int count = int.Parse(freq);
			
			if (dictionary.ContainsKey(name))
				dictionary[name] += count;
			else
				dictionary.Add(name, count);
		}
		
		return dictionary;
	}
}