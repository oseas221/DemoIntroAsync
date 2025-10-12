using System.Diagnostics;

namespace DemoIntroAsync
{
    public partial class Form1 : Form
    {
        HttpClient httpClient = new HttpClient();

        public Form1()
        {
            InitializeComponent();

            // Configura el HttpClient con un User-Agent válido
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Visible = true;

            var directorioActual = AppDomain.CurrentDomain.BaseDirectory;

            var destinoBaseSecuencial = Path.Combine(directorioActual, @"Imagenes\resultado-secuencial");

            var destinoBaseParalelo = Path.Combine(directorioActual, @"Imagenes\resultado-paralelo");

            PrepararEjecución(destinoBaseParalelo, destinoBaseSecuencial);

            Console.WriteLine("inicio");
            List<Imagen> imagenes = ObtenerImagenes();

            //Manera secuencial
            var sw = new Stopwatch();
            sw.Start();
            foreach (var imagen in imagenes)
            {
                await ProcesarImagen(destinoBaseSecuencial, imagen);
            }

            Console.WriteLine("Secuencial - duracion en segundos : {0}",
                sw.ElapsedMilliseconds / 1000.0);
            sw.Reset();

            sw.Start();

            var tareasEnumerable = imagenes.Select(async imagen =>
            {
                await ProcesarImagen(destinoBaseParalelo, imagen);
            });

            await Task.WhenAll(tareasEnumerable);
            Console.WriteLine("Paralelo - duracion en segundos : {0}",
                sw.ElapsedMilliseconds / 1000.0);

            sw.Stop();

            pictureBox1.Visible = false;
        }

        private async Task ProcesarImagen(string directorio, Imagen imagen)
        {
            try
            {
                var respuesta = await httpClient.GetAsync(imagen.URL);

                // Verifica que la descarga fue exitosa
                if (!respuesta.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al descargar {imagen.Nombre}: {respuesta.StatusCode}");
                    return;
                }

                var contenido = await respuesta.Content.ReadAsByteArrayAsync();

                // Verifica que el contenido no esté vacío
                if (contenido == null || contenido.Length == 0)
                {
                    Console.WriteLine($"Contenido vacío para {imagen.Nombre}");
                    return;
                }

                Bitmap bitmap;
                using (var ms = new MemoryStream(contenido))
                {
                    // Crea una copia del bitmap para que no dependa del stream
                    using (var imagenTemp = Image.FromStream(ms))
                    {
                        bitmap = new Bitmap(imagenTemp);
                    }
                }

                bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                var destino = Path.Combine(directorio, imagen.Nombre);
                bitmap.Save(destino);

                // Libera la memoria del bitmap
                bitmap.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando {imagen.Nombre}: {ex.Message}");
            }
        }

        private static List<Imagen> ObtenerImagenes()
        {
            var imagenes = new List<Imagen>();

            for (int i = 0; i < 7; i++)
            {
                imagenes.Add(
                    new Imagen()
                    {
                        Nombre = $"JoyadeCeren {i}.jpeg",
                        URL = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/7a/ES_JoyadeCeren_06_2011_Estructura_9_Area_2_Tamazcal_2106_zoom_out.jpg/1024px-ES_JoyadeCeren_06_2011_Estructura_9_Area_2_Tamazcal_2106_zoom_out.jpg"
                    });
                imagenes.Add(
                    new Imagen()
                    {
                        Nombre = $"Tazumal {i}.jpeg",
                        URL = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/df/Templo_tazumal.jpg/800px-Templo_tazumal.jpg"
                    });
                imagenes.Add(
                    new Imagen()
                    {
                        Nombre = $"Añil {i}.jpeg",
                        URL = "https://upload.wikimedia.org/wikipedia/commons/8/83/Indigofera_tinctoria0.jpg"

                    });

            }

            return imagenes;
        }

        private void BorrarArchivos(string directorio)
        {
            var archivos = Directory.EnumerateFiles(directorio);
            foreach (var archivo in archivos)
            {
                File.Delete(archivo);
            }
        }

        private void PrepararEjecución(string destinoBaseParalelo, string destinoBaseSecuencial)
        {
            if (!Directory.Exists(destinoBaseParalelo))
            {
                Directory.CreateDirectory(destinoBaseParalelo);
            }

            if (!Directory.Exists(destinoBaseSecuencial))
            {
                Directory.CreateDirectory(destinoBaseSecuencial);
            }

            BorrarArchivos(destinoBaseSecuencial);
            BorrarArchivos(destinoBaseParalelo);
        }

        private async Task<string> ProcesamientoLargo()
        {
            await Task.Delay(3000);
            return "Dayanna";
        }

        private async Task RealizarProcesamientoLargoA()
        {
            await Task.Delay(1000);
            Console.WriteLine("Procesos A Finalizado");
        }

        private async Task RealizarProcesamientoLargoB()
        {
            await Task.Delay(1000);
            Console.WriteLine("Procesos B Finalizado");
        }

        private async Task RealizarProcesamientoLargoC()
        {
            await Task.Delay(1000);
            Console.WriteLine("Procesos C Finalizado");
        }
    }
}
