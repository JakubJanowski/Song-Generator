using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine {
	public struct Syllable {
		public AccentedPhoneme[] prefix;
		public AccentedPhoneme vowel;
		public AccentedPhoneme[] suffix;

		private const int VowelSound = 5;   // importance of vowel sound
		private const int VowelAccent = 1;  // importance of syllable accent
		private const int Suffix = 7;       // importance of consonants after vowel
		private const int Prefix = 3;       // importance of consonants before vowel

		public Syllable(AccentedPhoneme[] phonemes, bool ends) {
			int vowelIndex = -1;

			for (int index = 0; index < phonemes.Length; index++) {
				if (phonemes[index].accent != Accent.Consonant) {
					if (vowelIndex != -1)
						throw new Exception("A syllable cannot have more than one vowel sound");
					vowelIndex = index;
				}
			}

			if (vowelIndex == -1)
				throw new Exception("A syllable must have a vowel sound");

			int prefixLength = vowelIndex;
			int suffixLength = phonemes.Length - vowelIndex - 1;

			this.prefix = new AccentedPhoneme[prefixLength];
			Array.Copy(phonemes, 0, this.prefix, 0, prefixLength);

			this.suffix = new AccentedPhoneme[suffixLength];
			Array.Copy(phonemes, vowelIndex + 1, this.suffix, 0, suffixLength);

			this.vowel = phonemes[vowelIndex];
		}

		/// <summary>
		/// Function computing Levenshtein distance.
		/// https://en.wikipedia.org/wiki/Levenshtein_distance
		/// https://en.wikipedia.org/wiki/Edit_distance
		/// </summary>
		/// <param name="s">phonetic representation of first word</param>
		/// <param name="t">phonetic representation of second word</param>
		/// <returns>Levenshtein distance of two the words</returns>
		private static int EditDistance(AccentedPhoneme[] s, AccentedPhoneme[] t) {
			int n = s.Length;
			int m = t.Length;

			if (n == 0)
				return m;
			if (m == 0)
				return n;

			int[,] matrix = new int[n + 1, m + 1];

			// We have a matrix like this:
			// 0	1	2	3	4	5	⋯	m
			// 1	0	0	0	0	0	⋯	0
			// 2	0	0	0	0	0	⋯	0
			// 3	0	0	0	0	0	⋯	0
			// 4	0	0	0	0	0	⋯	0
			// 5	0	0	0	0	0	⋯	0
			// ⋮		⋮	⋮	⋮	⋮	⋮	⋱	0
			// n	0	0	0	0	0	0	0
			for (int i = 0; i <= n; i++)
				matrix[i, 0] = i;
			for (int i = 0; i <= m; i++)
				matrix[0, i] = i;

			for (int y = 1; y <= n; y++) {
				for (int x = 1; x <= m; x++) {
					int cost;
					if (s[y - 1].phoneme == t[x - 1].phoneme)
						cost = 0;
					else
						cost = 1;

					int delete = matrix[y - 1, x] + 1;
					int add = matrix[y, x - 1] + 1;
					int replace = matrix[y - 1, x - 1] + cost;

					matrix[y, x] = Math.Min(Math.Min(delete, add), replace);
				}
			}

			return matrix[n, m];
		}

		/// <summary>
		/// Function computing the similiarity of two syllables
		/// </summary>
		/// <param name="a">first syllable</param>
		/// <param name="b">second syllable</param>
		/// <returns>similiarity in range [0, 1]</returns>
		public static float Rhyme(Syllable a, Syllable b) {
			int pointsAvailable = 0;
			float points = 0;

			// Check syllable's speech sound
			pointsAvailable += VowelSound;
			if (a.vowel.phoneme == b.vowel.phoneme)
				points += VowelSound;

			// Check syllable's accent
			pointsAvailable += VowelAccent;
			if (a.vowel.accent == b.vowel.accent)
				points += VowelAccent;

			// Check prefix
			// Find Levenshtein distance and compute it as fraction part of maximum distance
			if (!((a.prefix.Length == 0) && (b.prefix.Length == 0))) {
				pointsAvailable += Prefix;
				int distance = EditDistance(a.prefix, b.prefix);
				float similarity = 1 - ((float)distance / (float)(a.prefix.Length + b.prefix.Length));

				if (similarity < 0)
					similarity = 0;

				if (Math.Min(a.prefix.Length, b.prefix.Length) == 0)
					similarity = 0;

				points += similarity * Prefix;
			}

			// Check suffix - analogously

			if (!((a.suffix.Length == 0) && (b.suffix.Length == 0))) {
				pointsAvailable += Suffix;
				int distance = EditDistance(a.suffix, b.suffix);
				float similarity = 1 - ((float)distance / (float)(a.suffix.Length + b.suffix.Length));

				if (similarity < 0)
					similarity = 0;

				if (Math.Min(a.suffix.Length, b.suffix.Length) == 0)
					similarity = 0;

				points += similarity * Suffix;
			}

			return points / pointsAvailable;
		}

		/// <summary>
		/// Overridden ToString(), to print the word divided into syllables
		/// </summary>
		/// <returns>Phonetic representation with division into syllables</returns>
		public override string ToString() {
			const char delim = '/';
			string result = string.Empty;

			for (int i = 0; i < this.prefix.Length; i++)
				result += this.prefix[i].phoneme.ToString() + delim;

			result += this.vowel.phoneme.ToString();

			if (this.suffix.Length != 0)
				result += delim;

			for (int i = 0; i < this.suffix.Length; i++) {
				result += this.suffix[i].phoneme.ToString();
				if (i != this.suffix.Length - 1)
					result += delim;
			}
			return result;
		}
	}
}