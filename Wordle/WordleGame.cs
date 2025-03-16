using System;
using System.Collections.Generic;
using System.Linq;

namespace Wordle;

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
        CharactersPlayed = [];

        Words = [.. Dictionary.Words.Where(i => i.Length == Options.WordLength)];

        Word = Words[Random.Shared.Next(0, Words.Length)];
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

        // Evaluate Exact Matches First
        for (var i = 0; i < Options.WordLength; i++)
        {
            if (guessWord[i] == word[i])
            {
                word[i] = ' ';
                CharacterPlayed(i, CharacterStatus.ExactMatch);
            }
        }

        // Evaluate Matches and Eliminated
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
                    break;
                }
            }

            // Eliminated
            if (Guesses[Tries - 1, i] == null)
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
