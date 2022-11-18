using AutoMapper;
using ManejoTareas.Entities;
using ManejoTareas.Models;

namespace ManejoTareas.Services {
    public class AutoMapperProfiles : Profile {
        public AutoMapperProfiles() {
            /* Obtiene el total de pasos para el listado de tareas */
            CreateMap<Tarea, TareaDTO>().ForMember(dto => dto.PasosTotal, 
                                                   ent => ent.MapFrom(x => x.Pasos.Count()))
                                        .ForMember(dto => dto.PasosRealizados, 
                                                   ent => ent.MapFrom(x => x.Pasos.Where(p => p.Realizado)
                                                                                  .Count()));
        }
    }
}
