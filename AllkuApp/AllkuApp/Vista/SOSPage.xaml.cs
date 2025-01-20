using System;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace AllkuApp.Vista
{
    public partial class SOSPage : ContentPage
    {
        public SOSPage()
        {
            InitializeComponent();
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnClinicaTapped(object sender, EventArgs e)
        {
            if (sender is Label label)
            {
                string url = GetClinicUrl(label.Text);
                if (!string.IsNullOrEmpty(url))
                {
                    try
                    {
                        await Browser.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
                    }
                    catch (Exception)
                    {
                        await DisplayAlert("Error", "No se pudo abrir el mapa", "OK");
                    }
                }
            }
        }

        private void OnPhoneTapped(object sender, EventArgs e)
        {
            if (sender is Label label && label.GestureRecognizers[0] is TapGestureRecognizer tapGestureRecognizer)
            {
                string phoneNumber = tapGestureRecognizer.CommandParameter as string;
                if (!string.IsNullOrWhiteSpace(phoneNumber))
                {
                    try
                    {
                        PhoneDialer.Open(phoneNumber);
                    }
                    catch (Exception)
                    {
                        DisplayAlert("Error", "No se pudo realizar la llamada", "OK");
                    }
                }
            }
        }
        private string GetClinicUrl(string clinicName)
        {
            if (clinicName == "Mi Pet Clinic Veterinaria Sur de Quito")
                return "https://www.google.com.ec/maps/place/Mi+Pet+Clinic+Veterinaria+Sur+de+Quito/@-0.2683747,-78.5516043,17z/data=!3m1!4b1!4m6!3m5!1s0x91d5998d686a5f91:0x2ae2ed7bf9ca471a!8m2!3d-0.2683747!4d-78.5516043!16s%2Fg%2F11twg6rxkw?hl=es&entry=ttu&g_ep=EgoyMDI1MDExNS4wIKXMDSoASAFQAw%3D%3D";

            if (clinicName == "Hospital Veterinario Argos")
                return "https://www.google.com.ec/maps/place/Hospital+Veterinario+Argos/@-0.2033728,-78.4863937,17z/data=!3m1!4b1!4m6!3m5!1s0x91d59a0cfd17bc6d:0x26433911e445ab3f!8m2!3d-0.2033728!4d-78.4838188!16s%2Fg%2F11cj__gq_r?hl=es&entry=ttu&g_ep=EgoyMDI1MDExNS4wIKXMDSoASAFQAw%3D%3D";

            if (clinicName == "Veterinaria 24 HORAS, Entre Garras y Bugotes Sur de Quito")
                return "https://www.google.com.ec/maps/place/VETERINARIA+24+HORAS.+ENTRE+GARRAS+Y+BIGOTES+SUR+DE+QUITO/@-0.2666772,-78.5418523,17z/data=!3m1!4b1!4m6!3m5!1s0x91d599a9b20c2209:0x468cb9d7c42498d!8m2!3d-0.2666772!4d-78.5418523!16s%2Fg%2F11r3lzl8mq?hl=es&entry=ttu&g_ep=EgoyMDI1MDExNS4wIKXMDSoASAFQAw%3D%3D";

            if (clinicName == "Hospital Veterinaria Somalí")
                return "https://www.google.com.ec/maps/place/Hospital+Veterinaria+Somal%C3%AD/@-0.2630159,-78.5263831,17z/data=!3m1!4b1!4m6!3m5!1s0x91d598fa186356f7:0x99a5f70f89d85043!8m2!3d-0.2630159!4d-78.5263831!16s%2Fg%2F11f4qm67z2?hl=es&entry=ttu&g_ep=EgoyMDI1MDExNS4wIKXMDSoASAFQAw%3D%3D";

            if (clinicName == "Clinica VetService Veterinaria 24 Horas")
                return "https://www.google.com.ec/maps/place/CL%C3%8DNICA+VETSERVICE+VETERINARIA+24+HORAS/@-0.2457867,-78.5333864,17z/data=!3m1!4b1!4m6!3m5!1s0x91d599368d1a316d:0xbb8c6f6167855a6e!8m2!3d-0.2457867!4d-78.5333864!16s%2Fg%2F11jtcrx307?hl=es&entry=ttu&g_ep=EgoyMDI1MDExNS4wIKXMDSoASAFQAw%3D%3D";

            if (clinicName == "Hospital Veterinario Lucky")
                return "https://www.google.com.ec/maps/place/Hospital+Veterinario+Lucky/@-0.2850716,-78.4725924,17z/data=!3m1!4b1!4m6!3m5!1s0x91d597f833949b25:0x292e6274743ecbf2!8m2!3d-0.2850716!4d-78.4725924!16s%2Fg%2F11b7w5h0y9?hl=es&entry=ttu&g_ep=EgoyMDI1MDExNS4wIKXMDSoASAFQAw%3D%3D";

            if (clinicName == "All Pets")
                return "https://www.google.com.ec/maps/place/All+Pets/@-0.1824566,-78.4778993,17z/data=!3m1!4b1!4m6!3m5!1s0x91d59a80898080f7:0xd94939db6b116b32!8m2!3d-0.1824566!4d-78.4778993!16s%2Fg%2F11bv30k24b?hl=es&entry=ttu&g_ep=EgoyMDI1MDExNS4wIKXMDSoASAFQAw%3D%3D";

            if (clinicName == "Veterinaria Manimal")
                return "https://www.google.com.ec/maps/place/Veterinaria+Manimal/@-0.2946592,-78.4781802,17z/data=!3m1!4b1!4m6!3m5!1s0x91d59801d1d4a1ff:0x22d90f406667c311!8m2!3d-0.2946592!4d-78.4781802!16s%2Fg%2F11crztxbgg?hl=es&entry=ttu&g_ep=EgoyMDI1MDExNS4wIKXMDSoASAFQAw%3D%3D";

            if (clinicName == "Doctorvet Christian Saltos")
                return "https://www.google.com.ec/maps/place/Doctorvet+Christian+Saltos/@-0.1913569,-78.482286,17z/data=!3m1!4b1!4m6!3m5!1s0x91d59a7a36d4be3b:0x3944f19bffbf260e!8m2!3d-0.1913569!4d-78.482286!16s%2Fg%2F11g9nf_hmr?hl=es&entry=ttu&g_ep=EgoyMDI1MDExNS4wIKXMDSoASAFQAw%3D%3D";

            if (clinicName == "Hospital Veterinario Vetcarecenter")
                return "https://www.google.com.ec/maps/place/Hospital+Veterinario+Vetcarecenter/@-0.2008213,-78.4826833,17z/data=!3m1!4b1!4m6!3m5!1s0x91d59bdfe874e0ad:0x683d612f7f100c6e!8m2!3d-0.2008213!4d-78.4826833!16s%2Fg%2F11s38lq_5n?hl=es&entry=ttu&g_ep=EgoyMDI1MDExNS4wIKXMDSoASAFQAw%3D%3D";

            return null; // Si no se encuentra la clínica.
        }
    }
}