using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using HtmlAgilityPack;

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

        public List<String> file_to_list(String path)
        {
            List<String> results = new List<string>();
            string[] lines = System.IO.File.ReadAllLines(path);
            foreach (String line in lines)
                results.Add(line.Remove(line.Length-1));
            return results;
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
                System.IO.File.AppendAllText(file_path,link+"\n");
                                                   //added \n ^
        }
    }

    class LinkFinder
    {
        //get url and return all links from it (good format, really nice)
        private String url;
        private HashSet<String> links = new HashSet<string>();
        public LinkFinder(String url)
        {
            this.url = url;
        }

        public HashSet<String> get_links()
        {
            Uri uri;
            uri = new Uri(this.url);
           
            String scheme = uri.Scheme;
            string host = uri.Host;
            String path = uri.LocalPath;
            String absolute_path = uri.AbsolutePath;

            String buff;

            HtmlWeb web = new HtmlWeb();
            web.UserAgent = "Linkbot 0.1";

            HtmlDocument htmldoc = web.Load(this.url);


            foreach (HtmlNode link in htmldoc.DocumentNode.SelectNodes("//a[@href]"))
            {
                //check if url has already been crawled
                HtmlAttribute att = link.Attributes["href"];   // url => string att.Value
                if (att.Value.Length < 2)
                    continue;
                if (att.Value[0] == '/' && att.Value[1] != '/')
                    //hrefTags.Add(scheme + ":/" + att.Value);
                    buff = scheme + ":/" + att.Value;
                else if (att.Value[0] == '/' && att.Value[1] == '/')
                    //refTags.Add(scheme + ":" + att.Value);
                    buff = scheme + ":" + att.Value;
                else if (att.Value[0] == '.' && att.Value[1] == '/')
                    //hrefTags.Add(scheme + "://" + host + att.Value.Substring(1));
                    buff = scheme + "://" + host + att.Value.Substring(1);
                else if (att.Value[0] == '#')
                    //hrefTags.Add(scheme + "://" + host + path + att.Value);
                    buff = scheme + "://" + host + path + att.Value;
                else if (att.Value[0] == '.' && att.Value[1] == '.' && att.Value[2] == '/')
                    //hrefTags.Add(scheme + "://" + host + "/" + att.Value);
                    buff = scheme + "://" + host + "/" + att.Value;
                else if (att.Value.Contains("javascript:"))
                    continue;
                else if (att.Value.Substring(0, 5) != "https" && att.Value.Substring(0, 4) != "http")
                    //hrefTags.Add(scheme + "://" + host + "/" + att.Value);
                    buff = scheme + "://" + host + "/" + att.Value;
                else
                    buff = att.Value;
                this.links.Add(buff);
            }
            return links;
            //returns a list containing all the links from a page (raw)
        }
    }

    class Spider
    {
        private static String project_name = "", base_url = "", queue_file = "", crawled_file = "";//, domain_name = "";
        private static HashSet<String> queue = new HashSet<string>();
        private static HashSet<String> crawled = new HashSet<string>();
        //class variables

        public Spider(String project_name_in, String base_url_in)//, String domain_name_in)
        {
            project_name = project_name_in;
            base_url = base_url_in;
           // domain_name = domain_name_in;
            queue_file = project_name + "/queue.txt";
            crawled_file = project_name + "/crawled.txt";

            boot();
            crawl_page("first spider", base_url);
        }

        public static void boot()
        {
            //if you are the very first spider, you have to first create the proj directory, create the 2 data files
            Functions FilesFolders = new Functions();
            FilesFolders.create_dir(project_name);
            FilesFolders.create_data_files(project_name, base_url);

            //don't read from file everytime, read from file and convert to set/list
            queue = FilesFolders.file_to_set(queue_file);
            crawled = FilesFolders.file_to_set(crawled_file);
        }

        public static void crawl_page(String thread_name, String page_url)
        {
            if(!crawled.Contains(page_url))
            {
                Console.WriteLine(thread_name + " crawling " + page_url);
                Console.WriteLine("Queue "+queue.Count.ToString()+" | Crawled "+crawled.Count.ToString());
                Spider.add_links_to_queue(Spider.gather_links(page_url));
                Spider.queue.Remove(page_url);
                Spider.crawled.Add(page_url);
                //now we need to update the files
                Spider.update_files();
            }
        }

        public static HashSet<String> gather_links(String url)
        {
            try
            {
                LinkFinder linkfinder = new LinkFinder(url);
                HashSet<String> links = linkfinder.get_links();
                return links;
                //return set of links
            }catch
            {
                Console.WriteLine("Can not crawl page "+url);
                HashSet<String> emptyset = new HashSet<string>();
                return emptyset;
                //return emptyset
            }
        }

        public static void add_links_to_queue(HashSet<String> links)
        {
            foreach(String url in links)
            {
                //make sure they ar enot in waiting list, crawled list
                if (Spider.queue.Contains(url))
                    continue;
                if (Spider.crawled.Contains(url))
                    continue;

                /*
                 * if you want to crawl a specific website, test here for domain
                 */
                Spider.queue.Add(url);
            }
        }

        public static void update_files()
        {
            Functions FilesFolders = new Functions();
            FilesFolders.set_to_file(Spider.queue, Spider.queue_file);
            FilesFolders.set_to_file(Spider.crawled, Spider.crawled_file);
        }

    }

    class Webcrawler
    {

        public static String path = @"D:\FOLDERS\PROGRAMMING\CSHARP\web crawler 3\databases";

        public static String PROJECT_NAME = "webcrawler";
        public static String HOME_PAGE = "http://www.cprarad.ro/";
        public static String QUEUE_FILE = path + "/queue.txt";
        public static String CRAWLED_FILE = path + "/crawled.txt";
        public static int NUMBER_OF_THREADS = 2;

        Spider spider1 = new Spider(path, HOME_PAGE);
        static Queue<String> queue = new Queue<String>();
        //thread queue
        public void crawl()
        {

            //Spider spider1 = new Spider(path, HOME_PAGE);
            //first spider

            //check if there are items in the queue file, the crawl
            Functions FilesFolders = new Functions();
            HashSet<String> queued_links = new HashSet<string>();
            queued_links = FilesFolders.file_to_set(QUEUE_FILE);
            if (queued_links.Count > 0)
                Console.WriteLine("crwaling...");
                create_jobs();
        }

        public void create_jobs()
        {
            Functions FilesFolders = new Functions();
            foreach (String link in FilesFolders.file_to_set(QUEUE_FILE))
            {
                queue.Enqueue(link);
            }
            //queue.Join();
            crawl();
        }

        public void create_workers()
        {
            for (int i = 0; i < NUMBER_OF_THREADS; ++i)
            {
                Thread t = new Thread(work);
                t.IsBackground = true;
                t.Start();
            }
        }

        public void work()
        {
            //do the next job in the queue

            while (true)
            {
                String url = queue.Dequeue();
                Spider.crawl_page(Thread.CurrentThread.Name, url);
                //task_done
            }
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            // Queue<>
            //threa queue

            //the first thing ut needs to do, is to crawl homepage
            // Spider spider1 = new Spider(path, HOME_PAGE);

            /*HashSet<String> test = new HashSet<string>();
            LinkFinder finder = new LinkFinder("http://www.cprarad.ro/");
            test=finder.get_links();
            foreach(String link in test)
            {
                Console.WriteLine(link);
            }
            */

            Webcrawler webcrawler1 = new Webcrawler();

            webcrawler1.create_workers();
            webcrawler1.crawl();


        }
    }
}