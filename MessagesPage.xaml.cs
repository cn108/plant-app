namespace FinalYearProject
{
    public partial class MessagesPage : ContentPage
    {
        private readonly PlantAppDatabase _plantAppDatabase = new();

        public MessagesPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadAsync();
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await LoadAsync();
            Refresher.IsRefreshing = false;
        }

        private async Task LoadAsync()
        {
            var online = await ApiClient.GetPostsAsync();
            if (online.Ok && online.Value is not null)
            {
                PostsView.ItemsSource = online.Value;
                SourceLabel.Text = "Shared with the whole community. Pull down to refresh.";
                return;
            }

            // Offline: show what was posted on this device.
            PostsView.ItemsSource = _plantAppDatabase.GetAllMessages()
                .Select(m => new ApiPost(m.MessageId, m.UserName ?? "Unknown", m.Category ?? "General", m.MessageText ?? string.Empty, m.CreatedAt, []))
                .ToList();
            SourceLabel.Text = "Offline: showing posts saved on this device.";
        }

        private async void OnReplyClicked(object? sender, EventArgs e)
        {
            if (sender is not Button { BindingContext: ApiPost post }) return;
            if (!ApiClient.IsSignedIn)
            {
                await DisplayAlert("Offline", "Connect to the server and sign in to reply.", "OK");
                return;
            }

            var text = await DisplayPromptAsync("Reply", $"Reply to {post.Username}", "Send", "Cancel", maxLength: 2000);
            if (string.IsNullOrWhiteSpace(text)) return;

            var result = await ApiClient.ReplyAsync(post.Id, text);
            if (!result.Ok) await DisplayAlert("Could not reply", result.Error, "OK");
            else await LoadAsync();
        }

        private async void OnReportClicked(object? sender, EventArgs e)
        {
            if (sender is not Button { BindingContext: ApiPost post }) return;
            if (!ApiClient.IsSignedIn)
            {
                await DisplayAlert("Offline", "Connect to the server and sign in to report posts.", "OK");
                return;
            }

            var reason = await DisplayActionSheet("Report this post as", "Cancel", null, "Spam", "Abusive", "Misleading advice");
            if (string.IsNullOrEmpty(reason) || reason == "Cancel") return;

            var result = await ApiClient.ReportAsync(post.Id, reason);
            await DisplayAlert(result.Ok ? "Thanks" : "Could not report",
                result.Ok ? "A moderator will review this post." : result.Error, "OK");
        }
    }
}
