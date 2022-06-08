// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;

namespace TextSummarization
{
    class Program
    {
        
        public static void Main()
        {
            string textToSummarize = File.ReadAllText("SampleText2.txt");
            int summarizePercentage = 15;

            string[] sentences = textToSummarize
                .Split(Constants.SentenceEndCharacters, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x=>x.Trim()+'.').ToArray();

            string stopWordsRegex = @$"\b({string.Join('|', Constants.StopWords.Select(Regex.Escape))})\b";
            string punctuationRegex =
                $"({string.Join('|', Constants.PunctuationCharacters.Select(x => Regex.Escape(x.ToString())))})";

            List<string[]> sentenceTokens = sentences
                    .Select(sentence => Regex.Replace(sentence, stopWordsRegex, "", RegexOptions.IgnoreCase))
                    .Select(sentence => Regex.Replace(sentence, punctuationRegex, "", RegexOptions.IgnoreCase))
                    .Select(sentence => Regex.Replace(sentence, "\\s+", " ", RegexOptions.IgnoreCase)
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).Select(x=>x.ToLower()).ToArray())
                    .ToList();

            Debug.Assert(sentences.Length == sentenceTokens.Count);

            var tokens = sentenceTokens.SelectMany(x => x).ToList();
            var scoredTokens = tokens.GroupBy(x => x, (token, all) => (Token:token, Count:all.Count())).OrderByDescending(x=>x.Count).ToList();

            int maxScore = scoredTokens.Max(x => x.Count);
            var tokenScoresNormalized = scoredTokens.ToDictionary(x => x.Token, x => (double)x.Count / maxScore);

            var scoredSentences =
                sentences
                    .Select((sentence, i) => (Sentence: sentence, Score: sentenceTokens[i].Sum(token => tokenScoresNormalized[token])))
                    .ToList();

            int finalSentencesCount = (int)Math.Max(1,Math.Ceiling(((double) summarizePercentage/100) * sentences.Length));

            //scoredSentences.ForEach(x=>Console.WriteLine(x));

            var topScoredSentences = sentences.Select((sentence, i) => (OriginalIndex: i, SentenceText: sentence, Score: scoredSentences[i]))
                .OrderByDescending(x => x.Score).Take(finalSentencesCount);

            var topSentencesInOrder = topScoredSentences.OrderBy(x => x.OriginalIndex);

            string summary = string.Join("", topSentencesInOrder.Select(x=>x.SentenceText));
            
            Console.WriteLine(summary);
        }
    }
}

