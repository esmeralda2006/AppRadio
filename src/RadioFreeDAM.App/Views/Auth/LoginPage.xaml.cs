using RadioFreeDAM.App.ViewModels.Auth;

namespace RadioFreeDAM.App.Views.Auth;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
