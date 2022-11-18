using ManejoTareas.Entities;
using ManejoTareas.Models;
using ManejoTareas.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManejoTareas.Controllers {

    [Route("api/pasos")]
    public class PasosController : ControllerBase {
        private readonly ApplicationDbContext context;
        private readonly IUsuarioRepository usuarioRepository;

        public PasosController(ApplicationDbContext context,
                               IUsuarioRepository usuarioRepository) {
            this.context = context;
            this.usuarioRepository = usuarioRepository;
        }

        [HttpPost("{tareaId:int}")]
        public async Task<ActionResult<Paso>> Post(int tareaId, [FromBody] PasoDTO pasoDTO) {
            var usuarioID = usuarioRepository.ObtenerUsuarioId();
            var tarea = await context.Tareas.FirstOrDefaultAsync(t => t.Id == tareaId);

            if (tarea is null) {
                return NotFound();
            }

            if (tarea.UsuarioId != usuarioID) {
                return Forbid();
            }

            var existePasos = await context.Pasos.AnyAsync(p => p.TareaId == tareaId);
            var ordenMayor = 0;

            if (existePasos) {
                ordenMayor = await context.Pasos.Where(p => p.TareaId == tareaId)
                                                .Select(p => p.Orden)
                                                .MaxAsync();
            }

            var paso = new Paso();
            paso.TareaId = tareaId; 
            paso.Orden = ordenMayor + 1;
            paso.Descripcion = pasoDTO.Descripcion;
            paso.Realizado = pasoDTO.Realizado;

            context.Add(paso);
            await context.SaveChangesAsync();

            return paso;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] PasoDTO pasoDTO) {
            var usuarioID = usuarioRepository.ObtenerUsuarioId();
            var paso = await context.Pasos.Include(p => p.Tarea)
                                          .FirstOrDefaultAsync(p => p.Id == id);

            if (paso is null) {
                return NotFound();
            }

            if (paso.Tarea.UsuarioId != usuarioID) {
                return Forbid();
            }

            paso.Descripcion = pasoDTO.Descripcion;
            paso.Realizado = pasoDTO.Realizado;

            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id) {
            var usuarioID = usuarioRepository.ObtenerUsuarioId();
            var paso = await context.Pasos.Include(p => p.Tarea)
                                          .FirstOrDefaultAsync(t => t.Id == id);

            if (paso is null) {
                return NotFound();
            }

            if (paso.Tarea.UsuarioId != usuarioID) {
                return Forbid();
            }

            context.Remove(paso);
            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("ordenar/{tareaId:int}")]
        public async Task<IActionResult> Ordenar(int tareaId, [FromBody] Guid[] ids) {
            var usuarioID = usuarioRepository.ObtenerUsuarioId();
            var tarea = await context.Tareas.FirstOrDefaultAsync(t => t.Id == tareaId && t.UsuarioId == usuarioID);

            if (tarea is null) {
                return NotFound();
            }

            var pasos = await context.Pasos.Where(p => p.TareaId == tareaId).ToListAsync();
            var pasosIds = pasos.Select(x => x.Id);
            var pasoNoPertenece = ids.Except(pasosIds).ToList();

            if (pasoNoPertenece.Any()) { 
                return BadRequest("No todos los pasos están presentes");
            }

            var pasosDic = pasos.ToDictionary(p => p.Id);

            for (int i = 0; i < ids.Length; i++) {
                var id = ids[i];
                var paso = pasosDic[id];
                paso.Orden = i + 1;
            }

            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
