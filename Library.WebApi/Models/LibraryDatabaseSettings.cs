namespace Library.WebApi.Models
{
    public class LibraryDatabaseSettings : ILibraryDatabaseSettings
    {
        public string CollectionBooks { get; set; }
        public string CollectionUsers { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface ILibraryDatabaseSettings
    {
        string CollectionBooks { get; set; }
        string CollectionUsers { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}