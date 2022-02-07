using System.Linq;


namespace WordleSolver
{
    public class CharacterCount
    {
        public char Character { get; private set; }
        public int Count { get; private set; }
        public bool Exact { get; private set; }

        public CharacterCount(char character, int count, bool exact)
        {
            Character = character;
            Count = count;
            Exact = exact;
        }

        public void ChangeCount(int newCount)
        {
            Count = newCount;
        }

        public void SetExact(bool exact)
        {
            Exact = exact;
        }

        public string BuildRegexPattern()
        {
            return "(?=^([^" + Character + "]*" + Character + "){" + Count.ToString() + (Exact ? "" : ",") + "}[^" + Character + "]*$)";
        }

        public bool Check(string word)
        {
            var inWordCount = word.Count(c => c == Character);

            if (Exact) return inWordCount == Count;
            return inWordCount >= Count;
        }

        public CharacterCount Copy()
        {
            return new CharacterCount(Character, Count, Exact);
        }
    }

}


