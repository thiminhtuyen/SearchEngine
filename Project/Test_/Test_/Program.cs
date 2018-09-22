using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_
{
    class Program
    {


        static void Main(string[] args)
        {
            string filesDirectory = @"H:\IR\Project\Test_\Test_collection";
            var txtFiles = System.IO.Directory.EnumerateFiles(filesDirectory, "*.txt");
            foreach (var file in txtFiles)
            {   
                Console.WriteLine("New Doc");
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
                        Console.WriteLine("Title " + sublineTitle);
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
                        Console.WriteLine("Author " + sublineAuthor);
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
                        Console.WriteLine("Text " + sublineText);
                        Console.ReadKey();
                    }
                }
            }
            //string test = "hello hehehe hahaha";
            //Console.WriteLine("Length " + test.Length);
            //string flag = "hehehe";
            //string substring = test.Substring(test.IndexOf(flag));
            //Console.WriteLine("Substring: " + substring);
            ////Console.ReadKey();
            //string file = @"H:\IR\Project\Test_\Test_collection\1.txt";
            //System.IO.StreamReader reader = System.IO.File.OpenText(file);
            //string line;
            //string sublineID = "";
            //string sublineTitle = "";
            //string sublineAuthor = "";
            //string sublineText = "";
            //while (reader.EndOfStream == false)
            //{
            //    string flagID = ".I";
            //    string flagTitle = ".T";
            //    string flagAuthor = ".A";
            //    string flagText = ".W";
            //    line = reader.ReadLine();
            //    if (line.Contains(flagID))
            //    {
            //        sublineID = line.Substring(line.IndexOf(flagID) + 3);
            //        Console.WriteLine("ID " + sublineID);
            //        //Console.ReadKey();
            //    }
            //    else if (line.Contains(flagTitle))
            //    {
            //        line = reader.ReadLine();
            //        while (line.Contains(flagAuthor) == false)
            //        {
            //            sublineTitle += line;
            //            line = reader.ReadLine();
            //        }
            //        Console.WriteLine("Title " + sublineTitle);
            //        //Console.ReadKey();
            //    }
            //    else if (line.Contains(flagAuthor))
            //    {
            //        line = reader.ReadLine();
            //        while (line.Contains(flagText) == false)
            //        {
            //            sublineAuthor += line;
            //            line = reader.ReadLine();
            //        }
            //        Console.WriteLine("Author " + sublineAuthor);
            //        //Console.ReadKey();
            //    }
            //    else if (line.Contains(flagText))
            //    {
            //        line = reader.ReadLine(); // This is to skip the title in the text field
            //        line = reader.ReadLine();
            //        while (reader.EndOfStream == false)
            //        {
            //            Console.WriteLine("test" + line);
            //            Console.ReadKey();
            //            sublineText += line;
            //            line = reader.ReadLine();
            //        }
            //        //Console.WriteLine("Text " + sublineText);
            //        //Console.ReadKey();
            //    }

            //    //sublineID = line.Substring(line.IndexOf(flagID) + 2, line.Length - line.IndexOf(flagTitle));
            //    //sublineTitle = line.Substring(line.IndexOf(flagTitle) + 2, line.Length - line.IndexOf(flagAuthor));
            //    //sublineAuthor = line.Substring(line.IndexOf(flagAuthor) + 2, line.Length - line.IndexOf(flagText));
            //    //sublineText = line.Substring(line.IndexOf(flagText) + 2);


        }
    }
}
    
