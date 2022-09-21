using System.Collections.Generic;
using System.Linq;

namespace Wordle
{
    public class WordleGame
    {
        public GameOptions Options { get; }

        public string[] Words { get; }
        public string Word { get; }
        public int Tries { get; set; }
        
        public WordleGame(GameOptions options)
        {
            Options = options;
            Guesses = new Character[Options.Tries, Options.WordLength];
            CharactersPlayed = new Dictionary<char, Character>();

            Words = Dictionary.Words.Where(i => i.Length == 5).ToArray();

            Word = Words[Utils.Random.Next(0, Words.Length)];
        }

        public Character[,] Guesses { get; }

        public CharacterStatus GetCharacterStatus(int characterPosition)
        {
            return Guesses[Tries - 1, characterPosition].Status;
        }

        public Dictionary<char, Character> CharactersPlayed { get; }

        public bool EvaluateGuessWord(string guessWord)
        {
            var word = Word.ToArray();

            // Exact Match
            for (var i = 0; i < Options.WordLength; i++)
            {
                if (guessWord[i] == word[i])
                {
                    word[i] = ' ';
                    CharacterPlayed(i, CharacterStatus.ExactMatch);
                }
            }

            // Match or Eliminated
            for (var i = 0; i < Options.WordLength; i++)
            {
                // Exact match already found
                if (Guesses[Tries - 1, i] != null)
                    continue;

                // Match
                for (var j = 0; j < Options.WordLength; j++)
                {
                    if (guessWord[i] == word[j])
                    {
                        word[j] = ' ';
                        CharacterPlayed(i, CharacterStatus.Match);
                    }
                }

                // Eliminated
                if ((Guesses[Tries - 1, i]?.Status ?? CharacterStatus.Unknown) == CharacterStatus.Unknown)
                    CharacterPlayed(i, CharacterStatus.Eliminated);
            }

            void CharacterPlayed(int i, CharacterStatus status)
            {
                Guesses[Tries - 1, i] = new Character(guessWord[i], status);

                if (CharactersPlayed.TryGetValue(guessWord[i], out var character))
                {
                    if (character.Status != CharacterStatus.ExactMatch)
                        character.Status = status;
                }
                else
                {
                    CharactersPlayed.Add(guessWord[i], new Character(guessWord[i], status));
                }
            }

            return guessWord == Word;
        }
    }
}
