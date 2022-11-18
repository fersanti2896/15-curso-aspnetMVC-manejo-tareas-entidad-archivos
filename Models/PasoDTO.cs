using System.ComponentModel.DataAnnotations;

namespace ManejoTareas.Models {
    public class PasoDTO {
        [Required]
        public string Descripcion { get; set; }
        public bool Realizado { get; set; }
    }
}
