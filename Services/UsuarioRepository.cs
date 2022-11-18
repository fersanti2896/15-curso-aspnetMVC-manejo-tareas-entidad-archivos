using System.Security.Claims;

namespace ManejoTareas.Services {
    public class UsuarioRepository : IUsuarioRepository {
        private HttpContext httpContext;

        public UsuarioRepository(IHttpContextAccessor httpContextAccessor) {
            httpContext = httpContextAccessor.HttpContext;
        }
        public string ObtenerUsuarioId() {
            if (httpContext.User.Identity.IsAuthenticated) {
                var idClaim = httpContext.User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier)
                                                     .FirstOrDefault();

                return idClaim.Value;
            } else {
                throw new Exception("El usuario no está autenticado!");
            }
        }
    }
}
