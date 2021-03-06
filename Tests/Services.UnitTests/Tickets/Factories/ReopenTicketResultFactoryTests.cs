﻿using System;
using AutoFixture;
using Helpdesk.Domain.Tickets;
using Helpdesk.Services.Tickets.Factories.ReopenTicket;
using Helpdesk.Services.Tickets.Results;
using Helpdesk.Services.Tickets.Results.Enums;
using NUnit.Framework;

namespace Helpdesk.Services.UnitTests.Tickets.Factories
{
    [TestFixture]
    public class ReopenTicketResultFactoryTests
    {
        private readonly IFixture _fixture = new Fixture();
        private ReopenTicketResultFactory _factory;

        [SetUp]
        public void Setup()
        {
            _factory = new ReopenTicketResultFactory();
        }

        [Test]
        public void Reopened()
        {
            var userGuid = _fixture.Create<Guid>();
            var ticket = new Ticket
            {
                TicketId = _fixture.Create<int>()
            };

            var result = _factory.Reopened(ticket, userGuid);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(TicketReopenResult.Reopened, result.Result, $"Should be {TicketReopenResult.Reopened}.");
                Assert.AreEqual(ResultMessages.Reopened, result.Message, $"Should return the {nameof(ResultMessages.Reopened)} message.");
                Assert.AreEqual(ticket.TicketId, result.TicketId, "Should equal the passed through ticket's TicketId.");
                Assert.AreEqual(userGuid, result.UserGuid, "Should equal the passed through userGuid.");
            });
        }

        [Test]
        public void TicketNotFound()
        {
            var ticketId = _fixture.Create<int>();

            var result = _factory.TicketNotFound(ticketId);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(TicketReopenResult.TicketNotFound, result.Result, $"Should be {TicketReopenResult.TicketNotFound}.");
                Assert.AreEqual(ResultMessages.TicketNotFound, result.Message, $"Should return the {nameof(ResultMessages.TicketNotFound)} message.");
                Assert.AreEqual(ticketId, result.TicketId, "Should equal the passed through ticketId.");
                Assert.IsNull(result.UserGuid, "Should be null.");
            });
        }
    }
}