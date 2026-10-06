using Microsoft.Extensions.AI;

public class AiChatService
{
    private readonly IChatClient _chatClient;

    private readonly List<ChatMessage> _chatHistory =
    [
        new ChatMessage(
            ChatRole.System,
            """
            You are a senior .NET technical assistant.

            Explain concepts clearly.
            Prefer practical C# examples.
            Keep answers concise unless the user asks for detail.
            """
        )
    ];

    public AiChatService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task AskAsync(string question)
    {
        _chatHistory.Add(
            new ChatMessage(ChatRole.User, question)
        );

        Console.Write("AI: ");

        var assistantResponse = new List<string>();

        await foreach (
            var update in _chatClient.GetStreamingResponseAsync(_chatHistory))
        {
            Console.Write(update.Text);

            assistantResponse.Add(update.Text);
        }

        Console.WriteLine();

        string fullResponse = string.Concat(assistantResponse);

        _chatHistory.Add(
            new ChatMessage(
                ChatRole.Assistant,
                fullResponse
            )
        );
    }
}