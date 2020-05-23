﻿using System.Collections.Generic;
using Helpdesk.DomainModels.Common;
using Helpdesk.Services.Common.Results;
using Helpdesk.Services.Tickets.Results.Enums;
using Helpdesk.Services.Workflows;

namespace Helpdesk.Services.Tickets.Results
{
    public class UpdateTicketResult : IProcessResult<TicketUpdateResult>, IValidationResult, IUpdateResult, IWorkflowResult
    {
        public UpdateTicketResult(TicketUpdateResult result)
        {
            Result = result;
        }

        public TicketUpdateResult Result { get; }
        public string Message => GetMessage();
        public int TicketId { get; set; }
        public Dictionary<string, List<string>> ValidationFailures { get; set; }
        public IReadOnlyDictionary<string, ValueChange> PropertyChanges { get; set; }
        public IWorkflowProcess Workflow { get; set; }

        #region Methods

        private string GetMessage() => Result switch
        {
            TicketUpdateResult.Updated => ResultMessages.Updated,
            TicketUpdateResult.ValidationFailure => ResultMessages.ValidationFailure,
            TicketUpdateResult.TicketNotFound => ResultMessages.TicketNotFound,
            _ => Result.ToString(),
        };

        #endregion Methods
    }
}