﻿using System;
using System.Linq.Expressions;
using Data.Specifications;
using Helpdesk.Domain.Entities;

namespace Helpdesk.Services.Tickets.Specifications
{
    public class GetApprovedTickets : LinqSpecification<Ticket>
    {
        public override Expression<Func<Ticket, bool>> AsExpression()
        {
            return ticket => !ticket.ResolvedOn.HasValue
                          && !ticket.ClosedOn.HasValue
                          && ticket.ApprovedOn.HasValue;
        }
    }
}