using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using CommonData.Messages;
using Library.WebApi.Services;

namespace Library.WebApi.Consumer
{
    public abstract class Consumer<TMessageType> : IConsumer<IMessage<TMessageType>>
        where TMessageType : class
    {
        protected readonly LibraryService _libraryService;

        public Consumer(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        public abstract Task Consume(ConsumeContext<IMessage<TMessageType>> context);
    }
}
