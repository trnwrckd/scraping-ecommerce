using HtmlAgilityPack;
using Newtonsoft.Json;

namespace intro
{
    class Program
    {
        public static int brandId = 0;
        public static List<Brand> brands = new List<Brand>();
        public static List<string> productUrls = new List<string>();
        public static int productId = 0;
        public static List<Product> products = new List<Product>();
        public static List<string> productImgUrls = new List<string>();

        public static void GetBrands(string url)
        {
            // extract brandName from URL
            var brand = url.Substring(url.Remove(url.Length - 1, 1).LastIndexOf('/') + 1);
            // add to list
            brands.Add(
                new Brand
                {
                    Id = brandId,
                    BrandName = brand.Remove(brand.Length - 1),
                    Url = url
                }
            );

            brandId++;
        }

        public static void ScrapeThisPage(String url)
        {
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
            var nextPageBtn = Page.DocumentNode.SelectNodes(
                "//nav/ul/li/a[@class = 'next page-numbers']"
            );
            if (nextPageBtn != null)
            {
                var nextPageUrl = nextPageBtn.First().Attributes["href"].Value;
                ScrapeThisPage(nextPageUrl);
            }
        }

        public static void ExtractJsonFromUrl(String url)
        {
            // load page
            HtmlWeb web = new HtmlWeb();
            HtmlDocument ProductPage = web.Load(url);
            // get data from page
            // title
            var title = ProductPage.DocumentNode.SelectNodes(
                "//h1[@class = 'product_title entry-title']"
            )[0].InnerText;

            // brand img
            var brandImgUrl = ProductPage.DocumentNode
                .SelectNodes("//div[@class= 'woodmart-product-brand']/a/img")
                ?[0].GetAttributeValue("src", "");

            // brand color storage
            string brand = "";
            string[] colors = { };
            string storage = "";

            var attributes = ProductPage.DocumentNode.SelectNodes(
                "//th[@class = 'woocommerce-product-attributes-item__label']"
            );
            var attributeValues = ProductPage.DocumentNode.SelectNodes(
                "//td[@class = 'woocommerce-product-attributes-item__value']"
            );

            if (attributes != null)
            {
                for (var i = 0; i < attributes.Count; i++)
                {
                    var attr = attributes[i].InnerText.ToLower();
                    if (attr == "color" || attr == "colour")
                    {
                        colors = attributeValues[i].InnerText.Split(",");
                    }
                    else if (attr == "brand")
                    {
                        brand = attributeValues[i].InnerText;
                    }
                    else if (attr == "variant" || attr == "storage" || attr == "device storage")
                    {
                        var temp = attributeValues[i].InnerText;
                        if (temp != "Official" || temp != "Unofficial")
                        {
                            storage = attributeValues[i].InnerText.Split(",")[0];
                        }
                    }
                }
            }

            // price
            var price = Convert.ToInt32(
                ProductPage.DocumentNode
                    .SelectNodes("//span[@class= 'woocommerce-Price-currencySymbol']")
                    ?[8].NextSibling.InnerText.Split(".")[0].Replace(",", "")
            );
            // images
            var productImgs = ProductPage.DocumentNode.SelectNodes(
                "//div[@class= 'product-images-inner']//img"
            );
            foreach (var img in productImgs)
            {
                var imgSrc = img.GetAttributeValue("src", "");
                productImgUrls.Add(imgSrc);
            }
            // append to product list
            products.Add(
                new Product
                {
                    Id = productId,
                    Title = title,
                    Brand = brand,
                    Price = price,
                    Storage = storage,
                    Colors = colors,
                    BrandImgUrl = brandImgUrl,
                    ProductImgUrls = productImgUrls
                }
            );
            productId++;
            productImgUrls = new List<string>();
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
            foreach (var productUrl in productUrls)
                ExtractJsonFromUrl(productUrl);

            // print total
            Console.WriteLine("\n" + brands.Count() + " Brands found.");
            Console.WriteLine(productUrls.Count() + " Products found.");

            // write brands to file
            // string productsJson = JsonConvert.SerializeObject(products.ToArray());
            // System.IO.File.WriteAllText(@"products.json", productsJson);

            // completed
            Console.WriteLine("Scraping Completed!");
        }
    }
}
