using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crawler
{
    class Functions
    {
        //create folder
        public void create_dir(String path)
        {
            System.IO.Directory.CreateDirectory(path);
            // if folder already exists, this line will be ignored
             
        }
        //create a new file
        public void write_file(String path, String data)
        {
            System.IO.File.WriteAllText(path, data);
            //create a file
        }

        //create queue and crawled files
        public void create_data_files(String proj_name, String base_url)
        {
            String queue_path = proj_name + "/queue.txt";
            String crawled_path = proj_name + "/crawled.txt";
            if(!System.IO.File.Exists(queue_path))
                //if file does not exist, create it
                write_file(queue_path, base_url);
            //path, what data is going to go into the file
            //queue and crawled must not be empty when created for the first time
            //when it first boots up, it has the base url in the queue
            if (!System.IO.File.Exists(crawled_path))
                //if file does not exist, create it
                write_file(crawled_path, "");
            //crawled file must be created empty
        }

        //add data into existing file
        public void append_to_file(String path, String data)
        {
            System.IO.File.AppendAllText(path, data+'\n');
            //opens a file, appends, closes
        }

        //delete contents of a file
        public void delete_file_content(String path)
        {
            System.IO.File.WriteAllText(path,"");
            //overwrite file content with nothing
        }

        //read file and convert each line in set items
        public HashSet<String> file_to_set(String path)
        {
            HashSet<String> results = new HashSet<string>();
            string[] lines = System.IO.File.ReadAllLines(path);
            foreach (String line in lines)
                results.Add(line.Remove(line.Length-1));
            //remove \n at the end of each line
            return results;
        }

        //iterate trough a set, each line-> new line in file
        public void set_to_file(HashSet<String> links, String file_path)
        {
            delete_file_content(file_path);
            foreach(String link in links)
                System.IO.File.AppendAllText(file_path,link);

        }
    }

    class LinkFinder
    {
        
    }

    class Program
    {
        

        static void Main(string[] args)
        {
            String path = @"D:\FOLDERS\PROGRAMMING\CSHARP\web crawler 3\databases";

            Functions f = new Functions();
            f.create_dir(path);
            f.create_data_files(path, "www.cprarad.ro");
        }
    }
}
