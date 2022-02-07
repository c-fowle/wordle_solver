using System.Collections.Generic;
using System.Linq;


namespace WordleSolver
{
    public class PositionRule
    {
        static string Letters = "abcdefghijklmnopqrstuvwxyz";

        public char? DefiniteCharacter { get; private set; }
        public List<char> ExcludedCharacters { get; private set; }

        public PositionRule()
        {
            DefiniteCharacter = null;
            ExcludedCharacters = new List<char>();
        }

        public void AddExcludedCharacter(char character)
        {
            if (ExcludedCharacters.Contains(character)) return;
            ExcludedCharacters.Add(character);
        }

        public void SetDefiniteCharacter(char character)
        {
            DefiniteCharacter = character;
        }

        public string BuildRegexPattern()
        {
            if (DefiniteCharacter.HasValue) return DefiniteCharacter.Value.ToString();
            if (ExcludedCharacters.Count == 0) return string.Format("[{0}]", Letters);

            var validLetters = Letters.Where(c => !ExcludedCharacters.Contains(c));
            return string.Format("[{0}]", string.Join("", validLetters));
        }

        public bool Check(char positionCharacter)
        {
            if (DefiniteCharacter.HasValue) return positionCharacter == DefiniteCharacter;
            return !ExcludedCharacters.Contains(positionCharacter);
        }

        public PositionRule Copy()
        {
            var copy = new PositionRule();
            if (DefiniteCharacter.HasValue) copy.SetDefiniteCharacter(DefiniteCharacter.Value);
            foreach (var ec in ExcludedCharacters) copy.AddExcludedCharacter(ec);

            return copy;
        }
    }

}


