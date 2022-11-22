
async function manejarErrorAPI(resp) {
    let msgError = '';

    if (resp.status === 400) {
        msgError = await resp.text();
    } else if (resp.status === 404) {
        msgError = recursoNoEncontrado;
    } else {
        msgError = errorInesperado;
    }

    mostrarMsgError(msgError);
}

function mostrarMsgError(msg) {
    Swal.fire({
        icon: 'error',
        title: '¡Error!',
        text: msg
    });
}

function confirmarAccion({ callBackAceptar, callBackCancelar, titulo }) {
    Swal.fire({
        title: titulo || '¿Realmente desear realizar la operación?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Si',
        focusConfirm: true
    }).then(result => {
        if (result.isConfirmed) {
            callBackAceptar();
        } else if (callBackCancelar) {
            /* El usuario ha presionado el boton de cancelar */
            callBackCancelar();
        }
    });
}

/* Funcion que sirve para descargar archivo */
function descargarArchivo(url, nombre) {
    var link = document.createElement('a');
    link.download = nombre;
    link.target = "_blank";
    link.href = url;

    document.body.appendChild(link);
    link.click();

    document.body.removeChild(link);

    delete link;
}