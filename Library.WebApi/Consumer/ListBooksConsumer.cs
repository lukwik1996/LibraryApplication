using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommonData.Messages;
using CommonData.Models;
using Library.WebApi.Services;
using MassTransit;

namespace Library.WebApi.Consumer
{
    public class ListBooksConsumer : Consumer<Book>
    {
        public ListBooksConsumer(LibraryService libraryService) : base(libraryService)
        {
        }

        public override async Task Consume(ConsumeContext<IMessage<Book>> context)
        {
            Debug.WriteLine($"Consumer ListBooks odebral wiadomosc: {context.Message.Action}");

            try
            {
                if (context.SentTime.Value.AddSeconds(30) < DateTime.UtcNow)
                {
                    return;
                }
                
                var books = new List<Book>();

                if (context.Message.Action == Variables.ActionAll)
                {
                    books = _libraryService.GetBooks();
                }

                else if (context.Message.Action == Variables.ActionAvailable)
                {
                    books = _libraryService.GetAvailableBooks();
                }

                else if (context.Message.Action == Variables.ActionRented && context.Message.UserId != null)
                {
                    books = _libraryService.GetRentedBooks(context.Message.UserId);
                }

                else if (context.Message.Action == Variables.ActionDetails)
                {
                    books.Add(_libraryService.GetBook(context.Message.MessageContent.FirstOrDefault().Id));
                }

                else
                {
                    return;
                }

                if (books.Any())
                {
                    await context.RespondAsync<Message<Book>>(new MessageResponse<Book>()
                    {
                        Action = context.Message.Action,
                        StatusCode = 200,
                        MessageContent = books
                    });
                }
                else
                {
                    await context.RespondAsync<Message<Book>>(new MessageResponse<Book>()
                    {
                        Action = context.Message.Action,
                        StatusCode = 400
                    });
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error : {e.Message}\n\n" + $"{e.StackTrace}");
            }
        }
    }
}
