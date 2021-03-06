﻿using System;
using Helpdesk.Services.Common.Results;
using Helpdesk.Services.Tickets.Results.Enums;

namespace Helpdesk.Services.Tickets.Results
{
    public class ResolveTicketResult : IProcessResult<TicketResolveResult>
    {
        public ResolveTicketResult(TicketResolveResult result)
        {
            Result = result;
        }

        public TicketResolveResult Result { get; }
        public string Message => GetMessage();
        public int TicketId { get; set; }
        public Guid? UserGuid { get; set; }
        public DateTimeOffset? ResolvedOn { get; set; }
        public Guid? ResolvedBy { get; set; }
        public DateTimeOffset? ClosedOn { get; set; }
        public Guid? ClosedBy { get; set; }

        #region Methods

        private string GetMessage() => Result switch
        {
            TicketResolveResult.Resolved => ResultMessages.Resolved,
            TicketResolveResult.TicketNotFound => ResultMessages.TicketNotFound,
            TicketResolveResult.TicketAlreadyResolved => ResultMessages.TicketAlreadyResolved,
            TicketResolveResult.TicketAlreadyClosed => ResultMessages.TicketAlreadyClosed,
            _ => Result.ToString(),
        };

        #endregion Methods
    }
}