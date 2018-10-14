using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Store;
using Lucene.Net.QueryParsers;
using System.IO;
using Syn.WordNet;
using CustomizedSeachEngine;

namespace Project
{
    class Project
    {
        Lucene.Net.Store.Directory indexDirectory;
        Lucene.Net.Analysis.Analyzer analyzer;
        Lucene.Net.Index.IndexWriter writer;
        Lucene.Net.Search.IndexSearcher searcher;
        Lucene.Net.QueryParsers.QueryParser parser;
        Similarity customizedSimilarity;
        Lucene.Net.QueryParsers.MultiFieldQueryParser mtfParser;

        const Lucene.Net.Util.Version VERSION = Lucene.Net.Util.Version.LUCENE_30;
        const string TEXT_FN = "text";
        public string[] stopWords = { "a", "an", "and", "are", "as", "at", "be", "but", "by", "can", "could", "for", "have", "if", "in", "into", "is", "it", "must", "no", "not", "of", "on", "or", "should", "such", "that", "the", "their", "then", "there", "these", "they", "this", "to", "was", "will", "with", "what", "when", "why", "where", "how" };
        FileInfo stopWordList = new FileInfo(@"H:\IR\Project\SearchEngine\CustomizedSearchEngine\Project\collection\stopwords.txt");

        public Project()
        {
            indexDirectory = null;
            analyzer = null;
            writer = null;
        }

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

        public void CreateKeyWordAnalyzer()
        {
            analyzer = new Lucene.Net.Analysis.KeywordAnalyzer();

        }

        public void CreateStandardAnalyzer()
        {
            analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(VERSION);

        }

        public void CreateCustomizedAnalyzer()
        {
            analyzer = new Lucene.Net.Analysis.Snowball.SnowballAnalyzer(VERSION, "English");
            
        }
        public void CreateCustomizedAnalyzerNewSim()
        {
            analyzer = new Lucene.Net.Analysis.Snowball.SnowballAnalyzer(VERSION, "English");
            customizedSimilarity = new NewSimilarity();
            
        }

        public void CreateWriter()
        {
            IndexWriter.MaxFieldLength mfl = new IndexWriter.MaxFieldLength(IndexWriter.DEFAULT_MAX_FIELD_LENGTH);
            writer = new Lucene.Net.Index.IndexWriter(indexDirectory, analyzer, true, mfl);
            
        }

        public void CreateWriterNewSim()
        {
            IndexWriter.MaxFieldLength mfl = new IndexWriter.MaxFieldLength(IndexWriter.DEFAULT_MAX_FIELD_LENGTH);
            writer = new Lucene.Net.Index.IndexWriter(indexDirectory, analyzer, true, mfl);
            writer.SetSimilarity(customizedSimilarity);
        }

        /************************************************************************************************************************************************************************************
         
             INDEX DOCUMENTS
            
        ************************************************************************************************************************************************************************************/

        public void IndexTextCustomized(string filesDirectory)
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
                Field IDField = new Field("ID", sublineID.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                Field titleField = new Field("Title", sublineTitle, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                Field authorField = new Field("Author", sublineAuthor, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                Field textField = new Field("Text", sublineText, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                Field fullText = new Field("Full Text", (sublineID + sublineTitle + sublineAuthor + sublineText), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                doc.Add(IDField);
                doc.Add(titleField);
                doc.Add(authorField);
                doc.Add(textField);
                doc.Add(fullText);
                writer.AddDocument(doc);
                count += 1;
            }
        }
        public void IndexTextCustomizedBoosted(string filesDirectory)
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
                        Console.WriteLine("ID " + sublineID);
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
                Field IDField = new Field("ID", sublineID.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                Field titleField = new Field("Title", sublineTitle, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                Field authorField = new Field("Author", sublineAuthor, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                Field textField = new Field("Text", sublineText, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                Field fullText = new Field("Full Text", (sublineID + sublineTitle + sublineAuthor + sublineText), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                doc.Add(IDField);
                doc.Add(titleField);
                doc.Add(authorField);
                doc.Add(textField);
                doc.Add(fullText);
                titleField.Boost = 2;
                authorField.Boost = 2;
                writer.AddDocument(doc);
                count += 1;
            }
        }

        public void IndexTextBaseLine(string filesDirectory)
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
                reader.Close();
                count += 1;
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

        public void CreateSearcherNewSim()
        {
            searcher = new IndexSearcher(indexDirectory);
            searcher.Similarity = customizedSimilarity;
        }

        public void CreateParser(string field)
        {
            parser = new QueryParser(VERSION, field, analyzer);
        }

        public void CreateMTFParser()
        {
            mtfParser = new MultiFieldQueryParser(VERSION, new string[] { "Title", "Author", "Text" }, analyzer);
        }

        public void CleanUpSearch()
        {
            searcher.Dispose();
        }

        /************************************************************************************************************************************************************************************
         
             GENERATE QUERY
            
        ************************************************************************************************************************************************************************************/
        /// <summary>
        /// Import queries from the given file
        /// Store the queries in a dictionary with Key is the query ID and value is the query description
        /// </summary>
        /// <param name="queryPath"></param>
        /// <returns> Dictionary of queries </returns>
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
                if (line == "")
                {
                    //skip
                }
                else if (line.Contains(flagID))
                {
                    countID += 1;
                    queryID[countID] = line.Substring(line.IndexOf(flagID) + 3); // Skip the string ".I"
                }
                else if (line.Contains(flagDescription))
                {
                    // skip the string ".D"
                }
                else
                {
                    queryText[countID] += line;
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

        /// <summary>
        /// Get queries from the "Information needs file"
        /// </summary>
        /// <param name="myQuery"></param>
        /// <param name="infoId"></param>
        /// <returns></returns>
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


        public Query GenerateQuery(string querytext)
        {
            Console.WriteLine("Searching for" + querytext);
            querytext = querytext.ToLower();
            Query query = parser.Parse(querytext);
            return query;
        }



        /************************************************************************************************************************************************************************************
         
             SEARCHING
            
        ************************************************************************************************************************************************************************************/

        public TopDocs SearchIndexAsIs(string querytext)
        {

            Console.WriteLine("Searching for " + querytext);
            querytext = querytext.ToLower();
            Query query = parser.Parse(querytext);
            TopDocs results = searcher.Search(query, 100);
            System.Console.WriteLine("Number of results is " + results.TotalHits);
            return results;
        }
        public TopDocs SearchIndexBaseLine(string querytext)
        {
            Console.WriteLine("Searching for " + querytext);
            querytext = querytext.ToLower();
            Query query = parser.Parse(querytext);
            //Console.WriteLine(query);
            TopDocs results = searcher.Search(query, 100);
            System.Console.WriteLine("Number of results is " + results.TotalHits);
            return results;
        }

        public TopDocs SearchIndexCustomizedFT(string querytext)
        {
            Console.WriteLine("Searching for " + querytext);
            querytext = querytext.ToLower();
            Query query = parser.Parse(querytext);
            //Console.WriteLine(query);
            TopDocs results = searcher.Search(query, 100);
            System.Console.WriteLine("Number of results is " + results.TotalHits);
            return results;
        }

        public TopDocs SearchIndexCustomizedMTF(string querytext)
        {
            Console.WriteLine("Searching for " + querytext);
            querytext = querytext.ToLower();
            Query query = mtfParser.Parse(querytext);
            //Console.WriteLine(query);
            TopDocs results = searcher.Search(query, 100);
            System.Console.WriteLine("Number of results is " + results.TotalHits);
            return results;
        }


        public TopDocs SearchTextWithExplanation(string queryID, string querytext)
        {
            System.Console.WriteLine("Searching for query No." + queryID);
            querytext = querytext.ToLower();
            Query query = parser.Parse(querytext);
            //Console.WriteLine(query);
            TopDocs results = searcher.Search(query, 10);
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
            return results;
        }


        /************************************************************************************************************************************************************************************
         
             EXPANDED QUERY
            
        ************************************************************************************************************************************************************************************/
        public string GetExpandedQuery(string query)
        {
            var directory = System.IO.Directory.GetCurrentDirectory();

            var wordNet = new WordNetEngine();

            wordNet.AddDataSource(new StreamReader(Path.Combine(directory, "data.adj")), PartOfSpeech.Adjective);
            wordNet.AddDataSource(new StreamReader(Path.Combine(directory, "data.adv")), PartOfSpeech.Adverb);
            wordNet.AddDataSource(new StreamReader(Path.Combine(directory, "data.noun")), PartOfSpeech.Noun);
            wordNet.AddDataSource(new StreamReader(Path.Combine(directory, "data.verb")), PartOfSpeech.Verb);

            wordNet.AddIndexSource(new StreamReader(Path.Combine(directory, "index.adj")), PartOfSpeech.Adjective);
            wordNet.AddIndexSource(new StreamReader(Path.Combine(directory, "index.adv")), PartOfSpeech.Adverb);
            wordNet.AddIndexSource(new StreamReader(Path.Combine(directory, "index.noun")), PartOfSpeech.Noun);
            wordNet.AddIndexSource(new StreamReader(Path.Combine(directory, "index.verb")), PartOfSpeech.Verb);

            Console.WriteLine("Loading database...");
            wordNet.Load();
            Console.WriteLine("Load completed.");
            //Split the query into an array of tokens
            string[] tokens = query.Split(' ');
            string finalQuery = "";
            foreach (string word in tokens)
            {
                var synSetList = wordNet.GetSynSets(word);

                if (synSetList.Count == 0)
                {
                    Console.WriteLine($"No SynSet found for '{word}'");
                    finalQuery = finalQuery + " " + word;
                }

                foreach (var synSet in synSetList)
                {
                    var words = string.Join(" ", synSet.Words);

                    Console.WriteLine($"\nWords: {words}");
                    Console.WriteLine($"POS: {synSet.PartOfSpeech}");
                    Console.WriteLine($"Gloss: {synSet.Gloss}");
                    finalQuery = finalQuery + " " + words;
                }
            }
            return query;
        }


        public string GetWeightedExpandedQuery(string query)
        {
            var directory = System.IO.Directory.GetCurrentDirectory();

            var wordNet = new WordNetEngine();

            wordNet.AddDataSource(new StreamReader(Path.Combine(directory, "data.adj")), PartOfSpeech.Adjective);
            wordNet.AddDataSource(new StreamReader(Path.Combine(directory, "data.adv")), PartOfSpeech.Adverb);
            wordNet.AddDataSource(new StreamReader(Path.Combine(directory, "data.noun")), PartOfSpeech.Noun);
            wordNet.AddDataSource(new StreamReader(Path.Combine(directory, "data.verb")), PartOfSpeech.Verb);

            wordNet.AddIndexSource(new StreamReader(Path.Combine(directory, "index.adj")), PartOfSpeech.Adjective);
            wordNet.AddIndexSource(new StreamReader(Path.Combine(directory, "index.adv")), PartOfSpeech.Adverb);
            wordNet.AddIndexSource(new StreamReader(Path.Combine(directory, "index.noun")), PartOfSpeech.Noun);
            wordNet.AddIndexSource(new StreamReader(Path.Combine(directory, "index.verb")), PartOfSpeech.Verb);

            Console.WriteLine("Loading database...");
            wordNet.Load();
            Console.WriteLine("Load completed.");
            //Split the query into an array of tokens
            string[] tokens = query.Split(' ');
            string finalQuery = "";
            foreach (string word in tokens)
            {
                var synSetList = wordNet.GetSynSets(word);
                if (synSetList.Count == 0)
                {
                    Console.WriteLine($"No SynSet found for '{word}'");
                    finalQuery = finalQuery + " " + word;
                }
                else
                {
                    bool first = true;
                    foreach (var synSet in synSetList)
                    {
                        var words = string.Join(" ", synSet.Words);
                        string[] word_split = words.Split(' ');
                        foreach (string w in word_split)
                        {
                            while (first)
                            {
                                finalQuery = finalQuery + " " + w + "^5";
                                first = false;
                            }
                            finalQuery = finalQuery + " " + w;
                            Console.WriteLine($"\nWords: {words}");
                            Console.WriteLine($"POS: {synSet.PartOfSpeech}");
                            Console.WriteLine($"Gloss: {synSet.Gloss}");

                        }
                    }
                    Console.WriteLine("final" + finalQuery);

                }
            }
            return finalQuery;
        }

        /************************************************************************************************************************************************************************************
         
             DISPLAY RESULTS
            
        ************************************************************************************************************************************************************************************/

        public void DisplaySearchResult(TopDocs results)
        {
            int rank = 0;
            foreach (ScoreDoc scoreDoc in results.ScoreDocs)
            {
                rank++;
                // retrieve the document from the 'ScoreDoc' object
                Lucene.Net.Documents.Document doc = searcher.Doc(scoreDoc.Doc);
                string myFieldValue = doc.Get("Full Text").ToString();
                //Console.WriteLine("Rank " + rank + " score " + scoreDoc.Score + " text " + myFieldValue);
                Console.WriteLine("Rank " + rank + " text " + myFieldValue);
            }
        }



        //public void PageResult(int pageNumber, Query query)
        //{
        //    int resultPerPage = 10;
        //    int page = pageNumber;
        //    TopDocs results = searcher.Search(query, (page + 1) * resultPerPage);
        //    Highlighter highlighter = new Highlighter(new QueryScorer(query));
        //    SimpleFragmenter fragmenter = new SimpleFragmenter(50);
        //    int maxNumFragmentsRequired = 2;
        //    int i = 0;
        //    foreach (ScoreDoc scoreDoc in results.ScoreDocs)
        //    {
        //        i++;
        //        Lucene.Net.Documents.Document doc = searcher.Doc(scoreDoc.Doc);
        //        string myFieldValue = doc.Get(TEXT_FN).ToString();
        //        TokenStream tokenStream = analyzer.TokenStream(TEXT_FN, new System.IO.StringReader(myFieldValue));
        //        string fragment = highlighter.GetBestFragments(tokenStream, myFieldValue, maxNumFragmentsRequired, "...");

        //        if (i < page * resultPerPage)
        //        {
        //            continue;
        //        }
        //        else if (i < (page + 1) * resultPerPage && i < results.TotalHits)
        //        {
        //            //System.IO.StringReader reader = new System.IO.StringReader(myFieldValue);
        //            //string line = "";
        //            //string flagText = ".W";
        //            //line = reader.ReadLine();
        //            //Console.WriteLine(line);
        //            //while (line.Contains(flagText) == false)
        //            //{
        //            //    Console.WriteLine(line);
        //            //    line = reader.ReadLine();
        //            //}
        //            //line = reader.ReadLine(); // Skip the title in the abstract
        //            //line = reader.ReadLine(); // Read the first line in the abstract
        //            //Console.WriteLine(line + "..."); // Output the first line of the abstract to SO
        //            //continue;
        //            Console.WriteLine("\t" + fragment);
        //        }
        //        else { break; }
        //    }
        //}

        /************************************************************************************************************************************************************************************

            WRITE RESULTS TO FILE

       ************************************************************************************************************************************************************************************/
        public void WriteToResultFile(string resultPath, string queryID, TopDocs results, string searchType, string name)
        {
            string delimiter = " ";
            string filename = "\\" + name + ".txt";
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
                        string docID = myFieldValue.Substring(myFieldValue.IndexOf(flagID) + 3, myFieldValue.IndexOf('.', myFieldValue.IndexOf('.') + 1) - (myFieldValue.IndexOf(' ', 0)));
                        Console.WriteLine("docID" + docID);
                        writer.WriteLine(docID + delimiter + "Q0" + delimiter + queryID + delimiter + rank + delimiter + scoreDoc.Score + delimiter + searchType);
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
                        string docID = myFieldValue.Substring(myFieldValue.IndexOf(flagID) + 3, myFieldValue.IndexOf('.', myFieldValue.IndexOf('.') + 1) - (myFieldValue.IndexOf(' ', 0)));
                        writer.WriteLine(docID + delimiter + "Q0" + delimiter + queryID + delimiter + rank + delimiter + scoreDoc.Score + delimiter + searchType);
                    }
                    writer.Flush();
                    writer.Close();
                }
            }
        }

        public void WriteToResultFileCustomized(string resultPath, string queryID, TopDocs results, string searchType, string name)
        {
            string delimiter = " ";
            string filename = "\\" + name + ".txt";
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
                        string docID = doc.Get("ID").ToString();
                        Console.WriteLine("docID" + docID);
                        writer.WriteLine(docID + delimiter + "Q0" + delimiter + queryID + delimiter + rank + delimiter + scoreDoc.Score + delimiter + searchType);
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
                        string docID = doc.Get("ID").ToString();
                        writer.WriteLine(docID + delimiter + "Q0" + delimiter + queryID + delimiter + rank + delimiter + scoreDoc.Score + delimiter + searchType);
                    }
                    writer.Flush();
                    writer.Close();
                }
            }
        }
        static void Main(string[] args)
        {
            Project mySearchEngine = new Project();
            String sourceFolder = Prompt.ShowDialog("Please enter the directory to the source document", "Source Documents");
            String indexPath1 = Prompt.ShowDialog("please enter the directory to the Index Folder for BaseLine - As Is", "Index Path");
            String indexPath2 = Prompt.ShowDialog("please enter the directory to the Index Folder for BaseLine - Standard", "Index Path");
            String indexPath3 = Prompt.ShowDialog("please enter the directory to the Index Folder for Customized system", "Index Path");
            String indexPath4 = Prompt.ShowDialog("please enter the directory to the Index Folder for Customized system _ New Sim", "Index Path");
            String indexPath5 = Prompt.ShowDialog("please enter the directory to the Index Folder for Customized system - Full Text", "Index Path");
            String queryPath = Prompt.ShowDialog("Please enter the directory to the query file", "Query Path");
            String resultPath = Prompt.ShowDialog("Where do you want to save the results? Please enter the directory", "Result Path");


            //String sourceFolder = @"H:\IR\Project\SearchEngine\CustomizedSearchEngine\Project\collection\crandocs";
            //String indexPath1 = @"H:\IR\Project\SearchEngine\CustomizedSearchEngine\Project\collection\IndexAsIs";
            //String indexPath2 = @"H:\IR\Project\SearchEngine\CustomizedSearchEngine\Project\collection\IndexBL";
            //String indexPath3 = @"H:\IR\Project\SearchEngine\CustomizedSearchEngine\Project\collection\IndexCustomized";
            //String indexPath4 = @"H:\IR\Project\SearchEngine\CustomizedSearchEngine\Project\collection\IndexCustomizedNewSim";
            //String indexPath5 = @"H:\IR\Project\SearchEngine\CustomizedSearchEngine\Project\collection\IndexCustomizedFullText";
            //String queryPath = @"H:\IR\Project\SearchEngine\CustomizedSearchEngine\Project\collection\cran_information_needs.txt";
            //String resultPath = @"C:\Users\n9861998\source\repos\";


            // Take queries
            Dictionary<string, string> myQuery = new Dictionary<string, string>(); // Initialize a dictionary to store the queries
            myQuery = mySearchEngine.GetQuery(queryPath);                          // Populate the Dictionary by importing queries from the given file

            /*
             AUTOMATIC SYSTEM
             Automatically search the query and write the results to files for evaluation
            */

            /*
             BASELINE SYSTEM
             */

            // Search query as is
            // Build Index
            mySearchEngine.OpenIndexPath(indexPath1);
            mySearchEngine.CreateKeyWordAnalyzer();
            mySearchEngine.CreateWriter();
            // Index Text
            DateTime startIndex1 = System.DateTime.Now;
            mySearchEngine.IndexTextBaseLine(sourceFolder);
            DateTime indexEnd1 = System.DateTime.Now;
            mySearchEngine.CleanUpIndex();
            TimeSpan indexTime1 = indexEnd1 - startIndex1;

            mySearchEngine.CreateSearcher();
            mySearchEngine.CreateParser(TEXT_FN);

            foreach (KeyValuePair<string, string> entry in myQuery)
            {
                var result1 = mySearchEngine.SearchIndexAsIs(entry.Value);
                mySearchEngine.WriteToResultFile(resultPath, entry.Key, result1, "As Is", "AsIsResult");

            }


            //Search with Standard Analyzer

            //Build Index
            mySearchEngine.OpenIndexPath(indexPath2);
            mySearchEngine.CreateStandardAnalyzer();
            mySearchEngine.CreateWriter(false);

            // Index Text
            DateTime startIndex2 = System.DateTime.Now;
            mySearchEngine.IndexTextBaseLine(sourceFolder);
            DateTime indexEnd2 = System.DateTime.Now;
            mySearchEngine.CleanUpIndex();
            TimeSpan indexTime2 = indexEnd2 - startIndex2;

            mySearchEngine.CreateSearcher(false);
            mySearchEngine.CreateParser(TEXT_FN);

            foreach (KeyValuePair<string, string> entry in myQuery)
            {
                // Search query with BaseLine system
                var result2 = mySearchEngine.SearchIndexBaseLine(entry.Value);
                mySearchEngine.WriteToResultFile(resultPath, entry.Key, result2, "BaseLine System", "BaseLineResult");
            }


            /*
             CUSTOMIZED SYSTEM
             */
            //Without new similairty - Full Text
            mySearchEngine.OpenIndexPath(indexPath5);
            mySearchEngine.CreateCustomizedAnalyzer();
            mySearchEngine.CreateWriter();

            DateTime startIndex5 = System.DateTime.Now;
            mySearchEngine.IndexTextCustomized(sourceFolder);
            DateTime indexEnd5 = System.DateTime.Now;
            mySearchEngine.CleanUpIndex();
            TimeSpan indexTime5 = indexEnd5 - startIndex5;

            mySearchEngine.CreateSearcher();
            mySearchEngine.CreateParser("Full Text");
            //Search Query
            foreach (KeyValuePair<string, string> entry in myQuery)
            {
                // Search query with Customized System
                string query = mySearchEngine.GetWeightedExpandedQuery(entry.Value);
                var result5 = mySearchEngine.SearchIndexCustomizedFT(query);
                mySearchEngine.DisplaySearchResult(result5);
                mySearchEngine.WriteToResultFileCustomized(resultPath, entry.Key, result5, "Customized System FT", "CustomizedResultFullText");
            }

            // Without new similarity - MTF
            // Build Index
            mySearchEngine.OpenIndexPath(indexPath3);
            mySearchEngine.CreateCustomizedAnalyzer();
            mySearchEngine.CreateWriter();

            DateTime startIndex3 = System.DateTime.Now;
            mySearchEngine.IndexTextCustomized(sourceFolder);
            DateTime indexEnd3 = System.DateTime.Now;
            mySearchEngine.CleanUpIndex();
            TimeSpan indexTime3 = indexEnd3 - startIndex3;

            mySearchEngine.CreateSearcher();
            mySearchEngine.CreateMTFParser();
            //Search Query
            foreach (KeyValuePair<string, string> entry in myQuery)
            {
                // Search query with Customized System
                string query = mySearchEngine.GetWeightedExpandedQuery(entry.Value);
                var result3 = mySearchEngine.SearchIndexCustomizedMTF(query);
                mySearchEngine.DisplaySearchResult(result3);
                mySearchEngine.WriteToResultFileCustomized(resultPath, entry.Key, result3, "Customized System", "CustomizedResult");
            }

            // With new similarity
            mySearchEngine.OpenIndexPath(indexPath4);
            mySearchEngine.CreateCustomizedAnalyzerNewSim();
            mySearchEngine.CreateWriterNewSim();

            DateTime startIndex4 = System.DateTime.Now;
            mySearchEngine.IndexTextCustomized(sourceFolder);
            DateTime indexEnd4 = System.DateTime.Now;
            mySearchEngine.CleanUpIndex();
            TimeSpan indexTime4 = indexEnd4 - startIndex4;

            mySearchEngine.CreateSearcherNewSim();
            mySearchEngine.CreateMTFParser();
            //mySearchEngine.CreateMultiFieldParser();
            //Search Query
            foreach (KeyValuePair<string, string> entry in myQuery)
            {
                // Search query with Customized System
                string query = mySearchEngine.GetWeightedExpandedQuery(entry.Value);
                var result4 = mySearchEngine.SearchIndexCustomizedMTF(query);
                mySearchEngine.DisplaySearchResult(result4);
                mySearchEngine.WriteToResultFileCustomized(resultPath, entry.Key, result4, "Customized System", "CustomizedResultNewSim");
            }
            Console.WriteLine("******* END CODE *********");
            Console.ReadKey();

            /*
              INTERACTIVE SYSTEM
              Let the user freely interact with the system
            */

            ////Create Query
            //String infoNeedId = Prompt.ShowDialog("Please enter the infomation Id", "Take query ID");
            ////Console.WriteLine("Please enter the infomation Id:");
            ////string infoNeedId = Console.ReadLine();
            //string description = mySearchEngine.GetInfoDescription(myQuery, infoNeedId);
            //Console.WriteLine(description);


            //// Display results to user by page
            //Console.WriteLine("Please enter the page number");
            //string input = Console.ReadLine();
            //int pageNumber = Convert.ToInt16(input);
            //Query query = mySearchEngine.GenerateQuery(description);
            //Console.WriteLine("Query " + query);
            //mySearchEngine.PageResult(pageNumber, query);
            //Console.ReadKey();



            ////User Confirmation
            //DialogResult ans = MessageBox.Show("Do you like to create the index from the specified collection ? ", "Please confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (ans == DialogResult.Yes)
            //{
            //    // Index Text
            //    DateTime start = System.DateTime.Now;
            //    mySearchEngine.IndexText(sourceFolder);
            //    DateTime indexEnd = System.DateTime.Now;
            //    // Measuring Indexing execution time
            //    mySearchEngine.CleanUpIndex();
            //    Console.WriteLine("Time to index : " + (indexEnd - start));
            //    Console.WriteLine("Press any key to continue:");
            //    Console.ReadKey();
            //    // Create parser and searcher
            //    mySearchEngine.CreateSearcher();
            //    mySearchEngine.CreateParser();
            //}
            //else
            //{
            //    Environment.Exit(0);
            //}


        }
    }
}
