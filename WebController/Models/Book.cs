namespace WebController.Models
{
    public class Book
    {
        public string Title { get; set; }

        public string? Description { get; set; }

        public List<Page> Pages { get; private set; } = new List<Page>();

        public NamingTemplate NamingTemplate { get; set; }

        public Book(string title, NamingTemplate namingTemplate)
        {
            Title = title;
            NamingTemplate = namingTemplate;
        }

    }
}
