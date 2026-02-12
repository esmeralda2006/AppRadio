using RadioFreeDAM.App.ViewModels.Player;

namespace RadioFreeDAM.App.Views.Player;

public partial class PlayerPage : ContentPage
{
    private readonly PlayerViewModel _vm;

    public PlayerPage(PlayerViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }
}
