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
		private ProjectAgentName _projectAgentName = new ProjectAgentName(Environment.GetEnvironmentVariable("PROJETO_AGENT_NAME"));

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



		[HttpGet("[action]")]
		public IActionResult CriarEntity()
		{
			//Usa as credenciais;

			Google.Cloud.Dialogflow.V2.EntityTypesClient c = EntityTypesClient.Create();

			EntityType entidade = new EntityType();
			entidade.DisplayName = "EntidadeViaAPI";
			entidade.Kind = EntityType.Types.Kind.Map;

			var item = new EntityType.Types.Entity();
			item.Value = "Item 1";
			item.Synonyms.Add("Item 1"); //incluir o valor original como item da entidade.
			item.Synonyms.Add("Sinonimo 1");
			item.Synonyms.Add("Sinonimo 2");

			entidade.Entities.Add(new EntityType.Types.Entity(item));


			var request = new Google.Cloud.Dialogflow.V2.CreateEntityTypeRequest();
			request.EntityType = entidade;
			request.ParentAsProjectAgentName = _projectAgentName;

			c.CreateEntityType(request);

			return Ok(new { msg = "Entidade criada." });
		}


		[HttpGet("[action]")]
		public IActionResult ExcluirEntity(bool apenasItens = false)
		{
			//Usa as credenciais;

			Google.Cloud.Dialogflow.V2.EntityTypesClient c = EntityTypesClient.Create();

			var listE = c.ListEntityTypes(_projectAgentName);

			foreach (var entidade in listE)
			{
				if (entidade.DisplayName == "EntidadeViaAPI")
				{

					if (apenasItens)
					{
						//Apaga os itens
						c.BatchDeleteEntities(entidade.EntityTypeName, entidade.Entities.Select(e => e.Value).ToArray());
					}
					else
					{
						c.DeleteEntityType(entidade.EntityTypeName);
					}

					break;
				}
			}


			return Ok(new { msg = "Entidade excluída." });
		}


		[HttpGet("[action]")]
		public IActionResult AlterarEntity()
		{
			//Usa as credenciais;

			Google.Cloud.Dialogflow.V2.EntityTypesClient c = EntityTypesClient.Create();

			var listE = c.ListEntityTypes(_projectAgentName);


			foreach (var entidade in listE)
			{
				if (entidade.DisplayName == "EntidadeViaAPI")
				{
					var item = entidade.Entities.Where(item => item.Value == "Item 1").FirstOrDefault();

					if (item != null)
					{
						item.Synonyms.Remove("Sinonimo 1");
						item.Synonyms.Add("Sinonimo 3");

						var request = new Google.Cloud.Dialogflow.V2.UpdateEntityTypeRequest();
						request.EntityType = entidade;

						c.UpdateEntityType(request);

						break;
					}
				}
			}


			return Ok(new { msg = "Entidade alterada." });
		}




		[HttpGet("[action]")]
		public IActionResult CriarIntent()
		{
			//Usa as credenciais;

			Google.Cloud.Dialogflow.V2.IntentsClient c = IntentsClient.Create();

			Intent intent = new Intent();
			intent.DisplayName = "Bot.Email";

			var frase1 = new Intent.Types.TrainingPhrase();
			frase1.Parts.Add(new Intent.Types.TrainingPhrase.Types.Part() {
				Text = "Qual seu e-mail?"
			}); 

			var frase2 = new Intent.Types.TrainingPhrase();
			frase2.Parts.Add(new Intent.Types.TrainingPhrase.Types.Part()
			{
				Text = "Seu e-mail é?"
			});

			intent.TrainingPhrases.Add(frase1);
			intent.TrainingPhrases.Add(frase2);


			var resposta = new Intent.Types.Message();
			resposta.Text = new Intent.Types.Message.Types.Text();
			resposta.Text.Text_.Add("bot@unoeste.br");


			intent.Messages.Add(resposta);

			var request = new Google.Cloud.Dialogflow.V2.CreateIntentRequest();
			request.Intent = intent;
			request.ParentAsProjectAgentName = _projectAgentName;
			
			c.CreateIntent(request);
			
			//Não esquecer de trainar o agente: Config > ML Settings


			return Ok(new { msg = "Intent criada." });
		}


	}
}
 