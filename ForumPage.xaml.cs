using Microsoft.Maui.Controls;
using System;

namespace FinalYearProject
{
    public partial class ForumPage : ContentPage
    {
        private readonly PlantAppDatabase _plantAppDatabase;

        public ForumPage()
        {
            InitializeComponent();
            _plantAppDatabase = new PlantAppDatabase();
        }

        private void OnSendMessageClicked(object sender, EventArgs e)
        {
            try
            {
                var category = CategoryPicker.SelectedItem?.ToString();
                var message = MessageEntry.Text;
                var userId = App.CurrentUserId;
                var userName = App.CurrentUserName;

                if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(message))
                {
                    StatusLabel.Text = "Please select a category and type a message.";
                    return;
                }

                var forumMessage = new ForumMessage
                {
                    UserId = userId,
                    UserName = userName,
                    Category = category,
                    MessageText = message,
                    CreatedAt = DateTime.Now
                };

                _plantAppDatabase.AddForumMessage(forumMessage);

                StatusLabel.TextColor = Colors.Green;
                StatusLabel.Text = "Message sent!";
                MessageEntry.Text = string.Empty;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                StatusLabel.TextColor = Colors.Red;
                StatusLabel.Text = $"Error: {ex.Message}";
            }
        }

        private async void OnViewMessagesClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MessagesPage());
        }

    }
}
