﻿using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Data.Repositories;
using Data.Specifications;
using Helpdesk.Domain.Tickets;
using Helpdesk.Domain.Tickets.Events;
using Helpdesk.DomainModels.TicketLinks;
using Helpdesk.Services.Common;
using Helpdesk.Services.Common.Contexts;
using Helpdesk.Services.TicketLinks.Commands.UnlinkTickets;
using Helpdesk.Services.TicketLinks.Factories.UnlinkTickets;
using Moq;
using NUnit.Framework;

namespace Helpdesk.Services.UnitTests.TicketLinks.Commands
{
    [TestFixture]
    public class UnlinkTicketServiceTests
    {
        private readonly IFixture _fixture = new Fixture();

        [Test]
        public void UnUnlink_WhenLinkTicketIsSelfUnUnlink_ThrowsArgumentException()
        {
            var ticketId = _fixture.Create<int>();
            var unlinkTicket = new UnlinkTicket
            {
                FromTicketId = ticketId,
                ToTicketId = ticketId
            };

            var service = CreateService();

            Assert.ThrowsAsync<ArgumentException>(() => service.Unlink(unlinkTicket), "Should throw an ArgumentException when UnlinkTicket is self unlink.");
        }

        [Test]
        public async Task Unlink_VerifyThatSingleAsyncForAnOrSpecOfGetLinkedTicketsByIdIsCalled()
        {
            var unlinkTicket = _fixture.Create<UnlinkTicket>();
            var mockContext = new Mock<IEntityRepository<ITicketContext>>();

            var service = CreateService(mockContext: mockContext);

            await service.Unlink(unlinkTicket);

            mockContext.Verify(v => v.SingleAsync(It.IsAny<LinqSpecification<TicketLink>>()), Times.Once, "Should call the context's SingleAsync method exactly once for LinqSpecification of TicketLink.");
        }

        [Test]
        public async Task Unlink_WhenTicketLinkRecordIsNull_VerifyFactoryTicketsNotLinkedIsReturned()
        {
            var unlinkTicket = _fixture.Create<UnlinkTicket>();
            var mockContext = new Mock<IEntityRepository<ITicketContext>>();
            var mockFactory = new Mock<IUnlinkTicketsResultFactory>();

            mockContext.Setup(s => s.SingleAsync(It.IsAny<LinqSpecification<TicketLink>>())).ReturnsAsync((TicketLink)null);

            var service = CreateService(
                mockContext: mockContext,
                mockFactory: mockFactory);

            await service.Unlink(unlinkTicket);

            mockFactory.Verify(v => v.TicketsNotLinked(unlinkTicket.FromTicketId, unlinkTicket.ToTicketId), Times.Once, "Should return the factory's TicketsNotLinked method.");
        }

        [Test]
        public async Task Unlink_WhenTicketLinkExists_VerifyRemoveIsCalled()
        {
            var unlinkTicket = _fixture.Create<UnlinkTicket>();
            var ticketLink = new TicketLink();
            var mockContext = new Mock<IEntityRepository<ITicketContext>>();

            mockContext.Setup(s => s.SingleAsync(It.IsAny<LinqSpecification<TicketLink>>())).ReturnsAsync(ticketLink);

            var service = CreateService(
                mockContext: mockContext);

            await service.Unlink(unlinkTicket);

            mockContext.Verify(v => v.Remove(ticketLink), Times.Once, "Should call the context's Remove method for the ticket link.");
        }

        [Test]
        public async Task Unlink_VerifySaveAsyncIsCalled()
        {
            var unlinkTicket = _fixture.Create<UnlinkTicket>();
            var ticketLink = new TicketLink();
            var mockContext = new Mock<IEntityRepository<ITicketContext>>();

            mockContext.Setup(s => s.SingleAsync(It.IsAny<LinqSpecification<TicketLink>>())).ReturnsAsync(ticketLink);

            var service = CreateService(
                mockContext: mockContext);

            await service.Unlink(unlinkTicket);

            mockContext.Verify(v => v.SaveAsync(), Times.Once, "Should call the context's SaveAsync.");
        }

        [Test]
        public async Task Unlink_WhenTicketsAreUninked_VerifyEventIsRaised()
        {
            var unlinkTicket = _fixture.Create<UnlinkTicket>();
            var ticketLink = new TicketLink();
            var mockContext = new Mock<IEntityRepository<ITicketContext>>();
            var mockEventService = new Mock<IEventService>();

            mockContext.Setup(s => s.SingleAsync(It.IsAny<LinqSpecification<TicketLink>>())).ReturnsAsync(ticketLink);

            var service = CreateService(
                mockContext: mockContext,
                mockEventService: mockEventService);

            await service.Unlink(unlinkTicket);

            mockEventService.Verify(v => v.Publish(It.IsAny<TicketsUnlinkedEvent>()), Times.Once, "Should publish a TicketsUnlinkedEvent.");
        }

        [Test]
        public async Task Unlink_WhenTicketLinkIsRemoved_VerifyFactoryUnlinkedIsReturned()
        {
            var unlinkTicket = _fixture.Create<UnlinkTicket>();
            var ticketLink = new TicketLink();
            var mockContext = new Mock<IEntityRepository<ITicketContext>>();
            var mockFactory = new Mock<IUnlinkTicketsResultFactory>();

            mockContext.Setup(s => s.SingleAsync(It.IsAny<LinqSpecification<TicketLink>>())).ReturnsAsync(ticketLink);

            var service = CreateService(
                mockContext: mockContext,
                mockFactory: mockFactory);

            var result = await service.Unlink(unlinkTicket);

            mockFactory.Verify(v => v.Unlinked(unlinkTicket.FromTicketId, unlinkTicket.ToTicketId), Times.Once, "Should call the factory's Unlinked method.");
        }

        private UnlinkTicketService CreateService(
            IMock<IEntityRepository<ITicketContext>> mockContext = null,
            IMock<IUnlinkTicketsResultFactory> mockFactory = null,
            IMock<IEventService> mockEventService = null)
        {
            mockContext ??= new Mock<IEntityRepository<ITicketContext>>();
            mockFactory ??= new Mock<IUnlinkTicketsResultFactory>();
            mockEventService ??= new Mock<IEventService>();

            return new UnlinkTicketService(
                mockContext.Object,
                mockFactory.Object,
                mockEventService.Object);
        }
    }
}