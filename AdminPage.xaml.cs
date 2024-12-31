using System.Collections.ObjectModel;

namespace FinalYearProject;

public partial class AdminPage : ContentPage
{
    private readonly PlantAppDatabase _plantAppDatabase;
    private ObservableCollection<DBUsers> _users;

    public AdminPage()
	{
		InitializeComponent();
        _plantAppDatabase = new PlantAppDatabase();
        LoadUsers();
    }
    private void LoadUsers()
    {
        // Load all users from the database
        var users = _plantAppDatabase.GetUser(); // Assumes you have an instance of your database class
        UserCollectionView.ItemsSource = users;
    }

    private async void OnDeleteUserClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var userToDelete = button.BindingContext as DBUsers;

        bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete {userToDelete.Username}?", "Yes", "No");
        if (confirm)
        {
            _plantAppDatabase.DeleteUser(userToDelete.UserId);
            await DisplayAlert("Deleted", $"{userToDelete.Username} has been deleted.", "OK");
            LoadUsers();  // Refresh the user list
        }

    }

    private async void OnUserSelected(object sender, SelectionChangedEventArgs e)
    {
        var selectedUser = e.CurrentSelection.FirstOrDefault() as DBUsers;
        if (selectedUser != null)
        {
            await Navigation.PushAsync(new UserDetailsPage(selectedUser));
        }
    }
}