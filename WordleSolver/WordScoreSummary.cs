using System.Collections.Generic;
using System.Linq;


namespace WordleSolver
{
    public class WordScoreSummary
    {
        public string Word { get; private set; }
        public int MaxRemainingWordCount { get; private set; }
        public int MinRemainingWordCount { get; private set; }
        public double AverageRemainingWordCount { get; private set; }

        public List<int> AllRemainingWordCounts { get; private set; }

        public WordScoreSummary(string word, List<int> allRemainingWordCounts)
        {
            Word = word;
            AllRemainingWordCounts = allRemainingWordCounts.ToList();

            if (AllRemainingWordCounts.Count() > 0)
            {
                MaxRemainingWordCount = AllRemainingWordCounts.Max();
                MinRemainingWordCount = AllRemainingWordCounts.Min();
                AverageRemainingWordCount = AllRemainingWordCounts.Average();
            }
        }
    }

}


