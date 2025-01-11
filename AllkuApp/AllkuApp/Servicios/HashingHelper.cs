using System.Security.Cryptography;
using System.Text;

public static class HashingHelper
{
    // Método para hashear una contraseña usando SHA-256
    public static string HashPassword(string password)
    {
        // Crear una instancia de SHA256
        using (SHA256 sha256Hash = SHA256.Create())
        {
            // Convertir la contraseña a un arreglo de bytes
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Crear un StringBuilder para construir la cadena de texto hexadecimal
            StringBuilder builder = new StringBuilder();

            // Convertir cada byte a una cadena hexadecimal y agregarla al StringBuilder
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }

            // Devolver la cadena hexadecimal resultante
            return builder.ToString();
        }
    }
}