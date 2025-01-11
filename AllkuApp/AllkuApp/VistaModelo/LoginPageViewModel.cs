using System;
using Xamarin.Essentials;
using System.Windows.Input;
using Xamarin.Forms;

public class LoginPageViewModel
{
    public ICommand OpenWhatsAppCommand => new Command(() =>
    {
        string message = Uri.EscapeDataString("¡Hola! Encontré un can sin hogar y quiero más información.");
        string url = $"https://wa.me/593995322351?text={message}";

        Launcher.OpenAsync(new Uri(url));
    });
}
