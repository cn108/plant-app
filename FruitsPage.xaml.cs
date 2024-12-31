using Microsoft.Maui.Controls;
namespace FinalYearProject;

public partial class FruitsPage : ContentPage
{
	public FruitsPage()
	{
		InitializeComponent();
        BindingContext = new FruitsPageViewModel();
    }
}