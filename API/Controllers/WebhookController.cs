using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebhookDF.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
		private static readonly JsonParser _jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

		System.Text.Json.JsonSerializerOptions _jsonSetting = new System.Text.Json.JsonSerializerOptions()
		{
			PropertyNameCaseInsensitive = true
		};

		public WebhookController()
		{
		}


		[HttpGet]
        public IActionResult Get()
        {
			return Ok(new { msg = "deu certo" });
        }


		[HttpGet("[action]")]
		public IActionResult CriarIntent()
		{
			//Usar as credenciais;

			Google.Cloud.Dialogflow.V2.EntityTypesClient c = EntityTypesClient.Create();
		 
			var listE = c.ListEntityTypes(new ProjectAgentName("ep1-jimkbv"));

			EntityType entidade = null;

			foreach (var item in listE)
			{
				if (item.DisplayName == "eteste")
				{
					//c.DeleteEntityType(item.EntityTypeName);
					entidade = item;
					
					c.BatchDeleteEntities(item.EntityTypeName, item.Entities.Select(e => e.Value).ToArray());
				}
			}


			if (entidade == null)
			{
				entidade = new EntityType();

				entidade.DisplayName = "eteste";

				var e = new EntityType.Types.Entity();
				e.Value = "nome do item";
				e.Synonyms.Add("sinonimo11");
				e.Synonyms.Add("sinonimo22");
				entidade.Entities.Add(new EntityType.Types.Entity(e));
				entidade.Kind = EntityType.Types.Kind.Map;


				var request = new Google.Cloud.Dialogflow.V2.CreateEntityTypeRequest();
				//var request = new Google.Cloud.Dialogflow.V2.UpdateEntityTypeRequest();
				request.EntityType = entidade;
				//request.Parent = "ep1-jimkbv";
				request.ParentAsProjectAgentName = new ProjectAgentName("ep1-jimkbv");

				var retorno = c.CreateEntityType(request);
			}
			else {

				var e = new EntityType.Types.Entity();
				e.Value = "nome do item";
				e.Synonyms.Add("sinonimo1");
				entidade.Entities.Add(new EntityType.Types.Entity(e));

				var request = new Google.Cloud.Dialogflow.V2.UpdateEntityTypeRequest();
				request.EntityType = entidade;

				var retorno = c.UpdateEntityType(request);
				
			}



			return Ok(new { msg = "deu certo" });
		}


		private bool Autorizado(IHeaderDictionary httpHeader)
		{

			string basicAuth = httpHeader["Authorization"];

			if (!string.IsNullOrEmpty(basicAuth))
			{
				basicAuth = basicAuth.Replace("Basic ", "");

				byte[] aux = System.Convert.FromBase64String(basicAuth);
				basicAuth = System.Text.Encoding.UTF8.GetString(aux);

				if (basicAuth == "nome:token")
					return true;
			}

			return false;
		}
		
		
		[HttpPost("[action]")]
		public ActionResult GetWebhookResponse([FromBody] System.Text.Json.JsonElement dados)
		{
			if (!Autorizado(Request.Headers))
			{
				return StatusCode(401);
			}

			WebhookRequest request = _jsonParser.Parse<WebhookRequest>(dados.GetRawText());
			WebhookResponse response = new WebhookResponse();


			if (request != null)
			{
				string action = request.QueryResult.Action;
				var parameters = request.QueryResult.Parameters;


				switch (action)
				{

					case "teste.webhooks":
						response.FulfillmentText = "testando o webhooks....";
						break;

					case "teste.webhooks_com_parametros":
						if (parameters != null)
						{
							string cpf = parameters.Fields.ContainsKey("cpf") && parameters.Fields["cpf"].StringValue.Trim() != "" ? parameters.Fields["cpf"].StringValue : "";
							response.FulfillmentText = "testando o webhooks com parâmetros....CPF: " + cpf;
						}
						break;

					case "teste.webhooks-slot-filling":
						if (parameters != null)
						{
							string suco = parameters.Fields.ContainsKey("suco") && parameters.Fields["suco"].StringValue.Trim() != "" ? parameters.Fields["suco"].StringValue : "";

							if (suco != "")
								response.FulfillmentText = "testando o webhooks com slot filling....SUCO: " + suco;
							else response.FulfillmentText = "Qual suco: Laranja, Tangerina...";
						}
						break;

						

					default:
						response.FulfillmentText = "Não compreendi";
						break;
				}



			}

			return Ok(response);

			
		}
	}
}
 