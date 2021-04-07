using System.Linq;
using System.Collections.Generic;

public class Levenshtein
{
	private const int MaxWordLength = 15;
	private const int MaxLevenshteinCost = 7;
	private const int MinLevenshteinCost = 1;
	
	public bool IsUppercase { get; set; }
	public bool IsFirstLetterUpper { get; set; }

	public void RunAutoComplete(string input, List<string> corpus, AutocompleterInteractable[] autocompleters)
	{
		if(input.Length > 0)
		{
			char[] lastChar = input.Substring(input.Length - 1).ToCharArray();	
			string lastWord = input.Split(' ').Last ();
			char[] firstCharOfLastWord = lastWord.Substring(0).ToCharArray();
			
			if (firstCharOfLastWord.Length >= 1)
			{
				if (firstCharOfLastWord[0].ToString().Any(char.IsUpper))
					IsFirstLetterUpper = true;
				else
					IsFirstLetterUpper = false;
			}
			
			if(!char.IsWhiteSpace(lastChar[0]))
			{
				if(lastWord.Length < MaxWordLength)
				{
					if (input.Length >= 0)
					{
						Dictionary<int, int> dict = new Dictionary<int, int> ();

						for (int i = 0; i < corpus.Count; i++)
						{
							int cost = LevenshteinDistance.Compute(lastWord.ToLower(), corpus[i]);
							
							if (cost >= MinLevenshteinCost && cost <= MaxLevenshteinCost)
								dict.Add (i, cost);
						}

						if (lastWord.All (char.IsUpper))
							IsUppercase = true;
						if (lastWord.Any (char.IsLower))
							IsUppercase = false;
						
						List<int> distanceOrder = dict.OrderBy (kp => kp.Value).Select (kp => kp.Key).ToList ();

						for (int i = 0; i < distanceOrder.Count; i++)
						{
							if (i < autocompleters.Length)
							{
								if (IsUppercase)
									autocompleters [i].SetText(corpus [distanceOrder [i]].ToUpper());
								else if (IsFirstLetterUpper && IsUppercase == false)
									autocompleters [i].SetText(char.ToUpper(corpus [distanceOrder [i]][0]) + corpus [distanceOrder [i]].Substring(1));
								else if(!IsUppercase && IsFirstLetterUpper == false)
									autocompleters [i].SetText(corpus [distanceOrder [i]].ToLower ());
							}
						}
					}
				}
			}
		}
	}
}