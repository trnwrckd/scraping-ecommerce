namespace intro
{
    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; } = String.Empty;
        public string Brand { get; set; } = String.Empty;
        public string BrandImgUrl { get; set; } = String.Empty;
        public int? Price { get; set; }
        public List<string>? ProductImgUrls { get; set; }
        public string[]? Colors { get; set; }
        public String Storage { get; set; } = String.Empty;
    }
}
