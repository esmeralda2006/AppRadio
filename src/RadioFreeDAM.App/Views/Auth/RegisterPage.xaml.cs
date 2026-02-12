using RadioFreeDAM.App.ViewModels.Auth;

namespace RadioFreeDAM.App.Views.Auth;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
