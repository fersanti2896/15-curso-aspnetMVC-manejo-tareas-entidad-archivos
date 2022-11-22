using ManejoTareas.Entities;
using ManejoTareas.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManejoTareas.Controllers {
    [Route("api/archivos")]
    public class ArchivosController : ControllerBase {
        private readonly ApplicationDbContext context;
        private readonly IUsuarioRepository usuarioRepository;
        private readonly IAlmacenadorArchivosRepository almacenadorArchivos;
        private readonly string contenedor = "archivosadjuntos";

        public ArchivosController(ApplicationDbContext context, 
                                  IUsuarioRepository usuarioRepository,
                                  IAlmacenadorArchivosRepository almacenadorArchivos) {
            this.context = context;
            this.usuarioRepository = usuarioRepository;
            this.almacenadorArchivos = almacenadorArchivos;
        }

        [HttpPost("{tareaId:int}")]
        public async Task<ActionResult<IEnumerable<ArchivoAdjunto>>> Post(int tareaId, [FromForm] IEnumerable<IFormFile> archivos) {
            var usuarioID = usuarioRepository.ObtenerUsuarioId();
            var tarea = await context.Tareas.FirstOrDefaultAsync(t => t.Id == tareaId);

            if (tarea is null) {
                return NotFound();
            }

            if (tarea.UsuarioId != usuarioID) {
                return Forbid();
            }

            var existeArchivo = await context.ArchivosAdjuntos.AnyAsync(a => a.TareaId == tareaId);
            var ordenMayor = 0;

            if (existeArchivo) {
                ordenMayor = await context.ArchivosAdjuntos.Where(a => a.TareaId == tareaId)
                                                           .Select(a => a.Orden)
                                                           .MaxAsync();
            }

            /* Almacena los archivos */
            var resultados = await almacenadorArchivos.Almacenar(contenedor, archivos);
            var archivosAdj = resultados.Select((result, index) => new ArchivoAdjunto {
                                                    TareaId = tareaId,
                                                    FechaCreacion = DateTime.UtcNow,
                                                    Url = result.URL,
                                                    Titulo = result.Titulo,
                                                    Orden = ordenMayor + index + 1
                                                }).ToList();

            context.AddRange(archivosAdj);
            await context.SaveChangesAsync();

            return archivosAdj.ToList();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] string titulo) {
            var usuarioID = usuarioRepository.ObtenerUsuarioId();
            var archivoAdj = await context.ArchivosAdjuntos
                                          .Include(a => a.Tarea)
                                          .FirstOrDefaultAsync(a => a.Id == id);

            if (archivoAdj is null) {
                return NotFound();
            }

            if (archivoAdj.Tarea.UsuarioId != usuarioID) {
                return Forbid();
            }

            archivoAdj.Titulo = titulo;
            await context.SaveChangesAsync();

            return Ok();
        }
    }
}
