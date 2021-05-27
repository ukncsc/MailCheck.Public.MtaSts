using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Api.Authorisation.Filter;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.MtaSts.Api.Config;
using MailCheck.MtaSts.Api.Controllers;
using MailCheck.MtaSts.Api.Dao;
using MailCheck.MtaSts.Api.Domain;
using MailCheck.MtaSts.Api.Service;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MailCheck.MtaSts.Contracts.Entity;
using NUnit.Framework;

namespace MailCheck.MtaSts.Api.Test.Controllers
{
    [TestFixture]
    public class MtaStsControllerTests
    {
        private MtaStsController _sut;
        private IMtaStsService _mtaStsService;

        [SetUp]
        public void SetUp()
        {
            _mtaStsService = A.Fake<IMtaStsService>();
            _sut = new MtaStsController(A.Fake<IMtaStsApiDao>(), A.Fake<IMessagePublisher>(),
                A.Fake<IMtaStsApiConfig>(), _mtaStsService);
        }

        [Test]
        public async Task ItShouldReturnNotFoundWhenThereIsNoMtaStsState()
        {
            A.CallTo(() => _mtaStsService.GetMtaStsForDomain(A<string>._))
                .Returns(Task.FromResult<MtaStsInfoResponse>(null));

            IActionResult response = await _sut.GetMtaSts(new MtaStsDomainRequest {Domain = "ncsc.gov.uk"});

            Assert.That(response, Is.TypeOf(typeof(NotFoundObjectResult)));
        }

        [Test]
        public async Task ItShouldReturnTheFirstResultWhenTheMtaStsStateExists()
        {
            MtaStsInfoResponse state = new MtaStsInfoResponse("ncsc.gov.uk", MtaStsState.Created);

            A.CallTo(() => _mtaStsService.GetMtaStsForDomain(A<string>._))
                .Returns(Task.FromResult(state));

            ObjectResult response =
                (ObjectResult) await _sut.GetMtaSts(new MtaStsDomainRequest {Domain = "ncsc.gov.uk"});

            Assert.AreSame(response.Value, state);
        }

        [Test]
        public void AllEndpointsHaveAuthorisation()
        {
            IEnumerable<MethodInfo> controllerMethods =
                _sut.GetType().GetMethods().Where(x => x.DeclaringType == typeof(MtaStsController));

            foreach (MethodInfo methodInfo in controllerMethods)
            {
                Assert.That(methodInfo.CustomAttributes.Any(x =>
                    x.AttributeType == typeof(MailCheckAuthoriseResourceAttribute) ||
                    x.AttributeType == typeof(MailCheckAuthoriseRoleAttribute)));
            }
        }
    }
}