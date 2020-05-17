﻿using System.Threading.Tasks;
using Data.Repositories;
using Helpdesk.Domain.Enums;
using Helpdesk.Services.Common;
using Helpdesk.Services.Notifications;
using Helpdesk.Services.Tickets.Events.StartTicket;
using Helpdesk.Services.Tickets.Factories.StartTicket;
using Helpdesk.Services.Tickets.Results;
using Helpdesk.Services.Tickets.Specifications;
using Helpdesk.Services.Workflows;
using Helpdesk.Services.Workflows.Enums;

namespace Helpdesk.Services.Tickets.Commands.StartTicket
{
    public class StartTicketService : IStartTicketService
    {
        private readonly IContextRepository<ITicketContext> _repository;
        private readonly INotificationService _notificationService;
        private readonly IWorkflowService _workflowService;
        private readonly IStartTicketResultFactory _factory;

        public StartTicketService(
            IContextRepository<ITicketContext> repository,
            INotificationService notificationService,
            IWorkflowService workflowService,
            IStartTicketResultFactory factory)
        {
            _repository = repository;
            _notificationService = notificationService;
            _workflowService = workflowService;
            _factory = factory;
        }

        public virtual async Task<StartTicketResult> Start(int ticketId, int userId)
        {
            var ticket = await _repository.SingleAsync(new GetTicketById(ticketId));

            if (ticket == null) return _factory.TicketNotFound(ticketId);

            switch (ticket.GetStatus())
            {
                case TicketStatus.Resolved:
                    return _factory.TicketAlreadyResolved(ticket);

                case TicketStatus.Closed:
                    return _factory.TicketAlreadyClosed(ticket);

                case TicketStatus.InProgress:
                    return _factory.TicketAlreadyStarted(ticket);
            }

            var beforeWorkflow = await _workflowService.Process(new BeforeTicketStartedWorkflow(ticketId, userId));
            if (beforeWorkflow.Result != WorkflowResult.Succeeded) return _factory.WorkflowFailed(ticketId, userId, beforeWorkflow);

            ticket.Start();
            await _repository.SaveAsync();

            var workflow = _workflowService.Process(new TicketStartedWorkflow(ticketId, userId));
            var notification = _notificationService.Queue(new TicketStartedNotification(ticketId, userId));
            await Task.WhenAll(workflow, notification);

            return _factory.Started(ticket);
        }
    }
}