using Microsoft.AspNetCore.SignalR.Client;

namespace FinalYearProject;

public partial class ChatPage : ContentPage
{
    private readonly HubConnection _connection;

    public ChatPage()
    {
        InitializeComponent();

        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5012/chatHub")
            .Build();

        _connection.On<string>("MessageReceived", (message) =>
        {
            Dispatcher.Dispatch(() =>
            {
                chatMessages.Text += $"{Environment.NewLine}{message}";
            });
        });

        Task.Run(async () =>
        {
            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting connection: {ex.Message}");
            }
        });
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(MessageEntry.Text))
        {
            await _connection.InvokeAsync("SendMessage", MessageEntry.Text);
            MessageEntry.Text = string.Empty; // Clear the entry after sending
        }
    }
}
