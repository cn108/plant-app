namespace FinalYearProject;

public partial class DashboardPage : ContentPage
{
    private readonly int currentThreadId;
    private PlantAppDatabase _plantAppDatabase;
	public DashboardPage()
	{
		InitializeComponent();
        _plantAppDatabase = new PlantAppDatabase();
	}

    private async void OnCreatePostClicked(object sender, EventArgs e)
    {
       
            await Navigation.PushAsync(new CreatePostPage(currentThreadId));
    }

    private async void ProfileClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ProfilePage());
    }
}