using HtmlAgilityPack;
using Newtonsoft.Json;

namespace intro
{
    class Program
    {
        public static List<ProductBrand>  brands = new List<ProductBrand>();
        public static int brandId = 0;
        public static List<string>  productUrls = new List<string>();
        public static void GetBrands (string url){
            // extract brandName from URL
            var brand = url.Substring(url.Remove(url.Length - 1, 1).LastIndexOf('/') + 1);
            // add to list
            brands.Add(
                new ProductBrand
                {
                    Id = brandId,
                    Brand = brand.Remove(brand.Length - 1),
                    Url = url
                }
            );

            brandId++;
        }
        public static void ScrapeThisPage( String url ){
            // scrape current page and get links for all product
            HtmlWeb web = new HtmlWeb();
            HtmlDocument Page = web.Load(url);
            var ProductAnchors = Page.DocumentNode.SelectNodes(
                "//div/h3[@class = 'product-title']/a"
            );
            foreach (var productAnchor in ProductAnchors)
            {
                var productUrl = productAnchor.Attributes["href"].Value;
                productUrls.Add(productUrl);
            }

            // if there's a next page, scrape that too
            var nextPageBtn = Page.DocumentNode.SelectNodes("//nav/ul/li/a[@class = 'next page-numbers']");
            if(nextPageBtn != null){
                var nextPageUrl = nextPageBtn.First().Attributes["href"].Value;
                ScrapeThisPage(nextPageUrl);
            }
        }
        public static void ExtractJsonFromUrl( string url ){
            // create product class
            // analyze what infos can be extracted from site
            // append to product list
        }
        static void Main(string[] args)
        {
            DotNetEnv.Env.Load();
            HtmlWeb web = new HtmlWeb();
            var SiteUrl = Environment.GetEnvironmentVariable("WEBSITE_URL");
            HtmlDocument doc = web.Load(SiteUrl);

            // getting category list and urls from sideNav
            var BrandAnchors = doc.DocumentNode.SelectNodes(
                "/html/body/div[1]/header/div/div[3]/div/div/div[1]/div/div/div/div/ul/li/a[@class = 'woodmart-nav-link']"
            );

            // get all brands
            foreach (var anchor in BrandAnchors)
            {
                var url = anchor.Attributes["href"].Value;
                GetBrands(url);
            }
            
            // removing accessories for now
            brands.RemoveAt(brands.Count - 1);

            // for each brand get all products
            foreach (var brand in brands)
                ScrapeThisPage(brand.Url);

            // for each product extract data
            foreach(var productUrl in productUrls)
                ExtractJsonFromUrl(productUrl);
            
            // print total 
            Console.WriteLine(brands.Count() + " Brands found.");
            Console.WriteLine(productUrls.Count() + " Products found.");

            //write brands to file
                // string brandsListJson = JsonConvert.SerializeObject(brands.ToArray());
                // System.IO.File.WriteAllText(@"brands.json", brandsListJson);
                
            // completed
            Console.WriteLine("Scraping Completed!");
        }
    }
}
