namespace RadioFreeDAM.App.Helpers;

public static class ServiceHelper
{
    public static T GetService<T>() => Current.GetService<T>();

    public static IServiceProvider Current => 
#if WINDOWS10_0_19041_0_OR_GREATER
        MauiWinUIApplication.Current.Services;
#elif ANDROID
        MauiApplication.Current.Services;
#else
        null;
#endif
}
