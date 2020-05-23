﻿using System;
using Helpdesk.DomainModels.Tickets.Events;
using Helpdesk.Services.Workflows;
using Helpdesk.Services.Workflows.Enums;

namespace Helpdesk.Services.Tickets.Events.DeleteTicket
{
    public class TicketDeletedWorkflow : TicketDeletedEvent, IWorkflowProcess
    {
        public TicketDeletedWorkflow(int ticketId, Guid userGuid)
            : base(ticketId, userGuid)
        {
        }

        public WorkflowResult Result { get; set; }
        public string Message { get; set; }
    }
}