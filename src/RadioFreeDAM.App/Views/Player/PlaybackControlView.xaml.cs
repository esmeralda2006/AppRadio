using RadioFreeDAM.App.Services.Player;
using RadioFreeDAM.App.Helpers;

namespace RadioFreeDAM.App.Views.Player;

public partial class PlaybackControlView : ContentView
{
	public PlaybackControlView()
	{
		InitializeComponent();
        BindingContext = ServiceHelper.GetService<RadioPlayerService>();
	}
}
