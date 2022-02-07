using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Newtonsoft.Json;


namespace WordleSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            var fullWordList = File.ReadAllText(Path.Combine("Data", "wordList.txt")).ToLower().Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var firstGuess = "tares";
            var secondGuesses = File.ReadAllLines(Path.Combine("Data", "secondGuesses.txt")).Select(s => s.Trim().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ToDictionary(s => s[0], s => s.Length > 1 ? s[1] : null);
            var secondGuessesHard = File.ReadAllLines(Path.Combine("Data", "secondGuessesHard.txt")).Select(s => s.Trim().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ToDictionary(s => s[0], s => s.Length > 1 ? s[1] : null);

            while (true)
            {
                var validWords = fullWordList.ToList();
                var guess = firstGuess;
                var isFirstGuess = true;

                Console.Write("Hard mode? [y/n] ");
                var isHardMode = Console.ReadKey().Key == ConsoleKey.Y;

                Console.WriteLine("");
                Console.WriteLine("");

                var positionRules = new PositionRule[5] { new PositionRule(), new PositionRule(), new PositionRule(), new PositionRule(), new PositionRule() };
                var characterCounts = new Dictionary<char, CharacterCount>();

                var possibleResults = GetPossibleResults();

                while (validWords.Count() > 1)
                {
                    Console.WriteLine("Possible words: {0}", validWords.Count);
                    if (validWords.Count() < 10)
                    {
                        Console.WriteLine(string.Join(", ", validWords));
                    }

                    Console.WriteLine("Best guess word: {0}", guess);

                    var validInput = false;
                    var result = new int[5];

                    while (!validInput)
                    {
                        Console.WriteLine("Insert result (0 for grey, 1 for yellow, 2 for green):");
                        var input = Console.ReadLine();
                        if (input.Length != 5)
                        {
                            Console.WriteLine("Invalid input size...");
                            Console.ReadLine();
                            continue;
                        }
                        var parseOkay = true;
                        for (var i = 0; i < 5; ++i)
                        {
                            if (!int.TryParse(input[i].ToString(), out result[i]) || result[i] < 0 || result[i] > 2)
                            {
                                Console.WriteLine("Invalid input content...");
                                Console.ReadLine();
                                parseOkay = false;
                                break;
                            }
                        }

                        validInput = parseOkay;
                    }

                    UpdateRulesFromResult(guess, result, positionRules, characterCounts);
                    validWords = GetValidWords(validWords, positionRules, characterCounts.Select(kvp => kvp.Value)).ToList();

                    if (validWords.Count() <= 1) break;
                    if (isFirstGuess)
                    {
                        var resultKey = string.Join("", result.Select(i => i.ToString()));
                        if (isHardMode) guess = secondGuessesHard[resultKey];
                        else guess = secondGuesses[resultKey];
                        isFirstGuess = false;
                    }
                    else
                    {
                        if (validWords.Count() == 2) guess = validWords.First();
                        if (validWords.Count() < 5 || isHardMode)
                        {
                            guess = GetBestGuess(validWords, validWords, positionRules, characterCounts, possibleResults);
                        }
                        else
                        {
                            guess = GetBestGuess(validWords, fullWordList, positionRules, characterCounts, possibleResults);
                        }
                    }
                    Console.WriteLine();
                }

                Console.WriteLine("Answer: {0}", validWords.FirstOrDefault());
                Console.WriteLine("Play again? [y/n]");
                if (Console.ReadKey().Key == ConsoleKey.N) break;
                Console.Clear();
            }
        }

        static string GetBestFirstGuess(IEnumerable<string> fullWordList)
        {
            var positionRules = new PositionRule[5] { new PositionRule(), new PositionRule(), new PositionRule(), new PositionRule(), new PositionRule() };
            var excludeCharacters = new List<char>();
            var characterCounts = new Dictionary<char, CharacterCount>();
            var possibleResults = GetPossibleResults();

            var bestGuess = GetBestGuess(fullWordList, fullWordList, positionRules, characterCounts, possibleResults, writeResult: true);
            return bestGuess;
        }

        static Dictionary<string, string> GetBestSecondGuesses(IEnumerable<string> fullWordList, bool hardMode=false)
        {
            var secondGuessFilepath = Path.Combine("Data", "secondGuesses" + (hardMode ? "Hard" : "") + ".txt");
            var firstGuess = "tares";
            var bestSecondGuesses = new Dictionary<string, string>();


            if (File.Exists(secondGuessFilepath))
            {
                var secondGuessData = File.ReadAllLines(secondGuessFilepath);
                foreach (var data in secondGuessData)
                {
                    var secondGuessParts = data.Trim().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (secondGuessParts.Length != 2) continue;

                    bestSecondGuesses.Add(secondGuessParts[0], secondGuessParts[1]);
                }
            }

            var possibleResults = GetPossibleResults();

            foreach(var result in possibleResults)
            {
                var resultString = string.Join("", result.Select(i => i.ToString()));
                if (bestSecondGuesses.ContainsKey(resultString)) continue;

                var positionRules = new PositionRule[5] { new PositionRule(), new PositionRule(), new PositionRule(), new PositionRule(), new PositionRule() };
                var characterCounts = new Dictionary<char, CharacterCount>();

                UpdateRulesFromResult(firstGuess, result, positionRules, characterCounts);
                var validWords = GetValidWords(fullWordList, positionRules, characterCounts.Select(kvp => kvp.Value)).ToList();
                var guess = "";

                if (validWords.Count() <= 2)
                {
                    guess = validWords.FirstOrDefault();
                }
                else if (validWords.Count() < 5 || hardMode)
                {
                    guess = GetBestGuess(validWords, validWords, positionRules, characterCounts, possibleResults);
                }
                else if (validWords.Count() > 1)
                {
                    guess = GetBestGuess(validWords, fullWordList, positionRules, characterCounts, possibleResults);
                }

                Console.WriteLine("{0}: {1}", resultString, guess);

                bestSecondGuesses.Add(resultString, guess);
            }

            File.WriteAllLines(secondGuessFilepath, bestSecondGuesses.Select(kvp => string.Format("{0},{1}", kvp.Key, kvp.Value)));

            return bestSecondGuesses;
        }

        static string GetBestGuess(IEnumerable<string> validAnswerList, IEnumerable<string> fullWordList, PositionRule[] currentPositionRules, Dictionary<char, CharacterCount> currentCharacterCounts, IEnumerable<int[]> possibleResults, bool writeResult=false)
        {
            var summary = new List<WordScoreSummary>();

            foreach (var w in fullWordList)
            {
                var isValid = validAnswerList.Contains(w);
                var countsAfterResult = new List<int>();

                foreach (var result in possibleResults)
                {
                    var resultingPositionRules = currentPositionRules.Select(i => i.Copy()).ToArray();
                    var resultingCharacterCounts = currentCharacterCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Copy());

                    UpdateRulesFromResult(w, result, resultingPositionRules, resultingCharacterCounts);
                    var countAfterResult = GetValidWords(validAnswerList, resultingPositionRules, resultingCharacterCounts.Select(kvp => kvp.Value)).Count();

                    if (countAfterResult == 0) continue;
                    if (isValid && countAfterResult == 1 && result.Any(i => i != 2)) continue;

                    countsAfterResult.Add(countAfterResult);
                }

                if (countsAfterResult.Count == 0) continue;
                summary.Add(new WordScoreSummary(w, countsAfterResult));
            }

            if (writeResult)
            {
                var outputPath = Path.Combine("Data", "SummaryResult.json");
                File.WriteAllText(outputPath, JsonConvert.SerializeObject(summary));
            }
           
            var orderedSummary = summary.OrderBy(i => i.AverageRemainingWordCount);
            return orderedSummary.FirstOrDefault()?.Word;
        }

        static List<int[]> GetPossibleResults()
        {
            var possibleResults = new List<int[]>();

            for (var i = 0; i < 243; ++i)
            {
                var number = (float)i;
                var result = new int[5];

                for (var j = 4; j >= 0; --j)
                {
                    var power = (float)Math.Pow(3, j);
                    result[j] = (int)Math.Floor(number / power);
                    number = (number % power);
                }

                if (result.Count(j => j == 2) == 4 && result.Count(j => j == 1) == 1) continue;

                possibleResults.Add(result);
            }

            return possibleResults;
        }

        static void UpdateRulesFromResult(string guessWord, int[] result, PositionRule[] positionRules, Dictionary<char, CharacterCount> characterCounts)
        {
            var handledCharacters = new List<char>();

            for (var i = 0; i < 5; ++i)
            {
                var character = guessWord[i];
                if (handledCharacters.Contains(character)) continue;

                var characterIndexes = new List<int>();
                var nextIndex = i;

                while (nextIndex != -1)
                {
                    characterIndexes.Add(nextIndex);
                    nextIndex = guessWord.IndexOf(character, characterIndexes.Last() + 1);
                }

                var characterResults = characterIndexes.ToDictionary(j => j, j => result[j]);

                if (characterResults.All(kvp => kvp.Value == 0))
                {
                    for(var j = 0; j < 5; ++j) positionRules[j].AddExcludedCharacter(character);
                }
                else
                {
                    var characterCount = characterResults.Count(kvp => kvp.Value > 0);
                    var exactCount = characterCount < characterResults.Count;
                    if (characterCounts.ContainsKey(character))
                    {
                        if (characterCounts[character].Count < characterCount) characterCounts[character].ChangeCount(characterCount);
                        if (exactCount && (!characterCounts[character].Exact)) characterCounts[character].SetExact(exactCount);
                    }
                    else
                    {
                        characterCounts.Add(character, new CharacterCount(character, characterCount, exactCount));
                    }
                }

                foreach (var characterIndex in characterResults.Keys)
                {
                    if (characterResults[characterIndex] == 1) positionRules[characterIndex].AddExcludedCharacter(character);
                    else if (characterResults[characterIndex] == 2) positionRules[characterIndex].SetDefiniteCharacter(character);
                }

                handledCharacters.Add(character);
            }
        }

        static IEnumerable<string> GetValidWords(IEnumerable<string> wordList, PositionRule[] positionRules, IEnumerable<CharacterCount> characterCounts)
        {
            var regexBuilder = new StringBuilder("^");
            foreach (var characterCount in characterCounts)
            {
                regexBuilder.Append(characterCount.BuildRegexPattern());
            }
            for (var i = 0; i < 5; ++i)
            {
                regexBuilder.Append(positionRules[i].BuildRegexPattern());
            }
            regexBuilder.Append("$");

            var matchRegex = new Regex(regexBuilder.ToString());
            var allowedWords = wordList.Where(word =>
            {
                var isMatch = matchRegex.IsMatch(word);
                return isMatch;
            });

            return allowedWords;
        }
    }
}


