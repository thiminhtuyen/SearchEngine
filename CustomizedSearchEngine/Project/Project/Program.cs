﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.QueryParsers;
using System.Windows.Forms;

namespace Project
{
    class Project
    {
        Lucene.Net.Store.Directory indexDirectory;
        Lucene.Net.Analysis.Analyzer analyzer;
        Lucene.Net.Index.IndexWriter writer;
        Lucene.Net.Search.IndexSearcher searcher; 
        Lucene.Net.QueryParsers.QueryParser parser;

        const Lucene.Net.Util.Version VERSION = Lucene.Net.Util.Version.LUCENE_30;
        const string TEXT_FN = "text";

        public Project()
        {
            indexDirectory = null;
            analyzer = null;
            writer = null;
        }

        /// <summary>
        /// Create the index at the given index directory path
        /// </summary>
        /// <param name="args"></param>
        /// 
        
        public String TakePath()
        {
            /* Take input from the user*/
            String path = Console.ReadLine();
            return path;
            
        }

        
        public void OpenIndexPath(String indexPath)
        {
            /* Make sure the indexPath passed by the user to a new directory that does not exist */
            if (System.IO.Directory.Exists(indexPath))
            {
                Console.WriteLine("This directory already exists - Choose a directory that does not exist");
                Console.Write("Hit any key to exit");
                Console.ReadKey();
                Environment.Exit(0);
            }
            /* Open the indexPath after verification*/
            indexDirectory = FSDirectory.Open(indexPath);
        }

        public void CreateAnalyzer()
        {
            analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(VERSION);
        }

        public void CreateWriter()
        {
            IndexWriter.MaxFieldLength mfl = new IndexWriter.MaxFieldLength(IndexWriter.DEFAULT_MAX_FIELD_LENGTH);
            writer = new Lucene.Net.Index.IndexWriter(indexDirectory, analyzer, true, mfl);
        }

        public void IndexText(string filesDirectory)
        {
            /* A function to index the source documents */
            var txtFiles = System.IO.Directory.EnumerateFiles(filesDirectory, "*.txt");
            int count = 0;
            foreach (var file in txtFiles)
            {
                Console.WriteLine("Indexing...");
                Console.WriteLine("Doc: " + count);
                System.IO.StreamReader reader = System.IO.File.OpenText(file);
                string line;
                string sublineID = "";
                string sublineTitle = "";
                string sublineAuthor = "";
                string sublineText = "";
                while (reader.EndOfStream == false)
                {
                    string flagID = ".I";
                    string flagTitle = ".T";
                    string flagAuthor = ".A";
                    string flagText = ".W";
                    line = reader.ReadLine();
                    if (line.Contains(flagID))
                    {
                        sublineID = line.Substring(line.IndexOf(flagID) + 3);
                        //Console.WriteLine("ID " + sublineID);
                        //Console.ReadKey();
                    }
                    if (line.Contains(flagTitle))
                    {
                        line = reader.ReadLine();
                        while (line.Contains(flagAuthor) == false)
                        {
                            sublineTitle += line;
                            line = reader.ReadLine();
                        }
                        //Console.WriteLine("Title " + sublineTitle);
                        //Console.ReadKey();
                    }

                    if (line.Contains(flagAuthor))
                    {
                        line = reader.ReadLine();
                        while (line.Contains(flagText) == false)
                        {
                            sublineAuthor += line;
                            line = reader.ReadLine();
                        }
                        //Console.WriteLine("Author " + sublineAuthor);
                        //Console.ReadKey();
                    }
                    if (line.Contains(flagText))
                    {
                        line = reader.ReadLine(); // This is to skip the title in the text field
                        line = reader.ReadLine();
                        while (reader.EndOfStream == false)
                        {
                            sublineText += line;
                            line = reader.ReadLine();
                        }
                        //Console.WriteLine("Text " + sublineText);
                        //Console.ReadKey();
                        count += 1;
                    }
                }
                var doc = new Document();
                doc.Add(new Field("ID", sublineID.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                doc.Add(new Field("Title", sublineTitle, Field.Store.YES, Field.Index.ANALYZED));
                doc.Add(new Field("Author", sublineAuthor, Field.Store.YES, Field.Index.ANALYZED));
                doc.Add(new Field("Text", sublineText, Field.Store.YES, Field.Index.ANALYZED));
                writer.AddDocument(doc);
            }
        }

        public void CleanUpIndex()
        {
            writer.Optimize();
            writer.Flush(true, true, true);
            writer.Dispose();
        }

        public void CreateSearcher()
        {
            searcher = new IndexSearcher(indexDirectory);
        }

        public void CreateParser()
        {
            parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, TEXT_FN, analyzer);
        }

        public void CleanUpSearch()
        {
            searcher.Dispose();
        }

        public TopDocs SearchIndex(string querytext)
        {
            Console.WriteLine("Searching for " + querytext);
            querytext = querytext.ToLower();
            Query query = parser.Parse(querytext);
            TopDocs results = searcher.Search(query, 100);
            System.Console.WriteLine("Number of results is " + results.TotalHits);
            return results;

        }

        public void DisplaySearchResult(TopDocs results)
        {
            int rank = 0;
            foreach (ScoreDoc scoreDoc in results.ScoreDocs)
            {
                rank++;
                // retrieve the document from the 'ScoreDoc' object
                Lucene.Net.Documents.Document doc = searcher.Doc(scoreDoc.Doc);
                string myFieldValue = doc.Get(TEXT_FN).ToString();
                //Console.WriteLine("Rank " + rank + " score " + scoreDoc.Score + " text " + myFieldValue);
                Console.WriteLine("Rank " + rank + " text " + myFieldValue);
            }
        }

        public void SearchTextWithExplanation(string queryID, string querytext)
        {
            System.Console.WriteLine("Searching for query No." + queryID);
            querytext = querytext.ToLower();
            Query query = parser.Parse(querytext);

            TopDocs results = searcher.Search(query, 100);
            System.Console.WriteLine("Number of results is " + results.TotalHits);
            int rank = 0;
            foreach (ScoreDoc scoreDoc in results.ScoreDocs)
            {
                rank++;
                Lucene.Net.Documents.Document doc = searcher.Doc(scoreDoc.Doc);
                string myFieldValue = doc.Get(TEXT_FN).ToString();
                Console.WriteLine("Rank " + rank + " text " + myFieldValue);
                double score = scoreDoc.Score;
                Explanation ex = searcher.Explain(query, scoreDoc.Doc);
                Console.WriteLine(ex.ToString());
            }
        }

        public Dictionary<string, string> GetQuery(string queryPath)
        {
            string infilename = queryPath;
            System.IO.StreamReader reader = new System.IO.StreamReader(infilename);
            string line = "";
            string flagID = ".I";
            string flagDescription = ".D";
            string[] queryID = new string[5];
            string[] queryText = new string[5];
            Dictionary<string, string> myQuery = new Dictionary<string, string>();
            int countLine = 1;
            int countID = -1;
            while (line != null)
            {
                //Console.WriteLine("count" + countLine);
                //Console.WriteLine("line" + line);
                if (line == "")
                {
                    //skip
                }
                else if (line.Contains(flagID))
                {
                    countID += 1;
                    //Console.WriteLine("\ncountID " + countID);
                    queryID[countID] = line.Substring(line.IndexOf(flagID) + 3); // Skip the string ".I"
                    //Console.WriteLine("ID " + queryID[countID]);
                    //Console.ReadKey();
                }
                else if (line.Contains(flagDescription))
                {
                    // skip the string ".D"
                }
                else
                {
                    queryText[countID] += line;
                    //Console.WriteLine("Text " + queryText[countID]);
                    //Console.ReadKey();
                }
                countLine += 1;
                line = reader.ReadLine();
            }
            // Add IDs and Description to myQuery dictionary to return when the method is finished.
            /* A dictionary data structure is used to store IDs and Descriptions because C# does not
             allowed a method to return 2 arrays */
            int i;
            for (i = 0; i < 5; i++)
            {
                myQuery.Add(queryID[i], queryText[i]);
            }
            return myQuery;
        }

        static void Main(string[] args)
        {
            Project mySearchEngine = new Project();
            Console.WriteLine("Please enter the directory to the source documents");
            String sourceFolder = mySearchEngine.TakePath();
            Console.WriteLine("please enter the directory to the Index Folder");
            String indexPath = mySearchEngine.TakePath();
            // Build Index
            mySearchEngine.OpenIndexPath(indexPath);
            mySearchEngine.CreateAnalyzer();
            mySearchEngine.CreateWriter();

            //User Confirmation
            DialogResult ans = MessageBox.Show("Do you like to create the index from the specified collection ? ", "Please confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (ans == DialogResult.Yes)
            {
                // Index Text
                DateTime start = System.DateTime.Now;
                mySearchEngine.IndexText(sourceFolder);
                DateTime indexEnd = System.DateTime.Now;
                // Measuring Indexing execution time
                mySearchEngine.CleanUpIndex();
                Console.WriteLine("Time to index : " + (indexEnd - start));
                Console.WriteLine("Press any key to continue:");
                Console.ReadKey();
                // Create parser and searcher
                mySearchEngine.CreateSearcher();
                mySearchEngine.CreateParser();
            }
            else
            {
                Environment.Exit(0);
            }

            // Take queries
            Console.WriteLine("Please enter the directory to the query file");
            String queryPath = mySearchEngine.TakePath();
            Dictionary<string, string> myQuery = new Dictionary<string, string>();
            myQuery = mySearchEngine.GetQuery(queryPath);

            // Check if the dictionary holds the correct values
            foreach (KeyValuePair<string, string> entry in myQuery)
            {
                Console.WriteLine("Query ID: " + entry.Key);
                Console.WriteLine("Query Description: " + entry.Value);
            }
            Console.WriteLine("Press any key to continue:");
            Console.ReadKey();

            //Search Query
            foreach (KeyValuePair<string, string> entry in myQuery)
            {
                mySearchEngine.SearchTextWithExplanation(entry.Key, entry.Value);
            }
            Console.WriteLine("Press any key to continue:");
            Console.ReadKey();

        }
    }
}
