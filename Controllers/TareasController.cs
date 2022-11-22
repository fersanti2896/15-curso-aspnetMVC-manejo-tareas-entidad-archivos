using AutoMapper;
using AutoMapper.QueryableExtensions;
using ManejoTareas.Entities;
using ManejoTareas.Models;
using ManejoTareas.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManejoTareas.Controllers {
    [Route("api/tareas")]
    public class TareasController : ControllerBase {
        private readonly ApplicationDbContext context;
        private readonly IUsuarioRepository usuarioRepository;
        private readonly IMapper mapper;

        public TareasController(ApplicationDbContext context, 
                                IUsuarioRepository usuarioRepository,
                                IMapper mapper) {
            this.context = context;
            this.usuarioRepository = usuarioRepository;
            this.mapper = mapper;
        }

        /* Listado de tareas */
        [HttpGet]
        public async Task<List<TareaDTO>> Get() {
            var usuarioID = usuarioRepository.ObtenerUsuarioId();
            var tareas = await context.Tareas.Where(t => t.UsuarioId == usuarioID)
                                             .OrderBy(t => t.Orden)
                                             .ProjectTo<TareaDTO>(mapper.ConfigurationProvider)
                                             .ToListAsync();

            return tareas;
        }

        /* Obtener una tarea para editar */
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Tarea>> Get(int id) {
            var usuarioID = usuarioRepository.ObtenerUsuarioId();
            var tarea = await context.Tareas
                                     .Include(t => t.Pasos.OrderBy(p => p.Orden)) /* Carga los pasos de la tarea */
                                     .Include(t => t.ArchivosAdjuntos.OrderBy(p => p.Orden))
                                     .FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == usuarioID);

            if (tarea is null) {
                return NotFound();
            }

            return tarea;
        }

        /* Creación de tarea */
        [HttpPost]
        public async Task<ActionResult<Tarea>> Post([FromBody] string titulo) {
            var usuarioID = usuarioRepository.ObtenerUsuarioId();
            var existeTarea = await context.Tareas.AnyAsync(t => t.UsuarioId == usuarioID);
            var ordenMayor = 0;

            if (existeTarea) {
                ordenMayor = await context.Tareas.Where(t => t.UsuarioId == usuarioID)
                                                 .Select(t => t.Orden)
                                                 .MaxAsync();
            }

            var tarea = new Tarea { 
                Titulo = titulo,
                UsuarioId = usuarioID,
                FechaCreacion = DateTime.UtcNow,
                Orden = ordenMayor + 1 
            };

            context.Add(tarea);
            await context.SaveChangesAsync();

            return tarea;
        }

        [HttpPost("ordenar")]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids) {
            var usuarioID = usuarioRepository.ObtenerUsuarioId();
            var tareas = await context.Tareas.Where(t => t.UsuarioId == usuarioID)
                                             .ToListAsync();
            var tareaID = tareas.Select(t => t.Id);
            var tareaIDNoUsuario = ids.Except(tareaID).ToList();

            if (tareaIDNoUsuario.Any()) {
                return Forbid();
            }

            var tareaDic = tareas.ToDictionary(x => x.Id);

            for (int i = 0; i < ids.Length; i++) {
                var id = ids[i];
                var tarea = tareaDic[id];
                tarea.Orden = i + 1;
            }   

            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditarTarea(int id, [FromBody] TareaEditarDTO tareaEditarDTO) {
            var usuarioID = usuarioRepository.ObtenerUsuarioId();
            var tarea = await context.Tareas.FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == usuarioID);

            if (tarea is null) {
                return NotFound();
            }

            tarea.Titulo = tareaEditarDTO.Titulo;
            tarea.Descripcion = tareaEditarDTO.Descripcion;

            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id) {
            var usuarioID = usuarioRepository.ObtenerUsuarioId();
            var tarea = await context.Tareas.FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == usuarioID);

            if (tarea is null) {
                return NotFound();
            }

            context.Remove(tarea);
            await context.SaveChangesAsync();

            return Ok();
        }
    }
}
