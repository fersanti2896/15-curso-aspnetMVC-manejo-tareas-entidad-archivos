using ManejoTareas.Models;

namespace ManejoTareas.Services {
    public interface IAlmacenadorArchivosRepository {
        Task Borrar(string ruta, string contenedor);
        Task<AlmacenarArchivoResultado[]> Almacenar(string contenedor, IEnumerable<IFormFile> archivos);
    }
}
