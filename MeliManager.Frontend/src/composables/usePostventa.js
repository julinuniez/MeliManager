import { ref, computed, watch } from 'vue';
import Swal from 'sweetalert2'; // <-- Importamos la librería mágica

export function usePostventa() {
  const productosBase = [
    { sku: 'ALARMA-MOTO-01', nombre: 'Alarma Inalámbrica' },
    { sku: 'CANDADO-GPS-02', nombre: 'Candado U-Lock' },
    { sku: 'LINGA-ACERO-03', nombre: 'Linga de Acero 1.5m' }
  ];

  const skuSeleccionado = ref(productosBase[0].sku);
  
  const mensajeFijo = ref("¡Hola! Muchas gracias por elegirnos. Cualquier consulta nos comunicamos por este medio.");
  const mensajeSeguimiento = ref("¡Hola! Vimos que ya recibiste tu compra. Esperamos que esté todo perfecto.\n\nSi tenés un minuto, nos ayudaría muchísimo que nos dejes una calificación en la plataforma contándonos tu experiencia. ¡Gracias por confiar en nosotros!");

  const mensajesDinamicos = ref({
    'ALARMA-MOTO-01': "Te adjuntamos el manual en PDF para que puedas ver el esquema de instalación y cómo configurar la sensibilidad del sensor.\n\nRecordá que el control remoto ya incluye la pila puesta.",
    'CANDADO-GPS-02': "Para vincular el candado con tu celular, escaneá el código QR que viene en la caja. Si tenés dudas con la app, avisanos.",
    'LINGA-ACERO-03': "Te recordamos que la linga incluye 2 llaves computarizadas. ¡Te sugerimos guardar la copia en un lugar seguro!"
  });

  const archivosAdjuntos = ref({
    'ALARMA-MOTO-01': 'Guia_Instalacion_Alarma.pdf',
    'CANDADO-GPS-02': null,
    'LINGA-ACERO-03': null
  });

  const textoDinamicoActual = ref(mensajesDinamicos.value[skuSeleccionado.value] || "");
  const archivoActual = ref(archivosAdjuntos.value[skuSeleccionado.value] || null);

  watch(skuSeleccionado, (nuevoSku) => {
    textoDinamicoActual.value = mensajesDinamicos.value[nuevoSku] || "";
    archivoActual.value = archivosAdjuntos.value[nuevoSku] || null;
  });

  watch(textoDinamicoActual, (nuevoTexto) => {
    mensajesDinamicos.value[skuSeleccionado.value] = nuevoTexto;
  });

  const manejarSubidaArchivo = (event) => {
    const file = event.target.files[0];
    if (file) {
      archivoActual.value = file.name;
      archivosAdjuntos.value[skuSeleccionado.value] = file.name;
    }
  };

  const eliminarArchivo = () => {
    archivoActual.value = null;
    archivosAdjuntos.value[skuSeleccionado.value] = null;
  };

  const mensajeFinalPreview = computed(() => {
    const textoVariable = textoDinamicoActual.value ? `\n\n${textoDinamicoActual.value}` : "";
    return `${mensajeFijo.value}${textoVariable}`;
  });

  const guardarConfiguracion = () => {
    // NUEVO: El SweetAlert reemplaza al alert() nativo
    Swal.fire({
      title: '¡Reglas Guardadas!',
      text: 'Los mensajes, el PDF adjunto y la solicitud de calificación a las 72hs están listos para operar.',
      icon: 'success',
      confirmButtonText: 'Genial',
      confirmButtonColor: '#3483fa', // Color de ML
      background: '#ffffff',
      backdrop: `rgba(0,0,0,0.4)`
    });
  };

  return {
    productosBase,
    skuSeleccionado,
    mensajeFijo,
    mensajeSeguimiento,
    textoDinamicoActual,
    archivoActual,
    mensajeFinalPreview,
    manejarSubidaArchivo,
    eliminarArchivo,
    guardarConfiguracion
  };
}