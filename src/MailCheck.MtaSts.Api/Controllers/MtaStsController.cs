using System;
using System.Threading.Tasks;
using MailCheck.Common.Api.Authorisation.Filter;
using MailCheck.Common.Api.Authorisation.Service.Domain;
using MailCheck.Common.Api.Domain;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.MtaSts.Api.Config;
using MailCheck.MtaSts.Api.Dao;
using MailCheck.MtaSts.Api.Domain;
using MailCheck.MtaSts.Api.Service;
using MailCheck.MtaSts.Contracts.Scheduler;
using Microsoft.AspNetCore.Mvc;

namespace MailCheck.MtaSts.Api.Controllers
{
    [Route("/api/mtasts")]
    public class MtaStsController : Controller
    {
        private readonly IMtaStsApiDao _dao;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IMtaStsApiConfig _config;
        private readonly IMtaStsService _mtaStsService;

        public MtaStsController(IMtaStsApiDao dao,
            IMessagePublisher messagePublisher,
            IMtaStsApiConfig config,
            IMtaStsService mtaStsService)
        {
            _dao = dao;
            _messagePublisher = messagePublisher;
            _config = config;
            _mtaStsService = mtaStsService;
        }

        [HttpGet("{domain}/recheck")]
        [MailCheckAuthoriseResource(Operation.Update, ResourceType.MtaSts, "domain")]
        public async Task<IActionResult> RecheckMtaSts(MtaStsDomainRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse(ModelState.Values));
            }

            await _messagePublisher.Publish(new MtaStsRecordExpired(request.Domain), _config.SnsTopicArn);

            return new OkObjectResult("{}");
        }

        [HttpGet("{domain}")]
        [MailCheckAuthoriseRole(Role.Standard)]
        public async Task<IActionResult> GetMtaSts(MtaStsDomainRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse(ModelState.Values));
            }

            MtaStsInfoResponse response = await _mtaStsService.GetMtaStsForDomain(request.Domain);

            if (response == null)
            {
                return new NotFoundObjectResult(new ErrorResponse($"No MTA-STS found for {request.Domain}",
                    ErrorStatus.Information));
            }

            return new ObjectResult(response);
        }

        [HttpPost]
        [Route("domains")]
        [MailCheckAuthoriseRole(Role.Standard)]
        public async Task<IActionResult> GetMtaStsStates([FromBody] MtaStsInfoListRequest request)
        {
            return new ObjectResult(await _dao.GetMtaStsForDomains(request.Domains));
        }

        [HttpGet]
        [Route("history/{domain}")]
        [MailCheckAuthoriseResource(Operation.Read, ResourceType.MtaStsHistory, "domain")]
        public Task<IActionResult> GetMtaStsHistory(MtaStsDomainRequest request)
        {
            throw new NotImplementedException("MTA-STS History has not been implemented yet.");

            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(new ErrorResponse(ModelState.Values));
            //}

            //string history = await _dao.GetMtaStsHistory(request.Domain);

            //if (history == null)
            //{
            //    return new NotFoundObjectResult(new ErrorResponse($"No MTA-STS History found for {request.Domain}",
            //        ErrorStatus.Information));
            //}

            //return Content(history, "application/json");
        }
    }
}