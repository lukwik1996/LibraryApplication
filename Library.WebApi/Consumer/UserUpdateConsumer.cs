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
    public class UserUpdateConsumer : Consumer<UserUpdate>
    {
        public UserUpdateConsumer(LibraryService libraryService) : base(libraryService)
        {
        }

        public override async Task Consume(ConsumeContext<IMessage<UserUpdate>> context)
        {
            Debug.WriteLine($"Consumer UserUpdate odebral wiadomosc: {context.Message.Action}");

            try
            {
                if (context.SentTime.Value.AddSeconds(30) < DateTime.UtcNow)
                {
                    return;
                }

                if (context.Message.MessageContent.Any() && context.Message.Action.Any())
                {
                    var success = false;

                    if (context.Message.Action == Variables.ActionUpdateEmail)
                    {
                        if (_libraryService.UpdateEmail(context.Message.UserId,
                            context.Message.MessageContent.FirstOrDefault()))
                        {
                            success = true;
                        }
                    }

                    else if (context.Message.Action == Variables.ActionUpdatePassword)
                    {
                        if (_libraryService.UpdatePassword(context.Message.UserId,
                            context.Message.MessageContent.FirstOrDefault()))
                        {
                            success = true;
                        }
                    }

                    else
                    {
                        return;
                    }

                    if (success)
                    {
                        await context.RespondAsync<MessageResponse<UserUpdate>>(new MessageResponse<UserUpdate>()
                        {
                            Action = context.Message.Action,
                            StatusCode = 200
                        });
                    }
                    else
                    {
                        await context.RespondAsync<MessageResponse<UserUpdate>>(new MessageResponse<UserUpdate>()
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
