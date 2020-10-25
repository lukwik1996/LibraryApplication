using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommonData.Messages;
using CommonData.Models;
using Library.WebApi.Services;
using MassTransit;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Library.WebApi.Consumer
{
    public class EditBookDbConsumer : Consumer<Book>
    {
        public EditBookDbConsumer(LibraryService libraryService) : base(libraryService)
        {
        }

        public override async Task Consume(ConsumeContext<IMessage<Book>> context)
        {
            Debug.WriteLine($"Consumer EditBookDb odebral wiadomosc: {context.Message.Action}");

            try
            {
                if (context.SentTime.Value.AddSeconds(30) < DateTime.UtcNow)
                {
                    return;
                }

                if (context.Message.MessageContent.Any())
                {
                    if (context.Message.Action == Variables.ActionAdd)
                    {
                        var bookToAdd = context.Message.MessageContent.FirstOrDefault();

                        _libraryService.AddBook(bookToAdd);
                    }

                    else if (context.Message.Action == Variables.ActionEdit)
                    {
                        var newBook = context.Message.MessageContent.FirstOrDefault();

                        if (newBook.Id == null)
                        {
                            return;
                        }

                        var bookToEdit = _libraryService.GetBook(newBook.Id);

                        newBook.Renter = bookToEdit.Renter;
                        newBook.RentDate = bookToEdit.RentDate;
                        newBook.ReturnDate = bookToEdit.ReturnDate;

                        _libraryService.BookUpdate(newBook.Id, newBook);
                    }

                    else if (context.Message.Action == Variables.ActionRemove)
                    {
                        var bookToRemove = context.Message.MessageContent.FirstOrDefault();
                        _libraryService.BookRemove(bookToRemove.Id);
                    }

                    else if (context.Message.Action == Variables.ActionRent)
                    {
                        var bookToRent = context.Message.MessageContent.FirstOrDefault();

                        if (bookToRent.Id == null)
                        {
                            return;
                        }

                        var bookInDatabase = _libraryService.GetBook(bookToRent.Id);

                        bookInDatabase.Renter = bookToRent.Renter;
                        bookInDatabase.RentDate = bookToRent.RentDate;
                        bookInDatabase.ReturnDate = bookToRent.ReturnDate;

                        _libraryService.BookUpdate(bookInDatabase.Id, bookInDatabase);
                    }

                    else if (context.Message.Action == Variables.ActionReturn)
                    {
                        var bookToReturn = context.Message.MessageContent.FirstOrDefault();

                        if (bookToReturn.Id == null)
                        {
                            return;
                        }

                        var bookInDatabase = _libraryService.GetBook(bookToReturn.Id);

                        // TODO dodac ksiazke do historii wypozyczen

                        bookInDatabase.Renter = null;
                        bookInDatabase.RentDate = null;
                        bookInDatabase.ReturnDate = null;

                        _libraryService.BookUpdate(bookInDatabase.Id, bookInDatabase);
                    }

                    else
                    {
                        return;
                    }

                    await context.RespondAsync<MessageResponse<Book>>(new MessageResponse<Book>()
                    {
                        Action = context.Message.Action,
                        StatusCode = 200
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
