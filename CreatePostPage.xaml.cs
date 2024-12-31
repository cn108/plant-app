using Microsoft.AspNetCore.SignalR.Client;

namespace FinalYearProject;

public partial class CreatePostPage : ContentPage
{
    private readonly HubConnection _hubConnection;
    private int _threadId;

    public CreatePostPage(int threadId)
    {
        InitializeComponent();
        _threadId = threadId;

        // Establish a SignalR connection
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5012/forumHub")
            .Build();

        Task.Run(async () =>
        {
            await _hubConnection.StartAsync();
        });
    }

    private async void OnSubmitPostClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(PostEditor.Text))
        {
            // Send the post to the server along with the thread ID
            //await _hubConnection.InvokeAsync("SendMessage", _threadId, "User", PostEditor.Text);
            await _hubConnection.InvokeAsync("SendMessage", "User", PostEditor.Text, _threadId);

            // Optionally navigate back to the thread or forum page
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "Post content cannot be empty.", "OK");
        }
    }
}