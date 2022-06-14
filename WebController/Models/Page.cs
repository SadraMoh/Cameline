namespace WebController.Models
{
    public class Page
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public Page(string name, string url)
        {
            Name = name;
            Url = url;
        }
    }
}
