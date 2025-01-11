using Xamarin.Essentials;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class PerfilCaninoService
{
    private const string PerfilCaninoKey = "PerfilCanino";

    // Obtener el perfil del canino de las preferencias
    public Task<PerfilCanino> ObtenerPerfilCanino()
    {
        var perfilJson = Preferences.Get(PerfilCaninoKey, string.Empty);
        if (string.IsNullOrEmpty(perfilJson))
        {
            return Task.FromResult<PerfilCanino>(null);
        }

        var perfilCanino = JsonConvert.DeserializeObject<PerfilCanino>(perfilJson);
        return Task.FromResult(perfilCanino);
    }

    // Guardar el perfil del canino en las preferencias
    public Task GuardarPerfilCanino(PerfilCanino perfil)
    {
        var perfilJson = JsonConvert.SerializeObject(perfil);
        Preferences.Set(PerfilCaninoKey, perfilJson);
        return Task.CompletedTask;
    }
}

// Modelo de datos del perfil del canino
public class PerfilCanino
{
    public int IdCanino { get; set; }   

    public string NombreCanino { get; set; }

    public string RazaCanino { get; set; }
    public int EdadCanino { get; set; }
    public decimal PesoCanino { get; set; }
    public string CelularDueno { get; set; }
    public byte[] FotoCanino { get; set; }
}