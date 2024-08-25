using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatBotController : ControllerBase
{
    private readonly ILogger<ChatBotController> logger;
    private readonly IConfiguration configuration;
    private readonly HttpClient httpClient;
    private readonly IWebHostEnvironment webHostEnvironment;

    public ChatBotController(ILogger<ChatBotController> logger, IConfiguration configuration, HttpClient httpClient, IWebHostEnvironment webHostEnvironment)
    {
        this.logger = logger;
        this.configuration = configuration;
        this.httpClient = httpClient;
        this.webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    [Route("test")]
    public IActionResult Test()
    {
        return Ok("api is working!");
    }

    [HttpGet]
    [Route("chat-bot-response")]
    public async Task<IActionResult> GetChatBotResponse([FromQuery] string input)
    {
        var apiUrl = "https://api-inference.huggingface.co/models/microsoft/Phi-3-mini-4k-instruct";


        var apiKey = webHostEnvironment.IsDevelopment() ? configuration["Keys:HuggingFaceAPIKey"] : Environment.GetEnvironmentVariable("HuggingFaceAPIKey");

        // Set the Authorization header with your API key
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        // Create the JSON payload
        var requestContent = new JObject
        {
            ["inputs"] = input,
            ["parameters"] = new JObject
            {
                ["max_new_tokens"] = 1024
            },
            ["task"] = "text-generation"
        };

        JArray result;

        try
        {
            // Send the POST request
            var response = await httpClient.PostAsync(apiUrl, new StringContent(requestContent.ToString(), Encoding.UTF8, "application/json"));
            var responseString = await response.Content.ReadAsStringAsync();
            result = JArray.Parse(responseString);
        }
        catch (System.Exception ex)
        {
        return BadRequest(ex.Message);
        }

        // the chatbot tends to repeat what you asked so we are removing that
        var output = result.First?["generated_text"]?.ToString().Replace(input, "");

        // Return the generated text
        return Ok(output);
    }
}

