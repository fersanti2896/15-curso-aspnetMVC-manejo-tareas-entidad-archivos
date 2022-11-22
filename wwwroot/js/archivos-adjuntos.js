﻿
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
