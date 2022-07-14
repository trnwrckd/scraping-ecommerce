using HtmlAgilityPack;
using Newtonsoft.Json;

namespace intro
{
    class Program
    {
        public class Row
        {
            public string? Title { get; set; }
            
        }
        public class ProductCategory{
            public int Id {get; set;}
            public string? Category {get;set;}
            public string? Url {get;set;}
        }

        static void Main(string[] args)
        {
            HtmlWeb web = new HtmlWeb();
            DotNetEnv.Env.Load();
            HtmlDocument doc = web.Load(Environment.GetEnvironmentVariable("WEBSITE_URL"));

            // category 
            var CategoryAnchors = doc.DocumentNode.SelectNodes("/html/body/div[1]/header/div/div[3]/div/div/div[1]/div/div/div/div/ul/li    /a[@class = 'woodmart-nav-link']");
            
            var categories = new List<ProductCategory>();
            var categoryId = 0;
            foreach(var anchor in CategoryAnchors){
                var url = anchor.Attributes["href"].Value;
                var category = url.Substring(url.Remove(url.Length - 1, 1).LastIndexOf('/') + 1);
                categories.Add(new ProductCategory{
                    Id = categoryId,
                    Category = category.Remove(category.Length - 1),
                    Url = url
                });
                categoryId++;
            }
            
            string json = JsonConvert.SerializeObject(categories.ToArray());

            //write string to file
            System.IO.File.WriteAllText(@"categories.json", json);
            // completed
            Console.WriteLine("Scraping Completed!");
        }
    }
}
