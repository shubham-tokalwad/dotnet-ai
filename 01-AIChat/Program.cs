using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
string model = config["ModelName"];
string key = config["OpenAIKey"];

// Create the IChatClient
IChatClient chatClient =
    new OpenAIClient(key).GetChatClient(model).AsIChatClient();

builder.Services.AddSingleton(chatClient);
builder.Services.AddTransient<AiChatService>();

using IHost host = builder.Build();

var aiService = host.Services.GetRequiredService<AiChatService>();

// Start the conversation with context for the AI model
List<ChatMessage> chatHistory =
    [
        new ChatMessage(ChatRole.System, """
            You are a friendly hiking enthusiast who helps people discover fun hikes in their area.
            You introduce yourself when first saying hello.
            When helping people out, you always ask them for this information
            to inform the hiking recommendation you provide:

            1. The location where they would like to hike
            2. What hiking intensity they are looking for

            You will then provide three suggestions for nearby hikes that vary in length
            after you get that information. You will also share an interesting fact about
            the local nature on the hikes when making a recommendation. At the end of your
            response, ask if there is anything else you can help with.
        """)
    ];

// Loop to get user input and stream AI response
while (true)
{
    // Get user prompt and add to chat history
    Console.WriteLine("Your prompt:");
    string? userPrompt = Console.ReadLine();
    chatHistory.Add(new ChatMessage(ChatRole.User, userPrompt));

    // Stream the AI response and add to chat history
    Console.WriteLine("AI Response:");
    string response = "";
    await foreach (ChatResponseUpdate item in
        chatClient.GetStreamingResponseAsync(chatHistory))
    {
        Console.Write(item.Text);
        response += item.Text;
    }
    chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));
    Console.WriteLine();
}

while (true)
{
    Console.Write("You: ");

    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        continue;

    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    string response = await aiService.AskAsync(input);

    Console.WriteLine($"AI: {response}");
}

while (true)
{
    Console.Write("You: ");

    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        continue;

    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    await aiService.AskAsync(input);
}