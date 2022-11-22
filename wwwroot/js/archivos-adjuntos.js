
let inputArchivoTarea = document.getElementById('archivoATarea')

function manejarAgregarArchivo() {
    inputArchivoTarea.click();
}

async function manejarSeleccionarArchivo(event) {
    const archivos = event.target.files;
    const archivosArreglo = Array.from(archivos);

    const idTarea = editarTareaViewModel.id;
    const formData = new FormData();

    for (var i = 0; i < archivosArreglo.length; i++) {
        formData.append("archivos", archivosArreglo[i]);
    }

    const resp = await fetch(`${urlArchivos}/${idTarea}`, {
        method: 'POST',
        body: formData        
    });

    if (!resp.ok) {
        manejarErrorAPI(resp);
        return;
    }

    const json = await resp.json();
    prepararArchivosAdjuntos(json);

    inputArchivoTarea.value = null;
}

function prepararArchivosAdjuntos(archivosAdjuntos) {
    archivosAdjuntos.forEach(archivoAdjunto => {
        let fechaCreacion = archivoAdjunto.fechaCreacion;

        if (archivoAdjunto.fechaCreacion.indexOf('Z') === -1) {
            fechaCreacion += 'Z';
        }

        const fechaCreationDT = new Date(fechaCreacion);
        archivoAdjunto.publicado = fechaCreationDT.toLocaleString();

        editarTareaViewModel.archivosAdjuntos.push(
            new archivoAdjuntoViewModel({ ...archivoAdjunto, modoEdicion: false }));
    });
}

let tituloArchivoAdjAnterior;

function manejarTituloArchivoAdj(archivoAdjunto) {
    archivoAdjunto.modoEdicion(true);
    tituloArchivoAdjAnterior = archivoAdjunto.titulo();

    $("[name='txtArchivoAdjuntoTitulo']:visible").focus();
}


async function manejarFocosoutTituloArchivoAdj(archivoAdjunto) {
    archivoAdjunto.modoEdicion(false);

    const idTarea = archivoAdjunto.id;

    if (!archivoAdjunto.titulo()) {
        archivoAdjunto.titulo(tituloArchivoAdjAnterior);
    }

    if (archivoAdjunto.titulo() === tituloArchivoAdjAnterior) {
        return;
    }

    const data = JSON.stringify(archivoAdjunto.titulo());
    const resp = await fetch(`${urlArchivos}/${idTarea}`, {
        method: 'PUT',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!resp.ok) {
        manejarErrorAPI(resp);
    }
}

function manejarBorrarArchivoAdj(archivoAdjunto) {
    modalEditarTarea.hide();

    confirmarAccion({
        callBackAceptar: () => {
            borrarArchivoAdjunto(archivoAdjunto);
            modalEditarTarea.show();
        },
        callBackCancelar: () => {
            modalEditarTarea.show();
        },
        titulo: '¿Deseas borrar este archivo adjunto?'
    });
}

async function borrarArchivoAdjunto(archivoAdjunto) {
    const resp = await fetch(`${urlArchivos}/${archivoAdjunto.id}`, {
        method: 'DELETE'
    });

    if (!resp.ok) {
        manejarErrorAPI(resp);

        return;
    }

    editarTareaViewModel.archivosAdjuntos.remove(function (item) { return item.id == archivoAdjunto.id });
}