using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommonData.Messages;
using CommonData.Models;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly IRequestClient<Message<Book>> _bookClient;
        private readonly IRequestClient<MessageResponse<Book>> _bookResponseClient;

        private const string SessionStatus = "_Status";
        private const string SessionName = "_Name";
        private const string SessionUserEmail = "_Email";
        private const string SessionUserId = "_UserID";
        

        public BooksController( IRequestClient<Message<Book>> bookClient,
                                IRequestClient<MessageResponse<Book>> bookResponseClient )
        {
            _bookClient = bookClient;
            _bookResponseClient = bookResponseClient;
        }

        [HttpGet]
        public async Task<IActionResult> ListAvailable()
        {
            ViewBag.userID = HttpContext.Session.GetString(SessionUserId);
            ViewBag.status = HttpContext.Session.GetInt32(SessionStatus);
            ViewBag.fullName = HttpContext.Session.GetString(SessionName);
            ViewBag.Email = HttpContext.Session.GetString(SessionUserEmail);

            var response = await _bookClient.GetResponse<Message<Book>>(
                new
                {
                    Action = Variables.ActionAvailable
                });

            return View(response.Message);
        }

        [HttpGet]
        public async Task<IActionResult> ListAll()
        {
            var userId = HttpContext.Session.GetString(SessionUserId);
            if (userId == "")
            {
                return RedirectToAction("Login", "Users");
            }

            ViewBag.userID = HttpContext.Session.GetString(SessionUserId);
            ViewBag.fullName = HttpContext.Session.GetString(SessionName);
            ViewBag.Email = HttpContext.Session.GetString(SessionUserEmail);

            try
            {
                var response = await _bookClient.GetResponse<Message<Book>>(
                    new
                    {
                        Action = Variables.ActionAll
                    });

                if (response.Message.StatusCode == 200)
                {
                    var books = response.Message.MessageContent;

                    return View(books);
                }
                else
                {
                    ViewBag.ErrorMsg = "There was a problem retrieving book list.";
                }

                return View();

            }
            catch (RequestTimeoutException timeException)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(timeException.Message);
                ViewBag.ErrorMsg = "Your request timed out.";
                return View();
            }
            catch (Exception e)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(e.Message);
                ViewBag.ErrorMsg = "Unknown error occured.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Rented(string id)
        {
            var userId = HttpContext.Session.GetString(SessionUserId);
            if (userId == "")
            {
                return RedirectToAction("Login", "Users");
            }

            ViewBag.userID = HttpContext.Session.GetString(SessionUserId);
            ViewBag.fullName = HttpContext.Session.GetString(SessionName);
            ViewBag.Email = HttpContext.Session.GetString(SessionUserEmail);
            ViewBag.status = HttpContext.Session.GetInt32(SessionStatus);

            try
            {
                var response = await _bookClient.GetResponse<Message<Book>>(
                    new
                    {
                        Action = Variables.ActionRented,
                        UserId = userId
                    });

                if (response.Message.StatusCode == 200)
                {
                    return View(response.Message);
                }
                else
                {
                    ViewBag.ErrorMsg = "There was a problem retrieving book list.";
                }

                return View(response.Message);

            }
            catch (RequestTimeoutException timeException)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(timeException.Message);
                ViewBag.ErrorMsg = "Your request timed out.";
                return RedirectToAction("ListAvailable");
            }
            catch (Exception e)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(e.Message);
                ViewBag.ErrorMsg = "Unknown error occured.";
                return RedirectToAction("ListAvailable");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Rent(string id)
        {
            var userId = HttpContext.Session.GetString(SessionUserId);
            if (userId == "")
            {
                return RedirectToAction("Login", "Users");
            }

            ViewBag.fullName = HttpContext.Session.GetString(SessionName);
            ViewBag.Email = HttpContext.Session.GetString(SessionUserEmail);
            ViewBag.userID = HttpContext.Session.GetString(SessionUserId);
            ViewBag.status = HttpContext.Session.GetInt32(SessionStatus);

            var bookMessage = new List<Book>()
            {
                new Book()
                {
                    Id = id
                }
            };

            try
            {
                var response = await _bookClient.GetResponse<Message<Book>>(
                    new
                    {
                        Action = Variables.ActionDetails,
                        MessageContent = bookMessage
                    });

                if (response.Message.StatusCode == 200)
                {
                    var book = response.Message.MessageContent.FirstOrDefault();

                    return View(book);
                }
                else
                {
                    Debug.WriteLine("There was a problem retrieving the book from database.");
                }

                return RedirectToAction("ListAvailable");

            }
            catch (RequestTimeoutException timeException)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(timeException.Message);
                ViewBag.ErrorMsg = "Your request timed out.";
                return RedirectToAction("ListAvailable");
            }
            catch (Exception e)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(e.Message);
                ViewBag.ErrorMsg = "Unknown error occured.";
                return RedirectToAction("ListAvailable");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Rent(string id, Book book)
        {
            var today = DateTime.Today;

            book.Renter = HttpContext.Session.GetString(SessionUserId);
            book.RentDate = today.ToString("yyyy MMMM dd");
            book.ReturnDate = today.AddDays(21).ToString("yyyy MMMM dd");

            var bookMessage = new List<Book>()
            {
                new Book()
                {
                    Id = id,
                    Renter = book.Renter,
                    RentDate = book.RentDate,
                    ReturnDate = book.ReturnDate
                }
            };

            try
            {
                var response = await _bookResponseClient.GetResponse<MessageResponse<Book>>(
                    new
                    {
                        Action = Variables.ActionRent,
                        UserId = book.Renter,
                        MessageContent = bookMessage
                    });

                if (response.Message.StatusCode == 200)
                {
                    return RedirectToAction("Rented", new {id = book.Renter});
                }
                else
                {
                    Debug.WriteLine("There was a problem renting the book.");
                }

                return RedirectToAction("ListAvailable");

            }
            catch (RequestTimeoutException timeException)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(timeException.Message);
                ViewBag.ErrorMsg = "Your request timed out.";
                return RedirectToAction("ListAvailable");
            }
            catch (Exception e)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(e.Message);
                ViewBag.ErrorMsg = "Unknown error occured.";
                return RedirectToAction("ListAvailable");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Return(string id)
        {
            var userId = HttpContext.Session.GetString(SessionUserId);
            if (userId == "")
            {
                return RedirectToAction("Login", "Users");
            }

            ViewBag.fullName = HttpContext.Session.GetString(SessionName);
            ViewBag.Email = HttpContext.Session.GetString(SessionUserEmail);
            ViewBag.userID = HttpContext.Session.GetString(SessionUserId);
            ViewBag.status = HttpContext.Session.GetInt32(SessionStatus);

            var bookMessage = new List<Book>()
            {
                new Book()
                {
                    Id = id
                }
            };

            try
            {
                var response = await _bookClient.GetResponse<Message<Book>>(
                    new
                    {
                        Action = Variables.ActionDetails,
                        UserId = userId,
                        MessageContent = bookMessage
                    });

                if (response.Message.StatusCode == 200)
                {
                    var book = response.Message.MessageContent.FirstOrDefault();

                    return View(book);
                }
                else
                {
                    Debug.WriteLine("There was a problem retrieving the book from database.");
                }

                return RedirectToAction("Rented");

            }
            catch (RequestTimeoutException timeException)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(timeException.Message);
                ViewBag.ErrorMsg = "Your request timed out.";
                return RedirectToAction("Rented");
            }
            catch (Exception e)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(e.Message);
                ViewBag.ErrorMsg = "Unknown error occured.";
                return RedirectToAction("Rented");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReturnBook(string id)
        {
            var user = HttpContext.Session.GetString(SessionUserId);
            
            var bookMessage = new List<Book>()
            {
                new Book()
                {
                    Id = id
                }
            };

            try
            {
                var response = await _bookResponseClient.GetResponse<MessageResponse<Book>>(
                    new
                    {
                        Action = Variables.ActionReturn,
                        MessageContent = bookMessage
                    });

                if (response.Message.StatusCode == 200)
                {
                    return RedirectToAction("Rented", new { id = user});
                }
                else
                {
                    Debug.WriteLine("There was a problem returning the book.");
                }

                return RedirectToAction("ListAvailable");

            }
            catch (RequestTimeoutException timeException)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(timeException.Message);
                ViewBag.ErrorMsg = "Your request timed out.";
                return RedirectToAction("ListAvailable");
            }
            catch (Exception e)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(e.Message);
                ViewBag.ErrorMsg = "Unknown error occured.";
                return RedirectToAction("ListAvailable");
            }
        }
        
        [HttpGet]
        public IActionResult Add()
        {
            var userId = HttpContext.Session.GetString(SessionUserId);
            if (userId == "")
            {
                return RedirectToAction("Login", "Users");
            }

            ViewBag.fullName = HttpContext.Session.GetString(SessionName);
            ViewBag.Email = HttpContext.Session.GetString(SessionUserEmail);
            ViewBag.userID = HttpContext.Session.GetString(SessionUserId);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(string id, Book book)
        {
            ViewBag.fullName = HttpContext.Session.GetString(SessionName);
            ViewBag.Email = HttpContext.Session.GetString(SessionUserEmail);
            ViewBag.userID = HttpContext.Session.GetString(SessionUserId);
            ViewBag.status = HttpContext.Session.GetInt32(SessionStatus);

            var bookMessage = new List<Book>()
            {
                new Book()
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    Genre = book.Genre,
                    ReleaseYear = book.ReleaseYear,
                    Isbn = book.Isbn
                }
            };

            try
            {
                var response = await _bookResponseClient.GetResponse<MessageResponse<Book>>(
                    new
                    {
                        Action = Variables.ActionAdd,
                        MessageContent = bookMessage
                    });

                if (response.Message.StatusCode == 200)
                {
                    return RedirectToAction("ListAll");
                }
                else
                {
                    Debug.WriteLine("There was a problem adding the book.");
                }

                return RedirectToAction("ListAll");

            }
            catch (RequestTimeoutException timeException)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(timeException.Message);
                ViewBag.ErrorMsg = "Your request timed out.";
                return RedirectToAction("ListAll");
            }
            catch (Exception e)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(e.Message);
                ViewBag.ErrorMsg = "Unknown error occured.";
                return RedirectToAction("ListAll");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            ViewBag.fullName = HttpContext.Session.GetString(SessionName);
            ViewBag.Email = HttpContext.Session.GetString(SessionUserEmail);
            ViewBag.userID = HttpContext.Session.GetString(SessionUserId);
            ViewBag.status = HttpContext.Session.GetInt32(SessionStatus);

            var bookMessage = new List<Book>()
            {
                new Book()
                {
                    Id = id
                }
            };

            try
            {
                var response = await _bookClient.GetResponse<Message<Book>>(
                    new
                    {
                        Action = Variables.ActionDetails,
                        MessageContent = bookMessage
                    });

                if (response.Message.StatusCode == 200)
                {
                    var book = response.Message.MessageContent.FirstOrDefault();

                    return View(book);
                }
                else
                {
                    Debug.WriteLine("There was a problem retrieving the book from database.");
                }

                return RedirectToAction("ListAvailable");

            }
            catch (RequestTimeoutException timeException)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(timeException.Message);
                ViewBag.ErrorMsg = "Your request timed out.";
                return RedirectToAction("ListAvailable");
            }
            catch (Exception e)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(e.Message);
                ViewBag.ErrorMsg = "Unknown error occured.";
                return RedirectToAction("ListAvailable");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DetailsRented(string id)
        {
            var userId = HttpContext.Session.GetString(SessionUserId);
            if (userId == "")
            {
                return RedirectToAction("Login", "Users");
            }

            ViewBag.fullName = HttpContext.Session.GetString(SessionName);
            ViewBag.Email = HttpContext.Session.GetString(SessionUserEmail);
            ViewBag.userID = HttpContext.Session.GetString(SessionUserId);
            ViewBag.status = HttpContext.Session.GetInt32(SessionStatus);

            var bookMessage = new List<Book>()
            {
                new Book()
                {
                    Id = id
                }
            };

            try
            {
                var response = await _bookClient.GetResponse<Message<Book>>(
                    new
                    {
                        Action = Variables.ActionDetails,
                        MessageContent = bookMessage
                    });

                if (response.Message.StatusCode == 200)
                {
                    var book = response.Message.MessageContent.FirstOrDefault();

                    return View(book);
                }
                else
                {
                    Debug.WriteLine("There was a problem retrieving the book from database.");
                }

                return RedirectToAction("Rented");

            }
            catch (RequestTimeoutException timeException)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(timeException.Message);
                ViewBag.ErrorMsg = "Your request timed out.";
                return RedirectToAction("Rented");
            }
            catch (Exception e)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(e.Message);
                ViewBag.ErrorMsg = "Unknown error occured.";
                return RedirectToAction("Rented");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var userId = HttpContext.Session.GetString(SessionUserId);
            if (userId == "")
            {
                return RedirectToAction("Login", "Users");
            }

            ViewBag.fullName = HttpContext.Session.GetString(SessionName);
            ViewBag.Email = HttpContext.Session.GetString(SessionUserEmail);
            ViewBag.userID = HttpContext.Session.GetString(SessionUserId);

            var bookMessage = new List<Book>()
            {
                new Book()
                {
                    Id = id
                }
            };

            try
            {
                var response = await _bookClient.GetResponse<Message<Book>>(
                    new
                    {
                        Action = Variables.ActionDetails,
                        MessageContent = bookMessage
                    });

                if (response.Message.StatusCode == 200)
                {
                    var book = response.Message.MessageContent.FirstOrDefault();

                    return View(book);
                }
                else
                {
                    Debug.WriteLine("There was a problem retrieving the book from database.");
                }

                return RedirectToAction("ListAll");

            }
            catch (RequestTimeoutException timeException)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(timeException.Message);
                ViewBag.ErrorMsg = "Your request timed out.";
                return RedirectToAction("ListAll");
            }
            catch (Exception e)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(e.Message);
                ViewBag.ErrorMsg = "Unknown error occured.";
                return RedirectToAction("ListAll");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, Book book)
        {
            var bookMessage = new List<Book>()
            {
                new Book()
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    Genre = book.Genre,
                    ReleaseYear = book.ReleaseYear,
                    Isbn = book.Isbn
                }
            };

            try
            {
                var response = await _bookResponseClient.GetResponse<MessageResponse<Book>>(
                    new
                    {
                        Action = Variables.ActionEdit,
                        MessageContent = bookMessage
                    });

                if (response.Message.StatusCode == 200)
                {
                    return RedirectToAction("ListAll");
                }
                else
                {
                    Debug.WriteLine("There was a problem editing the book.");
                }

                return RedirectToAction("ListAll");

            }
            catch (RequestTimeoutException timeException)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(timeException.Message);
                ViewBag.ErrorMsg = "Your request timed out.";
                return RedirectToAction("ListAll");
            }
            catch (Exception e)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(e.Message);
                ViewBag.ErrorMsg = "Unknown error occured.";
                return RedirectToAction("ListAll");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Remove(string id)
        {
            var userId = HttpContext.Session.GetString(SessionUserId);
            if (userId == "")
            {
                return RedirectToAction("Login", "Users");
            }

            var bookMessage = new List<Book>()
            {
                new Book()
                {
                    Id = id
                }
            };

            try
            {
                var response = await _bookResponseClient.GetResponse<MessageResponse<Book>>(
                    new
                    {
                        Action = Variables.ActionRemove,
                        MessageContent = bookMessage
                    });

                if (response.Message.StatusCode == 200)
                {
                    return RedirectToAction("ListAll");
                }
                else
                {
                    Debug.WriteLine("There was a problem removing the book.");
                }

                return RedirectToAction("ListAll");

            }
            catch (RequestTimeoutException timeException)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(timeException.Message);
                ViewBag.ErrorMsg = "Your request timed out.";
                return RedirectToAction("ListAll");
            }
            catch (Exception e)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(e.Message);
                ViewBag.ErrorMsg = "Unknown error occured.";
                return RedirectToAction("ListAll");
            }
        }
    }
}