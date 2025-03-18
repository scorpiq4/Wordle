using System.Collections.Concurrent;
using Wordle;

namespace WordleTests;

[TestClass]
public class WordFinderTests
{
    [TestMethod]
    public void TestOne()
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

        Assert.IsTrue(game.Tries > 0);
    }

    [TestMethod]
    public async Task TestMultiple()
    {
        var tries = new ConcurrentBag<int>();

        await Parallel.ForEachAsync(Enumerable.Range(0, 5000), async (i, t) =>
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
            await Task.CompletedTask;
        });
        var avg = tries.Average();
    }
}