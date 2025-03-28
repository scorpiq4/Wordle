﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wordle;

public class WordFinder
{
    public GameOptions Options { get; }

    public string[] Words { get; private set; }
    public string Word { get; }
    public int Tries { get; set; }

    public Dictionary<char, bool[]> RequiredCharacters { get; }
    public char[] MatchedCharacters { get; }
    public List<char> EliminatedCharacters { get; }

    public WordFinder(GameOptions options, string? word = null)
    {
        Options = options;

        Words = [.. Dictionary.Words.Where(i => i.Length == options.WordLength)];

        Word = word ?? Words[Random.Shared.Next(0, Words.Length)];

        RequiredCharacters = [];
        MatchedCharacters = [.. Enumerable.Range(0, Options.WordLength).Select(i => ' ')];
        EliminatedCharacters = [];
    }

    public string CalculateGuessWord()
    {
        var words = Words.AsEnumerable();

        if (EliminatedCharacters.Any())
        {
            words = words.Where(i => !i.Any(j => EliminatedCharacters.Contains(j)));
        }

        if (MatchedCharacters.Any(i => i != ' '))
        {
            words = words.Where(i =>
            {
                for (int j = 0; j < Options.WordLength; j++)
                {
                    if (MatchedCharacters[j] != ' ' && i[j] != MatchedCharacters[j])
                        return false;
                }
                return true;
            });
        }

        if (RequiredCharacters.Any())
        {
            words = words.Where(i =>
            {
                var required = 0;
                foreach (var requiredCharacter in RequiredCharacters)
                {
                    var characterFound = false;
                    for (int j = 0; j < Options.WordLength; j++)
                    {
                        if (i[j] == requiredCharacter.Key)
                        {
                            if (!requiredCharacter.Value[j])
                                return false;
                            characterFound = true;
                        }
                    }
                    if (characterFound)
                        required++;
                }
                return required == RequiredCharacters.Count;
            });
        }

        Words = [.. words];

        var letterFrequency =
            Words
            .SelectMany(i => i.ToCharArray())
            .GroupBy(i => i)
            .OrderByDescending(i => i.Count())
            .Select(i => i.Key)
            .ToArray();

        var guessWords = Words.AsEnumerable();

        if (RequiredCharacters.Count <= 2)
        {
            var test = guessWords.Where(i => !i.GroupBy(j => j).Any(j => j.Count() > 1));
            if (test.Any())
                guessWords = test;
        }

        var topGuessWords =
            guessWords
            .GroupBy(i => i.Sum(c => Array.IndexOf(letterFrequency, c)))
            .OrderByDescending(i => i.Count())
            .First()
            .ToArray();
        var index = Random.Shared.Next(0, topGuessWords.Length);
        return topGuessWords[index];
    }

    public bool EvaluateGuessWord(string guessWord)
    {
        for (int i = 0; i < Options.WordLength; i++)
        {
            if (guessWord[i] == Word[i])
            {
                MatchedCharacters[i] = guessWord[i];
                if (!RequiredCharacters.TryGetValue(guessWord[i], out var availablePositions))
                {
                    availablePositions = [.. Enumerable.Range(0, Options.WordLength).Select(j => true)];
                    RequiredCharacters.Add(guessWord[i], availablePositions);
                }
            }
            else
            {
                for (int j = 0; j < Options.WordLength; j++)
                {
                    if (guessWord[i] == Word[j])
                    {
                        if (!RequiredCharacters.TryGetValue(guessWord[i], out var availablePositions))
                        {
                            availablePositions = [.. Enumerable.Range(0, Options.WordLength).Select(k => true)];
                            RequiredCharacters.Add(guessWord[i], availablePositions);
                        }
                        availablePositions[i] = false;
                        break;
                    }
                }

                if (!RequiredCharacters.ContainsKey(guessWord[i]))
                    EliminatedCharacters.Add(guessWord[i]);
            }
        }

        while (Normalize()) { }

        bool Normalize()
        {
            var runAgain = false;
            foreach (var requiredCharacter in RequiredCharacters.ToArray())
            {
                var modified = false;
                for (int i = 0; i < Options.WordLength; i++)
                {
                    if (MatchedCharacters[i] != ' ' && requiredCharacter.Key != MatchedCharacters[i] && requiredCharacter.Value[i])
                    {
                        requiredCharacter.Value[i] = false;
                        modified = true;
                    }
                }

                if (modified && requiredCharacter.Value.Count(i => i) == 1)
                {
                    MatchedCharacters[Array.IndexOf(requiredCharacter.Value, true)] = requiredCharacter.Key;
                    runAgain = true;
                }
            }
            return runAgain;
        }

        return guessWord == Word;
    }
}
