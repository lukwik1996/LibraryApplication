using Library.WebApi.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using CommonData.Models;

namespace Library.WebApi.Services
{
    public class LibraryService
    {
        private readonly IMongoCollection<Book> _books;
        private readonly IMongoCollection<User> _users;

        public LibraryService(ILibraryDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _books = database.GetCollection<Book>(settings.CollectionBooks);
            _users = database.GetCollection<User>(settings.CollectionUsers);
        }

        // books
        public List<Book> GetBooks() =>
            _books.Find(book => true).ToList();


        /* v NOT USED v */
        // filtering books by title/author/genre/release_year

        public List<Book> GetBooksFiltered(string title, string author, int genre, string minYear, string maxYear)
        {
            // input check
            bool checkYear = int.TryParse(minYear, out int minY);
            checkYear &= int.TryParse(maxYear, out int maxY);

            const int maxStringLength = 20;

            title = title.Substring(maxStringLength);
            author = author.Substring(maxStringLength);

            List<Book> allBooks = GetBooks();
            List<Book> resultBooks = new List<Book>();

            // need to set execution time limit (fail fast) 
            foreach (var book in allBooks)
            {
                if (checkYear)
                {
                    int releaseYear = book.ReleaseYear;

                    if (releaseYear < minY || releaseYear > maxY)
                        continue;
                }

                if (!ContainsAllWords(title, book.Title))
                    continue;

                if (!ContainsAllWords(author, book.Author))
                    continue;

                resultBooks.Add(book);
            }

            return resultBooks;
        }

        bool ContainsAllWords(string userInput, string content)
        {
            string[] words = userInput.Split(' ');

            foreach (var word in words)
            {
                if (!content.Contains(word))
                    return false;
            }

            return true;
        }

        /* ^ NOT USED ^ */
        

        public List<Book> GetAvailableBooks() =>
            _books.Find(book => book.Renter == null).ToList();

        public List<Book> GetRentedBooks(string id) =>
            _books.Find(book => book.Renter == id).ToList();

        public Book GetBook(string id) =>
            _books.Find<Book>(book => book.Id == id).FirstOrDefault();

        public Book AddBook(Book book)
        {
            _books.InsertOne(book);
            return book;
        }

        public void BookUpdate(string id, Book bookIn) =>
            _books.ReplaceOne(book => book.Id == id, bookIn);

        public void BookRemove(string id) =>
            _books.DeleteOne(book => book.Id == id);


        // users

        public User GetUser(string id) =>
            _users.Find<User>(user => user.Id == id).FirstOrDefault();

        public bool Register(User user)
        {
            user.Password = HashPassword(user.Password);

            if (_users.Find<User>(usr => usr.Login == user.Login).FirstOrDefault() != null)
            {
                return false;
            }

            _users.InsertOne(user);
            return true;
        }

        public User Login(string login, string password)
        {
            password = HashPassword(password);
            return _users.Find<User>(user => user.Login == login && user.Password == password).FirstOrDefault();
        }

        public bool UpdatePassword(string id, UserUpdate userIn)
        {
            if (!CheckPassword(id, userIn.OldPassword))
            {
                return false;
            }

            var user = GetUser(id);

            if (userIn.NewPassword == userIn.RepeatNewPassword && userIn.NewPassword != null)
            {
                user.Password = HashPassword(userIn.NewPassword);
            }
            else
            {
                return false;
            }

            _users.ReplaceOne(userToUpdate => userToUpdate.Id == id, user);

            return true;
        }

        public bool UpdateEmail(string id, UserUpdate userIn)
        {
            if (!CheckPassword(id, userIn.OldPasswordEmail))
            {
                return false;
            }

            var user = GetUser(id);
            
            if (userIn.NewEmail != null)
            {
                user.Email = userIn.NewEmail;
            }
            else
            {
                return false;
            }

            _users.ReplaceOne(userToUpdate => userToUpdate.Id == id, user);
            return true;
        }

        public bool CheckPassword(string id, string password)
        {
            var user = _users.Find(usr => usr.Id == id).FirstOrDefault();
            password = HashPassword(password);
            
            return user != null && user.Password == password;
        }

        public string HashPassword(string password)
        {
            if (password == null)
            {
                return null;
            }

            SHA256 mySha256 = SHA256.Create();
            var passwordBytes = mySha256.ComputeHash(Encoding.ASCII.GetBytes(password));

            return Encoding.UTF8.GetString(passwordBytes, 0, passwordBytes.Length);
        }
    }
}
