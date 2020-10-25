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
    public class UserRegisterConsumer : Consumer<User>
    {
        public UserRegisterConsumer(LibraryService libraryService) : base(libraryService)
        {
        }

        public override async Task Consume(ConsumeContext<IMessage<User>> context)
        {
            Debug.WriteLine($"Consumer UserRegister odebral wiadomosc: {context.Message.Action}");

            try
            {
                if (context.SentTime.Value.AddSeconds(30) < DateTime.UtcNow)
                {
                    return;
                }

                if (context.Message.MessageContent.Any() && context.Message.Action.Any())
                {
                    if (context.Message.Action == Variables.ActionRegister)
                    {
                        if (!_libraryService.Register(context.Message.MessageContent.FirstOrDefault()))
                        {
                            await context.RespondAsync<MessageResponse<User>>(new MessageResponse<User>()
                            {
                                Action = context.Message.Action,
                                StatusCode = 400
                            });

                            return;
                        }
                    }

                    else
                    {
                        return;
                    }

                    await context.RespondAsync<MessageResponse<User>>(new MessageResponse<User>()
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
