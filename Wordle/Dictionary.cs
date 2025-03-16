using System;
using System.Collections.Generic;
using System.Linq;

namespace Wordle;

public static class Dictionary
{
    static Dictionary()
    {
        Words = [.. Properties.Resources.TWL_2006_ALPHA
            .Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries)
            .Where(i => i.Length == 5)];
        WordsHash = [.. Words];
    }

    public static readonly string[] Words;
    public static readonly HashSet<string> WordsHash;
}
