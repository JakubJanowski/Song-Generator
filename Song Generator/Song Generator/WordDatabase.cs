using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine {
	/// <summary>
	/// Class manages database based on Arpabet representation of english words
	/// The database is built based on The CMU Pronouncing Dictionary
	/// and expanedd with the help of LOGIOS Lexicon Tool
	/// http://www.speech.cs.cmu.edu/cgi-bin/cmudict
	/// </summary>
	public static class WordDatabase {
		/// <summary>
		/// key: word
		/// value: AccentedPhoneme array
		/// </summary>
		private static Dictionary<string, AccentedPhoneme[]> database;

		/// <summary>
		/// Initializes database structure from binary stream
		/// </summary>
		/// <param name="input">Stream input from binary database file</param>
		public static void Load(Stream input) {
			int nRecords = 0;
			nRecords |= input.ReadByte() << 24;
			nRecords |= input.ReadByte() << 16;
			nRecords |= input.ReadByte() << 8;
			nRecords |= input.ReadByte();

			database = new Dictionary<string, AccentedPhoneme[]>(nRecords);

			// read records
			for (int recordIndex = 0; recordIndex < nRecords; recordIndex++) {
				int wordLength = input.ReadByte();

				// read word
				char[] letters = new char[wordLength];
				for (int letterIndex = 0; letterIndex < wordLength; letterIndex++)
					letters[letterIndex] = (char)input.ReadByte();
				string word = new string(letters);

				int phonemeLength = input.ReadByte();

				// read phoneme and it's accent
				AccentedPhoneme[] accentedPhoneme = new AccentedPhoneme[phonemeLength];
				for (int phonemeIndex = 0; phonemeIndex < phonemeLength; phonemeIndex++) {
					accentedPhoneme[phonemeIndex].phoneme = (Phoneme)input.ReadByte();
					accentedPhoneme[phonemeIndex].accent = (Accent)input.ReadByte();
				}

				database[word] = accentedPhoneme;
			}
		}

		public static AccentedPhoneme[] AccentedPhonemes(string word) {
			// wrap hashtable
			string query = word.ToUpper();
			if (database.ContainsKey(query))
				return (AccentedPhoneme[])database[query];
			return null;
		}

		private static int CountSyllables(AccentedPhoneme[] phonemes) {
			int syllablesCount = 0;
			if (phonemes != null)
				foreach (var phoneme in phonemes)
					if (phoneme.accent != Accent.Consonant)
						syllablesCount++;
			if (syllablesCount == 0) {
				syllablesCount = (int)Math.Floor((double)phonemes.Length / Math.E);
				if (syllablesCount == 0)
					return 1;
			}
			return syllablesCount;
		}

		public static int CountSyllables(string word) {
			if (!database.ContainsKey(word)) {
				int syllablesCount = (int)Math.Floor((double)word.Length / Math.E);
				if (syllablesCount == 0)
					return 1;
				return syllablesCount;
			}
			return CountSyllables((AccentedPhoneme[])database[word]);
		}

		public static Syllable[] Syllables(string word) {
			AccentedPhoneme[] phoneme = AccentedPhonemes(word);
			if (phoneme != null)
				return Syllables(phoneme);
			return null;
		}


		/// <summary>
		/// generate syllables for given phonemes of a word
		/// </summary>
		/// <param name="phonemes">AccentedPhoneme array generated for given word</param>
		/// <returns>Syllable array for given phonemes</returns>
		public static Syllable[] Syllables(AccentedPhoneme[] phonemes) {
			// initialize all ungrouped to -1
			int[] syllableGroups = Enumerable.Repeat(-1, phonemes.Length).ToArray();

			// add each vowel to its own group
			for (int phonemeIndex = 0, syllableIndex = 0; phonemeIndex < phonemes.Length; phonemeIndex++)
				if (phonemes[phonemeIndex].accent != Accent.Consonant)
					syllableGroups[phonemeIndex] = syllableIndex++;

			// iterate through constants
			for (int iterator = 0; iterator < 3; iterator++) {
				// copy the last iteration
				int[] newGroups = new int[phonemes.Length];
				for (int index = 0; index < phonemes.Length; index++)
					newGroups[index] = syllableGroups[index];

				// update each unlabeled phoneme
				for (int phonemeIndex = 0; phonemeIndex < phonemes.Length; phonemeIndex++) {
					// check labels on left and right neighbors
					if (syllableGroups[phonemeIndex] == -1) {
						if (phonemeIndex == 0) {  // leftmost
							if (syllableGroups.Length == 1)
								return null;
							newGroups[0] = syllableGroups[1];
						}
						else if (phonemeIndex == phonemes.Length - 1)   // rightmost
							newGroups[phonemeIndex] = syllableGroups[phonemeIndex - 1];
						else if (syllableGroups[phonemeIndex - 1] == -1)    // propagate from right
							newGroups[phonemeIndex] = syllableGroups[phonemeIndex + 1];
						else if (syllableGroups[phonemeIndex + 1] == -1)    // propagate from left
							newGroups[phonemeIndex] = syllableGroups[phonemeIndex - 1];
						else {  // need to tiebreak
							if (phonemes[phonemeIndex - 1].accent == Accent.Primary)
								newGroups[phonemeIndex] = syllableGroups[phonemeIndex - 1];
							else if (phonemes[phonemeIndex + 1].accent == Accent.Primary)
								newGroups[phonemeIndex] = syllableGroups[phonemeIndex + 1];
							else if (phonemes[phonemeIndex - 1].accent == Accent.Secondary)
								newGroups[phonemeIndex] = syllableGroups[phonemeIndex - 1];
							else
								newGroups[phonemeIndex] = syllableGroups[phonemeIndex + 1];
						}

					}
				}
				syllableGroups = newGroups;
			}

			// post-processing

			// create an array for each syllable
			int syllableCount = CountSyllables(phonemes);
			AccentedPhoneme[][] tmpSyllables = new AccentedPhoneme[syllableCount][];
			// calculate the length of each syllable
			int[] lengths = new int[syllableCount];
			for (int index = 0; index < phonemes.Length; index++) {
				if (syllableGroups[index] == -1)
					return null;
				lengths[syllableGroups[index]]++;
			}
			// initialize each temporary syllable
			for (int index = 0; index < syllableCount; index++)
				tmpSyllables[index] = new AccentedPhoneme[lengths[index]];
			// fill in each temporary syllable
			int[] syllableGroupIndexes = new int[syllableCount];
			for (int index = 0; index < phonemes.Length; index++)
				tmpSyllables[syllableGroups[index]][syllableGroupIndexes[syllableGroups[index]]++] = phonemes[index];

			// create actual syllables
			Syllable[] returnValue = new Syllable[syllableCount];
			for (int index = 0; index < syllableCount; index++)
				returnValue[index] = new Syllable(tmpSyllables[index], (index == (syllableCount - 1)));

			return returnValue;
		}


		/// <summary>
		/// Finds one-word rhymes for specified word
		/// </summary>
		/// <param name="syllables">word to find rhyme for</param>
		/// <returns>
		/// list containing all found rhyming words
		/// </returns>
		public static List<string>[] findRhymes(string word) {
			List<string>[] rhymingWords = new List<string>[12]; // consider 11 as the maximum number of syllables in an english word (antidisestablishmentarianism)
			for (int i = 0; i < 12; i++)
				rhymingWords[i] = new List<string>();
			Syllable[] syllables1 = Syllables(word);
			if (syllables1 == null)
				return rhymingWords;
			Array.Reverse(syllables1);
			int maxIndex;
			float rhymeCoefficient;
			foreach (var record in database) {
				Syllable[] syllables2 = Syllables(record.Key);
				if (syllables2 == null)
					continue;
				Array.Reverse(syllables2);
				maxIndex = Math.Min(syllables1.Length, syllables2.Length);
				rhymeCoefficient = 0;
				for (int index = 0; index < maxIndex; index++)
					rhymeCoefficient += Syllable.Rhyme(syllables1[index], syllables2[index]) * (maxIndex - index);
				if (rhymeCoefficient / ((maxIndex * maxIndex + maxIndex) / 2) >= 0.8)
					rhymingWords[syllables2.Length].Add(record.Key);
			}
			return rhymingWords;
		}


		/// <summary>
		/// abandoned functionality
		/// inefficient
		/// Finds up to two-word rhymes for specified syllables
		/// </summary>
		/// <param name="syllables">table of 5 syllables to find rhyme for</param>
		/// <returns>
		/// list containing tuples
		/// each tuple contains number of syllables in returned rhyme
		/// and a list of words that the rhyme consists of
		/// </returns>
		public static List<Tuple<int, List<string>>> findRhymes2(Syllable[] syllables) {
			List<Tuple<int, List<string>>> rhymes = new List<Tuple<int, List<string>>>();
			Array.Reverse(syllables);
			int maxIndex;
			int syllablesFound;
			float rhymeCoefficient;


			foreach (var record in database) {
				syllablesFound = 0;
				Syllable[] rhymeSyllables = Syllables(record.Key);
				if (rhymeSyllables == null)
					continue;
				Array.Reverse(rhymeSyllables);
				maxIndex = Math.Min(syllables.Length, rhymeSyllables.Length);
				rhymeCoefficient = 0;

				for (int index = 0; index < maxIndex; index++)
					rhymeCoefficient += Syllable.Rhyme(syllables[index], rhymeSyllables[index]) * (maxIndex - index);
				if (rhymeCoefficient / ((maxIndex * maxIndex + maxIndex) / 2) >= 0.8) {
					syllablesFound += rhymeSyllables.Length;
					if (syllablesFound < 5) {

						foreach (var record2 in database) {
							Syllable[] rhymeSyllables2 = Syllables(record2.Key);
							if (rhymeSyllables2 == null)
								continue;
							Array.Reverse(rhymeSyllables2);
							maxIndex = Math.Min(syllables.Length - syllablesFound, rhymeSyllables2.Length);
							rhymeCoefficient = 0;

							for (int index = 0; index < maxIndex; index++)
								rhymeCoefficient += Syllable.Rhyme(syllables[index + syllablesFound], rhymeSyllables2[index]) * (maxIndex - index);
							if (rhymeCoefficient / ((maxIndex * maxIndex + maxIndex) / 2) >= 0.8) {
								syllablesFound += rhymeSyllables2.Length;
								if (syllablesFound < 5) {

								}
								rhymes.Add(new Tuple<int, List<string>>(syllablesFound, new List<string>()));
								rhymes.Last().Item2.Add(record.Key);
								rhymes.Last().Item2.Add(record2.Key);
							}
						}


					}
					else {
						rhymes.Add(new Tuple<int, List<string>>(syllablesFound, new List<string>()));
						rhymes.Last().Item2.Add(record.Key);
					}
				}
			}
			return rhymes;
		}

		public static bool exists(string word) {
			return database.ContainsKey(word);
		}
	}
}
