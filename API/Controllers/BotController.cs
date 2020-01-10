using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Dialogflow.V2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebhookDF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotController : ControllerBase
    {

		private ProjectAgentName _projectAgentName = new ProjectAgentName(Environment.GetEnvironmentVariable("PROJETO_AGENT_NAME"));

		[HttpGet("[action]")]
		public ActionResult DetectIntentFromTexts(string q, string sessionId)
		{
			try
			{
				var client = SessionsClient.Create();

				DetectIntentRequest request = new DetectIntentRequest();
				request.SessionAsSessionName = new SessionName(_projectAgentName.ProjectId, sessionId);
				request.QueryInput = new QueryInput
				{
					Text = new TextInput()
					{
						Text = q,
						LanguageCode = "pt-br"
					}
				};

				DetectIntentResponse response = client.DetectIntent(request);

				var queryResult = response.QueryResult;

				return Ok(queryResult);

			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message);

			}
		}


		[HttpGet("[action]")]
		public ActionResult GetEvent(string eventName, string sessionId)
		{

			try
			{
				var client = SessionsClient.Create();

				DetectIntentRequest request = new DetectIntentRequest();
				request.SessionAsSessionName = new SessionName(_projectAgentName.ProjectId, sessionId);
				request.QueryInput = new QueryInput
				{
					Event = new EventInput
					{
						Name = eventName,
						LanguageCode = "pt-br"
					}
				};

				request.QueryParams = new QueryParameters();

				DetectIntentResponse response = client.DetectIntent(request);

				var queryResult = response.QueryResult;

				return Ok(queryResult);
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message);
			}

		}

	}
}