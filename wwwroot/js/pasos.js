
function manejarAgregarPaso() {
    const indice = editarTareaViewModel.pasos().findIndex(p => p.esNuevo());

    if (indice !== -1) {
        return;
    }

    editarTareaViewModel.pasos.push(new pasoViewModel({ modoEdicion: true, realizado: false }));
    $("[name=txtPasoDescripcion]:visible").focus();
}

function obtenerCuerpoPeticion(paso) {
    return JSON.stringify({
        descripcion: paso.descripcion(),
        realizado: paso.realizado()
    });
}

async function insertarPaso(paso, data, idTarea) {
    const resp = await fetch(`${urlPasos}/${idTarea}`, {
        method: 'POST',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (resp.ok) {
        const json = await resp.json();
        paso.id(json.id);

        const tarea = obtenerTareaEdicion();
        tarea.pasosTotal(tarea.pasosTotal() + 1);


        if (paso.realizado()) {
            tarea.pasosRealizados(tarea.pasosRealizados() + 1);
        }
    } else {
        manejarErrorAPI(resp);
    }
}

async function manejarSalvarPaso(paso) {
    paso.modoEdicion(false);

    const esNuevo = paso.esNuevo();
    const idTarea = editarTareaViewModel.id;
    const data = obtenerCuerpoPeticion(paso);

    const descripcion = paso.descripcion();

    if (!descripcion) {
        paso.descripcion(paso.descripcionAnterior);

        if (esNuevo) {
            editarTareaViewModel.pasos.pop()
        }

        return;
    }

    if (esNuevo) {
        await insertarPaso(paso, data, idTarea);
    } else {
        actualizarPaso(data, paso.id());
    }
}

function manejarCancelarPaso(paso) {
    if (paso.esNuevo()) {
        editarTareaViewModel.pasos.pop();
    } else {
        paso.modoEdicion(false);
        paso.descripcion(paso.descripcionAnterior);
    }
}

/* Actualizando un paso */
function manejarDescripcionPaso(paso) {
    paso.modoEdicion(true);
    paso.descripcionAnterior = paso.descripcion();
    $("[name=txtPasoDescripcion]:visible").focus();
}

async function actualizarPaso(data, id) {
    const resp = await fetch(`${urlPasos}/${id}`, {
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

/* Actualiza el paso como realizado */
function manejarCheckboxPaso(paso) {
    if (paso.esNuevo()) {
        return true;
    }

    const data = obtenerCuerpoPeticion(paso);
    actualizarPaso(data, paso.id());

    /* Actualiza el numero de pasos */
    const tarea = obtenerTareaEdicion();
    let pasosRealizadosActual = tarea.pasosRealizados();

    if (paso.realizado()) {
        pasosRealizadosActual++;
    } else {
        pasosRealizadosActual--;
    }

    tarea.pasosRealizados(pasosRealizadosActual);

    return true;
}

/* Borra un paso */
async function borrarPaso(paso) {
    const resp = await fetch(`${urlPasos}/${paso.id()}`, {
        method: 'DELETE'
    });

    if (!resp.ok) {
        manejarErrorAPI(resp);

        return;
    }

    editarTareaViewModel.pasos.remove(function (item) {
        return item.id() == paso.id()
    });

    const tarea = obtenerTareaEdicion();
    tarea.pasosTotal(tarea.pasosTotal() - 1);

    if (paso.realizado()) {
        tarea.pasosRealizados(tarea.pasosRealizados() - 1);
    }
}

function manejarBorrarPaso(paso) {
    modalEditarTarea.hide();

    confirmarAccion({
        callBackAceptar: () => {
            borrarPaso(paso);
            modalEditarTarea.show();
        },
        callBackCancelar: () => {
            modalEditarTarea.show();
        },
        titulo: '¿Desea borrar este paso?'
    });
}

/* Obtiene los id de los pasos */
function obtenerIdsPasos() {
    const ids = $("[name=chbPaso]").map(function () {
        return $(this).attr('data-id');
    }).get();

    return ids;
}

/* Manda los pasos al backend */
async function enviarIdsPasos(ids) {
    var data = JSON.stringify(ids);

    await fetch(`${urlPasos}/ordenar/${editarTareaViewModel.id}`, {
        method: 'POST',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });
}

/* Ordena los pasos */
async function actualizarOrdenPasos() {
    const ids = obtenerIdsPasos();
    await enviarIdsPasos(ids);

    const arreOrganizado = editarTareaViewModel.pasos.sorted(function (a, b) {
        return ids.indexOf(a.id().toString()) - ids.indexOf(b.id().toString());
    });

    editarTareaViewModel.pasos(arreOrganizado);
}

$(function () {
    $("#reordenable-pasos").sortable({
        axis: 'y',
        stop: async function () {
            await actualizarOrdenPasos();
        }
    })
})

