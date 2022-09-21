using System;
using System.Collections.Generic;
using System.Linq;

namespace Wordle
{
    public static class Dictionary
    {
        static Dictionary()
        {
            Words = Properties.Resources.TWL_2006_ALPHA
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Where(i => i.Length >= 4)
                .ToArray();
            WordsHash = new HashSet<string>(Words);
        }

        public static readonly string[] Words;
        public static readonly HashSet<string> WordsHash;
    }
}
