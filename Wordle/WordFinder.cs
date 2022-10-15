using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wordle
{
    public class WordFinder
    {
        public static void Test()
        {
            var tries = new System.Collections.Concurrent.ConcurrentBag<int>();

            Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(0, 10000), (range, state) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    var gameOptions = new GameOptions { WordLength = 5, Tries = 6 };
                    var word = default(string); // "FLUFF";
                    var game = new WordFinder(gameOptions, word);
                    var guessWords = new List<string>();
                    do
                    {
                        game.Tries++;
                        guessWords.Add(game.CalculateGuessWord());
                    } while (!game.EvaluateGuessWord(guessWords.Last()));
                    tries.Add(game.Tries);
                }
            });
            var avg = tries.Average();
        }

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

            Words = Dictionary.Words.Where(i => i.Length == 5).ToArray();

            Word = word ?? Words[Utils.Random.Next(0, Words.Length)];

            RequiredCharacters = new Dictionary<char, bool[]>();
            MatchedCharacters = Enumerable.Range(0, Options.WordLength).Select(i => ' ').ToArray();
            EliminatedCharacters = new List<char>();
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

            Words = words.ToArray();

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
            var index = Utils.Random.Next(0, topGuessWords.Length);
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
                        availablePositions = Enumerable.Range(0, Options.WordLength).Select(j => true).ToArray();
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
                                availablePositions = Enumerable.Range(0, Options.WordLength).Select(k => true).ToArray();
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

            return Word == guessWord;
        }
    }
}
