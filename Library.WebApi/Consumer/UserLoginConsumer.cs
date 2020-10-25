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
    public class UserLoginConsumer : Consumer<User>
    {
        public UserLoginConsumer(LibraryService libraryService) : base(libraryService)
        {
        }

        public override async Task Consume(ConsumeContext<IMessage<User>> context)
        {
            Debug.WriteLine($"Consumer UserLogin odebral wiadomosc: {context.Message.Action}");

            try
            {
                if (context.SentTime.Value.AddSeconds(30) < DateTime.UtcNow)
                {
                    return;
                }

                if (context.Message.MessageContent.Any() && context.Message.Action.Any())
                {
                    var users = new List<User>();

                    if (context.Message.Action == Variables.ActionLogin)
                    {
                        var user = _libraryService.Login(context.Message.MessageContent.FirstOrDefault().Login,
                            context.Message.MessageContent.FirstOrDefault().Password);

                        if (user != null)
                        {
                            users.Add(user);
                        }
                    }

                    else if (context.Message.Action == Variables.ActionUpdate)
                    {
                        var user = _libraryService.GetUser(context.Message.MessageContent.FirstOrDefault().Id);

                        users.Add(user);
                    }

                    else
                    {
                        return;
                    }

                    if (users.Any())
                    {
                        await context.RespondAsync<Message<User>>(new MessageResponse<User>()
                        {
                            Action = context.Message.Action,
                            StatusCode = 200,
                            MessageContent = users
                        });
                    }
                    else
                    {
                        await context.RespondAsync<Message<User>>(new MessageResponse<User>()
                        {
                            Action = context.Message.Action,
                            StatusCode = 400
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error : {e.Message}\n\n" + $"{e.StackTrace}");
            }
        }
    }
}
