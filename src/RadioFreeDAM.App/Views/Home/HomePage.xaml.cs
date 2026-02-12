using RadioFreeDAM.App.ViewModels.Home;

namespace RadioFreeDAM.App.Views.Home;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.LoadStationsCommand.CanExecute(null))
            _viewModel.LoadStationsCommand.Execute(null);
    }
}
