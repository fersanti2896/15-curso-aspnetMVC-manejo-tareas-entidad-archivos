using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoTareas.Services {
    public class Constantes {
        public const string RolAdmin = "admin";

        public static readonly SelectListItem[] culturasSoportadas = new SelectListItem[] {
            new SelectListItem{Value = "es", Text = "Español"},
            new SelectListItem{Value = "en", Text = "English"}
        };
    }
}
