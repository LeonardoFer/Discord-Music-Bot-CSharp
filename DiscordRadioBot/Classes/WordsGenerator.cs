using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

using DiscordRadioBot.Enums;

using static System.Console;

namespace DiscordRadioBot.Classes
{
    public class WordsGenerator
    {
        private readonly Random _randomNumber;
        public WordsGenerator(Random randomNumberInstance)
        {
            _randomNumber = randomNumberInstance;
        }
        /// <summary>
        /// Calculate which Language should be used to generate the words.
        /// </summary>
        /// <returns></returns>
        private List<string> CalculateLanguageChance()
        {            
            List<string> words;
            int languageChance = _randomNumber.Next(1, 100);
            if(languageChance <= 50)
            {
                words = GenerateWordList(CalculateNumberOfWords(), Language.Portuguese);
            }
            else
            {
                words = GenerateWordList(CalculateNumberOfWords(), Language.English);
            }
            
            return words;
        }
        /// <summary>
        /// Calculate the number of Words to search for. 1-5, adjustable chance.
        /// </summary>
        /// <returns></returns>
        private int CalculateNumberOfWords()
        {
            /* TODO: Use more words in the future but right now Lavalink is blowing up with some search strings.
            int numberOfWords;
            int numberOfWordsChance = _randomNumber.Next(1, 100);            
            if(numberOfWordsChance >= 30)
            {
                numberOfWords = 1;
            }
            else
            {
                numberOfWords = 2;
            }
            */
            int numberOfWords = 1;
            int numberOfWordsChance = 100;
            WriteLine($"Number of words to search for: {numberOfWords}. Chance: {numberOfWordsChance}%.\n");

            return numberOfWords;
        }
        /// <summary>
        /// Gets a list of words between 1 and 5 containing either English or Portuguese words and the word Música or Music in the end.
        /// </summary>
        /// <param name="numberOfWords"> Number of Words to grab from the source Files. </param>
        /// <returns></returns>
        private List<string> GenerateWordList(int numberOfWords, Language language)
        {
            string filePath;
            int fileNumberLines;

            switch(language)
            {
                case Language.Portuguese:
                    filePath = ConfigurationManager.AppSettings["PortugueseFile"];
                    fileNumberLines = File.ReadLines(filePath).Count();
                    WriteLine($"There is {fileNumberLines} lines in the Portuguese file.");
                    WriteLine($"Searching for Portuguese Words:\n");
                    break;
                case Language.English:
                    filePath = ConfigurationManager.AppSettings["EnglishFile"];
                    fileNumberLines = File.ReadLines(filePath).Count();
                    WriteLine($"There is {fileNumberLines} lines in the English file.");
                    WriteLine($"Searching for English Words:\n");
                    break;
                default:
                    throw new FileNotFoundException("Unable to find or load Language Resource File!");
            }

            List<string> wordsToSearch = new List<string>(6);

            FileStream fileStream = null;
            try
            {
                fileStream = File.OpenRead(filePath);
                using(StreamReader streamReader = new StreamReader(fileStream, true))
                {
                    fileStream = null;
                    string word = string.Empty;
                    int wordCounter = 0;
                    while(wordCounter < numberOfWords)
                    {
                        int lineCounter = 0;
                        int lineNumber = _randomNumber.Next(1, fileNumberLines + 1);
                        Write($"Line number: {lineNumber},");
                        while(lineCounter < lineNumber)
                        {
                            word = streamReader.ReadLine();
                            lineCounter++;
                            if(lineCounter == lineNumber)
                            {
                                Write($" word found! The word was: {word}!\n");
                                wordsToSearch.Add(word);
                                streamReader.DiscardBufferedData();
                                streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                            }
                        }
                        wordCounter++;
                    }
                }
            }
            finally
            {
                if(fileStream != null)
                {
                    fileStream.Dispose();
                }
            }

            string suffix;
            switch(language)
            {
                case Language.Portuguese:
                    suffix = _randomNumber.Next(0, 100) >= 50 ? "música" : "remix";
                    wordsToSearch.Add(suffix);
                    break;
                case Language.English:
                    suffix = _randomNumber.Next(0, 100) >= 50 ? "lofi" : "remix";
                    wordsToSearch.Add(suffix);
                    break;
            }

            return wordsToSearch;
        }
        /// <summary>
        /// Returns a list of words to use for a search query.
        /// </summary>
        /// <returns></returns>
        public List<string> GenerateWordList()
        {
            List<string> wordList = CalculateLanguageChance();

            return wordList;
        }
    }
}
