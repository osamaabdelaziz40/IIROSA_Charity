using MediatR;
using System;

namespace Framework.Core.Data.Events
{
    public class BaseDomainEvent: INotification
    {
        public DateTimeOffset DateOccurred { get; protected set; } = DateTimeOffset.Now;
    }
}
