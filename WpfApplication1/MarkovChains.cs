using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/// <summary>
/// Author: Jakub Janowski
/// Author: Maciej Khuat Cao
/// </summary>
/// 
namespace Engine {
	/// <summary>
	/// Class used to generate sentences using specified text
	/// </summary>
	public static class MarkovChains {
		/// <summary>
		/// Dictionary holding lists of words that come after the given key word
        /// second order Markov chain
		/// </summary>
		private static Dictionary<string, List<Tuple<string, string>>> markovChainForward;

        /// <summary>
        /// variables used by generator
        /// </summary>
        private static Random rand = new Random();
        private static int generatorInterations;
        private const int maxGeneratorInterations = 5000;
        private static string eventualRhyme;
        private static int eventualRhymeSyllables;
        private static List<string>[] rhymes;

        /// <summary>
        /// Function creating second order Markov chain from given input text
        /// </summary>
        /// <param name="text">
        /// String containing sentences to generate text from.
        /// Unrelated sentences should be separated with #end delimiter.
        /// Each group of sentences must have at least two words.
        /// for this function to create correct Markov chain
        /// </param>
        public static void CreateMarkovChain(string text) {
			string[] words = text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

			if (words.Length < 2)
				return;

			int distinctWords = words.Distinct().Count();
			markovChainForward = new Dictionary<string, List<Tuple<string, string>>>(distinctWords + 1);

			// markovChain["#START"] list contains words that can begin a sentence
			markovChainForward.Add("#START", new List<Tuple<string, string>>());
            markovChainForward["#START"].Add(new Tuple<string, string>(words[0], words[1]));


            for (int i = 0; i < words.Length - 2; i++) {
				// populate forward chain
				if (words[i + 2] == "#end") {   // end of song mark
                    // add one reference for i-th word
                    if (!markovChainForward.ContainsKey(words[i]))
                        markovChainForward.Add(words[i], new List<Tuple<string, string>>());
                    markovChainForward[words[i]].Add(new Tuple<string, string>(words[i + 1], null));
                    
                    i += 2;
                    // start analyzing next song
                    if (i < words.Length - 2)
                        markovChainForward["#START"].Add(new Tuple<string, string>(words[i + 1], words[i + 2]));
                    else if (i < words.Length)
                        markovChainForward["#START"].Add(new Tuple<string, string>(words[i + 1], null));
                }
				else {
					if (!markovChainForward.ContainsKey(words[i]))
						markovChainForward.Add(words[i], new List<Tuple<string, string>>());
                    markovChainForward[words[i]].Add(new Tuple<string, string>(words[i + 1], words[i + 2]));

					// If it's the beginning of a sentence, add the next word to the start node too
					if (words[i][words[i].Length - 1] == '.' || words[i][words[i].Length - 1] == '!' || words[i][words[i].Length - 1] == '?')
						markovChainForward["#START"].Add(new Tuple<string, string>(words[i + 1], words[i + 2]));
				}
			}
		}


		public static string Generate(int length) {
			string currentWord = "#START";
			string nextWord = null;
			string str = "";
			int syllableCount = 0;
			int r;
			Regex expression = new Regex("[^a-z0-9' -]");

            r = rand.Next(0, markovChainForward[currentWord].Count);
            if (length > 0)
                str += markovChainForward[currentWord][r].Item1;
            if (length > 1) {
                str += " " + markovChainForward[currentWord][r].Item2 + " ";
                nextWord = markovChainForward[currentWord][r].Item2;
                currentWord = markovChainForward[currentWord][r].Item1;
                syllableCount += WordDatabase.CountSyllables(currentWord.ToUpper());
                syllableCount += WordDatabase.CountSyllables(nextWord.ToUpper());
            }

			string result = Generate(length - 2, syllableCount, currentWord, nextWord);
			if (result == null)
				return null;
			return str + result;
        }

        public static string Generate(int length, int syllableCount, string currentWord, string nextWord) {
            string str = "";
            int r;
            Regex expression = new Regex("[^a-z0-9' -]");

            for (int wordsGenerated = 0; wordsGenerated < length; wordsGenerated++) {
                r = rand.Next(0, markovChainForward[currentWord].Count);
                for (int index = r; index < markovChainForward[currentWord].Count; index++)
                    if (markovChainForward[currentWord][index].Item1 == nextWord && markovChainForward[currentWord][index].Item2 != null) {
                        r = index;
                        str += markovChainForward[currentWord][r].Item2;
                        syllableCount += WordDatabase.CountSyllables(markovChainForward[currentWord][r].Item2.ToUpper());
                        goto found;
                    }
                for (int index = 0; index < r; index++)
                    if (markovChainForward[currentWord][index].Item1 == nextWord && markovChainForward[currentWord][index].Item2 != null) {
                        r = index;
                        str += markovChainForward[currentWord][r].Item2;
                        syllableCount += WordDatabase.CountSyllables(markovChainForward[currentWord][r].Item2.ToUpper());
                        goto found;
                    }

                // try to find in first order chain
                r = rand.Next(0, markovChainForward[nextWord].Count);
                nextWord = markovChainForward[nextWord][r].Item1;
                str += nextWord;
                syllableCount += WordDatabase.CountSyllables(nextWord.ToUpper());
                // this means it's the lase word of a sentence, so we start a new sentence
                if (nextWord.Contains("."))
                    str += " ";
                else
                    str += ". ";

                currentWord = "#START";
                r = rand.Next(0, markovChainForward[currentWord].Count);
                if (++wordsGenerated < length) {
                    nextWord = markovChainForward[currentWord][r].Item1;
                    str += " " + nextWord;
                    syllableCount += WordDatabase.CountSyllables(nextWord.ToUpper());
                }
                if (++wordsGenerated < length) {
                    nextWord = markovChainForward[currentWord][r].Item2;
                    str += " " + nextWord + " ";
                    currentWord = markovChainForward[currentWord][r].Item1;
                    syllableCount += WordDatabase.CountSyllables(nextWord.ToUpper());
                }
                continue;

                found:
                nextWord = markovChainForward[currentWord][r].Item2;
                currentWord = markovChainForward[currentWord][r].Item1;
                str += " ";
            }

            rhymes = WordDatabase.findRhymes(expression.Replace(nextWord, ""));
            int syllables = WordDatabase.CountSyllables(nextWord.ToUpper());
            rhymes[syllables].Remove(expression.Replace(nextWord, "").ToUpper());
            // check if any rhyme was found
            foreach (var rhymeList in rhymes)
                if (rhymeList.Count > 0)
                    goto proceed;
            return null; // return if no rhyme was found

            proceed:
            eventualRhyme = nextWord;
            eventualRhymeSyllables = syllables;
            generatorInterations = 0;
            string secondLine = Generate(syllableCount, currentWord, nextWord);
            if (secondLine == null)
                return null;  // could not create rhyming line
            return str + "\n" + secondLine;
        }

        /// <summary>
        /// Generates recursively words that match the previously generated in Markov's chain
        /// And returns when a word from given list is found in the chain and the syllable count
        /// in generated line match the count from previous line
        /// </summary>
        /// <param name="syllablesLeft">specify number of syllables left to generate</param>
        /// <param name="previousWord">specify the last word placed in text to use as reference in Markov's chain structure</param>
        /// <param name="rhymes">
        /// array of lists of rhymes for the word at the end of previous line
        /// each array index corresponds to the number of syllables in each word from list plus one
        /// </param>
        /// <returns></returns>
        public static string Generate(int syllablesLeft, string prepreviousWord, string previousWord) {
			Regex expression = new Regex("[^a-z0-9' -]");
            int r1;
            if (!markovChainForward.ContainsKey(previousWord)) {
                r1 = rand.Next(0, markovChainForward["#START"].Count);
                prepreviousWord = markovChainForward["#START"][r1].Item1;
                syllablesLeft -= WordDatabase.CountSyllables(prepreviousWord.ToUpper());
                previousWord = markovChainForward["#START"][r1].Item2;
                syllablesLeft -= WordDatabase.CountSyllables(previousWord.ToUpper());
                if (syllablesLeft < 1)
                    return null;
            }
			r1 = rand.Next(0, markovChainForward[previousWord].Count);

			// If there is a rhyme that has exact number of syllables to end this line, pick it
			if (syllablesLeft < rhymes.Length) {
				int r2 = rand.Next(0, rhymes[syllablesLeft].Count); // is rand generating it with bounds inclusively?
				// iterate over each found rhyme and check if such rhyme exists in markov chain for previous word
				// start iteration at random index
				for (int rhymeIndex = r2; rhymeIndex < rhymes[syllablesLeft].Count; rhymeIndex++) {
					for (int wordIndex = r1; wordIndex < markovChainForward[previousWord].Count; wordIndex++)
						if (expression.Replace(markovChainForward[previousWord][wordIndex].Item1, "").ToLower() == rhymes[syllablesLeft][rhymeIndex].ToLower())
							return markovChainForward[previousWord][wordIndex].Item1;
					for (int wordIndex = 0; wordIndex < r1; wordIndex++)
						if (expression.Replace(markovChainForward[previousWord][wordIndex].Item1, "").ToLower() == rhymes[syllablesLeft][rhymeIndex].ToLower())
							return markovChainForward[previousWord][wordIndex].Item1;
				}
				for (int rhymeIndex = 0; rhymeIndex < r2; rhymeIndex++) {
					for (int wordIndex = r1; wordIndex < markovChainForward[previousWord].Count; wordIndex++)
						if (expression.Replace(markovChainForward[previousWord][wordIndex].Item1, "").ToLower() == rhymes[syllablesLeft][rhymeIndex].ToLower())
							return markovChainForward[previousWord][wordIndex].Item1;
					for (int wordIndex = 0; wordIndex < r1; wordIndex++)
						if (expression.Replace(markovChainForward[previousWord][wordIndex].Item1, "").ToLower() == rhymes[syllablesLeft][rhymeIndex].ToLower())
							return markovChainForward[previousWord][wordIndex].Item1;
				}

                // use the same word as it's rhyme
                if (syllablesLeft == eventualRhymeSyllables) {
                    for (int wordIndex = r1; wordIndex < markovChainForward[previousWord].Count; wordIndex++)
                        if (expression.Replace(markovChainForward[previousWord][wordIndex].Item1, "").ToLower() == eventualRhyme.ToLower())
                            return markovChainForward[previousWord][wordIndex].Item1;
                    for (int wordIndex = 0; wordIndex < r1; wordIndex++)
                        if (expression.Replace(markovChainForward[previousWord][wordIndex].Item1, "").ToLower() == eventualRhyme.ToLower())
                            return markovChainForward[previousWord][wordIndex].Item1;
                }
            }

            r1 = rand.Next(0, markovChainForward[prepreviousWord].Count);
            int syllableCount;
            string nextWord;
			for (int wordIndex = r1; wordIndex < markovChainForward[prepreviousWord].Count; wordIndex++) {
                nextWord = markovChainForward[prepreviousWord][wordIndex].Item2;
                if (markovChainForward[prepreviousWord][wordIndex].Item1 == previousWord && nextWord != null) {
                    syllableCount = WordDatabase.CountSyllables(expression.Replace(nextWord, "").ToUpper());
                    if (syllableCount >= syllablesLeft) // if given word has too many syllables to occur in this line
                        continue;
                    
                    string newText = Generate(syllablesLeft - syllableCount, previousWord, nextWord);
                    if (newText != null)
                        return nextWord + " " + newText;

                    if (++generatorInterations > maxGeneratorInterations)
                        return null;
                }
			}
            for (int wordIndex = 0; wordIndex < r1; wordIndex++) {
                nextWord = markovChainForward[prepreviousWord][wordIndex].Item2;
                if (markovChainForward[prepreviousWord][wordIndex].Item1 == previousWord && nextWord != null) {
                    syllableCount = WordDatabase.CountSyllables(expression.Replace(nextWord, "").ToUpper());
                    if (syllableCount >= syllablesLeft) // if given word has too many syllables to occur in this line
                        continue;

                    string newText = Generate(syllablesLeft - syllableCount, previousWord, nextWord);
                    if (newText != null)
                        return nextWord + " " + newText;

                    if (++generatorInterations > maxGeneratorInterations)
                        return null;
                }
            }

            // try to find in first order chain
            r1 = rand.Next(0, markovChainForward[previousWord].Count);
            for (int wordIndex = r1; wordIndex < markovChainForward[previousWord].Count; wordIndex++) {
                nextWord = markovChainForward[previousWord][wordIndex].Item1;
                syllableCount = WordDatabase.CountSyllables(expression.Replace(nextWord, "").ToUpper());

                if (syllableCount >= syllablesLeft) // if given word has too many syllables to occur in this line
                    continue;

                string newText = Generate(syllablesLeft - syllableCount, previousWord, nextWord);
                if (newText != null)
                    return nextWord + " " + newText;

                if (++generatorInterations > maxGeneratorInterations)
                    return null;
            }
            for (int wordIndex = 0; wordIndex < r1; wordIndex++) {
                nextWord = markovChainForward[previousWord][wordIndex].Item1;
                syllableCount = WordDatabase.CountSyllables(expression.Replace(nextWord, "").ToUpper());

                if (syllableCount >= syllablesLeft) // if given word has too many syllables to occur in this line
                    continue;

                string newText = Generate(syllablesLeft - syllableCount, previousWord, nextWord);
                if (newText != null)
                    return nextWord + " " + newText;

                if (++generatorInterations > maxGeneratorInterations)
                    return null;
            }
            
            return null;
		}
	}
}
