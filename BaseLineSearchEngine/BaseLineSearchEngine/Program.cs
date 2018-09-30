using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.QueryParsers;

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
        public string[] stopWords = { "a", "an", "and", "are", "as", "at", "be", "but", "by", "for", "if", "in", "into", "is", "it", "no", "not", "of", "on", "or", "such", "that", "the", "their", "then", "there", "these", "they", "this", "to", "was", "will", "with" };


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
                string line = "";
                while (reader.EndOfStream == false)
                {
                    line += reader.ReadLine();
                }
                
                var doc = new Document();
                doc.Add(new Field(TEXT_FN, line, Field.Store.YES, Field.Index.ANALYZED));
                writer.AddDocument(doc);
            }
            count += 1;
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

        public string GetInfoDescription(Dictionary<string, string> myQuery, string infoId)
        {
            string description = "";
            foreach (KeyValuePair<string, string> entry in myQuery)
            {
                if (entry.Key == infoId)
                {
                    description = entry.Value;
                    break;
                }
            }
            return description;
        }

        //Tokenising the information needs(Purpose:to remove stop words)
        public string[] TokeniseDescription(string description)
        {
            // stub
            char[] delims = new char[] { ' ', '\t', '\'', '"', '-', '(', ')', ',', '’', '\n', ':', ';', '?', '.', '!' };
            return description.ToLower().Split(delims, StringSplitOptions.RemoveEmptyEntries);
        }

        //Remove Stop words
        public string[] StopWordFilter(string[] descriptionTokens)
        {

            int numTokens = descriptionTokens.Count();
            List<string> filteredTokens = new List<string>();
            for (int i = 0; i < numTokens; i++)
            {
                string token = descriptionTokens[i];
                if (!stopWords.Contains(token) && (token.Length > 2))
                    filteredTokens.Add(token);
            }
            return filteredTokens.ToArray<string>();
        }

        public void WriteToResultFile(string resultPath, string queryID, TopDocs results)
        {
            string delimiter = " ";
            string filename = "result_trec_eval.txt";
            string filepath = resultPath + filename;
            Console.WriteLine("File Path" + filepath);
            int rank = 0;
            string flagID = ".ID";
            Console.WriteLine(System.IO.File.Exists(filepath) == false);
            if (System.IO.File.Exists(filepath) == false)
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filepath, false))
                {
                    foreach (ScoreDoc scoreDoc in results.ScoreDocs)
                    {
                        Console.WriteLine("HERERE");
                        rank++;
                        Lucene.Net.Documents.Document doc = searcher.Doc(scoreDoc.Doc);
                        string myFieldValue = doc.Get(TEXT_FN).ToString();
                        string docID = myFieldValue.Substring(myFieldValue.IndexOf(flagID) + 4, myFieldValue.IndexOf('.', myFieldValue.IndexOf('.') + 1) - (myFieldValue.IndexOf(' ', 0)));
                        Console.WriteLine("docID" + docID);
                        writer.WriteLine(docID + delimiter + " " + "Q0" + delimiter + queryID + delimiter + rank + delimiter + scoreDoc.Score + delimiter + "BaseLineSystem");
                    }
                    writer.Flush();
                    writer.Close();
                }
            }
            else
            {
                Console.WriteLine("EXISSSTTTTTTTTTTTTTTTTTTTT");
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filepath, true))
                {
                    foreach (ScoreDoc scoreDoc in results.ScoreDocs)
                    {
                        rank++;
                        Lucene.Net.Documents.Document doc = searcher.Doc(scoreDoc.Doc);
                        string myFieldValue = doc.Get(TEXT_FN).ToString();
                        string docID = myFieldValue.Substring(myFieldValue.IndexOf(flagID) + 4, myFieldValue.IndexOf('.', myFieldValue.IndexOf('.') + 1) - (myFieldValue.IndexOf(' ', 0)));
                        writer.WriteLine(docID + delimiter + "Q0" + delimiter + queryID + delimiter + rank + delimiter + scoreDoc.Score + delimiter + "BaseLineSystem");
                    }
                    writer.Flush();
                    writer.Close();
                }
            }
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

            //Create Query
            Console.WriteLine("Please enter the infomation Id:");
            string infoNeedId = Console.ReadLine();
            string description = mySearchEngine.GetInfoDescription(myQuery, infoNeedId);
            Console.WriteLine(description);

            //Remove Stop words
            string[] Tokens = mySearchEngine.TokeniseDescription(description);
            string[] DescriptionTokens = mySearchEngine.StopWordFilter(Tokens);


            //test
            //string des = string.Join(" ", DescriptionTokens);
            //mySearchEngine.SearchTextWithExplanation(infoNeedId, description);
            TopDocs results = mySearchEngine.SearchIndex(description);
            mySearchEngine.DisplaySearchResult(results);
            Console.WriteLine("Press any key to continue:");
            Console.ReadKey();

            // Write results to trec_eval format file
            Console.WriteLine("Where do you want to save the results? Please enter the directory");
            string resultPath = mySearchEngine.TakePath();
            mySearchEngine.WriteToResultFile(resultPath, infoNeedId, results);
            Console.ReadKey();
        }
    }
}
