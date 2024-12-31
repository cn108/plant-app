using Microsoft.Maui.Controls;
using System.Collections.Generic;

namespace FinalYearProject
{
    public partial class MessagesPage : ContentPage
    {
        private readonly PlantAppDatabase _plantAppDatabase;

        public MessagesPage()
        {
            InitializeComponent();
            _plantAppDatabase = new PlantAppDatabase();
            LoadMessages();
        }

        private void LoadMessages()
        {
            List<ForumMessage> messages = _plantAppDatabase.GetAllMessages();
            MessagesListView.ItemsSource = messages;
        }
    }
}
