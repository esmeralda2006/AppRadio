using RadioFreeDAM.App.ViewModels.Favorites;

namespace RadioFreeDAM.App.Views.Favorites;

public partial class FavoritesPage : ContentPage
{
    private readonly FavoritesViewModel _viewModel;

    public FavoritesPage(FavoritesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.LoadFavoritesCommand.CanExecute(null))
            await _viewModel.LoadFavoritesCommand.ExecuteAsync(null);
    }
}
