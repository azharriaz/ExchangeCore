using System;
using ExchangeCore.Application.Common.Interfaces;

namespace ExchangeCore.Infrastructure.Services
{
    public class DateTimeService : IDateTime
    {
        public DateTime Now => DateTime.Now;
    }
}
