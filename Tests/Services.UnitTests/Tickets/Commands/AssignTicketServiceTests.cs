﻿using System;
using System.Threading.Tasks;
using AutoFixture;
using Data.Repositories;
using Helpdesk.Domain.Tickets;
using Helpdesk.Domain.Tickets.Enums;
using Helpdesk.Domain.Tickets.Events;
using Helpdesk.Services.Common;
using Helpdesk.Services.Common.Contexts;
using Helpdesk.Services.Tickets.Commands.AssignTicket;
using Helpdesk.Services.Tickets.Factories.AssignTicket;
using Helpdesk.Services.Tickets.Specifications;
using Moq;
using NUnit.Framework;

namespace Helpdesk.Services.UnitTests.Tickets.Commands
{
    [TestFixture]
    public class AssignTicketServiceTests
    {
        private readonly IFixture _fixture = new Fixture();

        [Test]
        public async Task AssignUser_VerifyThatSingleAsyncForGetTicketByIdIsCalled()
        {
            var ticketId = _fixture.Create<int>();
            var mockContext = new Mock<IEntityRepository<ITicketContext>>();

            var service = CreateService(mockContext: mockContext);

            await service.AssignUser(ticketId, It.IsAny<Guid>());

            mockContext.Verify(v => v.SingleAsync(It.Is<GetTicketById>(t => t.TicketId == ticketId)), Times.Once, "Should call the context's SingleAsync method exactly once for GetTicketById.");
        }

        [Test]
        public async Task AssignUser_WhenTicketRecordIsNull_VerifyFactoryTicketNotFoundIsReturned()
        {
            var ticketId = _fixture.Create<int>();
            var mockContext = new Mock<IEntityRepository<ITicketContext>>();
            var mockFactory = new Mock<IAssignTicketResultFactory>();

            mockContext.Setup(s => s.SingleAsync(It.IsAny<GetTicketById>())).ReturnsAsync((Ticket)null);

            var service = CreateService(
                mockContext: mockContext,
                mockFactory: mockFactory);

            await service.AssignUser(ticketId, It.IsAny<Guid>());

            mockFactory.Verify(v => v.TicketNotFound(ticketId), Times.Once, "Should return the factory's TicketNotFound method.");
        }

        [Test]
        public async Task AssignUser_WhenTicketIsResolved_VerifyFactoryTicketAlreadyResolvedIsReturned()
        {
            var mockTicket = new Mock<Ticket>();
            var mockContext = new Mock<IEntityRepository<ITicketContext>>();
            var mockFactory = new Mock<IAssignTicketResultFactory>();

            mockContext.Setup(s => s.SingleAsync(It.IsAny<GetTicketById>())).ReturnsAsync(mockTicket.Object);
            mockTicket.Setup(s => s.GetStatus()).Returns(TicketStatus.Resolved);

            var service = CreateService(
                mockContext: mockContext,
                mockFactory: mockFactory);

            await service.AssignUser(It.IsAny<int>(), It.IsAny<Guid>());

            mockFactory.Verify(v => v.TicketAlreadyResolved(mockTicket.Object), Times.Once, "Should return the factory's TicketAlreadyResolved method.");
        }

        [Test]
        public async Task AssignUser_WhenTicketIsAssigned_VerifyFactoryTicketAlreadyAssignedIsReturned()
        {
            var mockTicket = new Mock<Ticket>();
            var mockContext = new Mock<IEntityRepository<ITicketContext>>();
            var mockFactory = new Mock<IAssignTicketResultFactory>();

            mockContext.Setup(s => s.SingleAsync(It.IsAny<GetTicketById>())).ReturnsAsync(mockTicket.Object);
            mockTicket.Setup(s => s.GetStatus()).Returns(TicketStatus.Closed);

            var service = CreateService(
                mockContext: mockContext,
                mockFactory: mockFactory);

            await service.AssignUser(It.IsAny<int>(), It.IsAny<Guid>());

            mockFactory.Verify(v => v.TicketAlreadyClosed(mockTicket.Object), Times.Once, "Should return the factory's TicketAlreadyAssigned method.");
        }

        [Test]
        public async Task AssignUser_WhenTicketCanBeAssigned_VerifyTicketAssignMethodIsCalled()
        {
            var userGuid = _fixture.Create<Guid>();
            var mockTicket = new Mock<Ticket>();
            var mockContext = new Mock<IEntityRepository<ITicketContext>>();

            mockContext.Setup(s => s.SingleAsync(It.IsAny<GetTicketById>())).ReturnsAsync(mockTicket.Object);

            var service = CreateService(
                mockContext: mockContext);

            await service.AssignUser(It.IsAny<int>(), userGuid);

            mockTicket.Verify(v => v.AssignUser(userGuid), Times.Once, "Should call the ticket's Assign method.");
        }

        [Test]
        public async Task AssignUser_WhenTicketIsAssigned_VerifySaveAsyncIsCalled()
        {
            var mockContext = new Mock<IEntityRepository<ITicketContext>>();

            mockContext.Setup(s => s.SingleAsync(It.IsAny<GetTicketById>())).ReturnsAsync(new Ticket());

            var service = CreateService(
                mockContext: mockContext);

            await service.AssignUser(It.IsAny<int>(), It.IsAny<Guid>());

            mockContext.Verify(v => v.SaveAsync(), Times.Once, "Should call the context's SaveAsync.");
        }

        [Test]
        public async Task AssignUser_WhenTicketIsAssigned_VerifyEventIsRaised()
        {
            var mockContext = new Mock<IEntityRepository<ITicketContext>>();
            var mockEventService = new Mock<IEventService>();

            mockContext.Setup(s => s.SingleAsync(It.IsAny<GetTicketById>())).ReturnsAsync(new Ticket());

            var service = CreateService(
                mockContext: mockContext,
                mockEventService: mockEventService);

            await service.AssignUser(It.IsAny<int>(), It.IsAny<Guid>());

            mockEventService.Verify(v => v.Publish(It.IsAny<TicketAssignedEvent>()), Times.Once, "Should publish a TicketAssignedEvent.");
        }

        [Test]
        public async Task AssignUser_WhenTicketIsAssigned_VerifyFactoryAssignedIsReturned()
        {
            var mockContext = new Mock<IEntityRepository<ITicketContext>>();
            var mockFactory = new Mock<IAssignTicketResultFactory>();

            var ticket = new Ticket();
            mockContext.Setup(s => s.SingleAsync(It.IsAny<GetTicketById>())).ReturnsAsync(ticket);

            var service = CreateService(
                mockContext: mockContext,
                mockFactory: mockFactory);

            await service.AssignUser(It.IsAny<int>(), It.IsAny<Guid>());

            mockFactory.Verify(v => v.Assigned(ticket), Times.Once, "Should return the factory's Assigned method.");
        }

        private AssignTicketService CreateService(
            IMock<IEntityRepository<ITicketContext>> mockContext = null,
            IMock<IAssignTicketResultFactory> mockFactory = null,
            IMock<IEventService> mockEventService = null)
        {
            mockContext ??= new Mock<IEntityRepository<ITicketContext>>();
            mockFactory ??= new Mock<IAssignTicketResultFactory>();
            mockEventService ??= new Mock<IEventService>();

            return new AssignTicketService(
                mockContext.Object,
                mockFactory.Object,
                mockEventService.Object);
        }
    }
}