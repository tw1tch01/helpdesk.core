﻿using System;
using Helpdesk.Domain.Common;

namespace Helpdesk.Services.UnitTests.Common.Specifications
{
    public class TestEntity : ICreatedAudit, IModifiedAudit
    {
        public int CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string CreatedProcess { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
        public string ModifiedProcess { get; set; }
    }
}