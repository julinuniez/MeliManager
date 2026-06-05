<script setup>
import { ref, watch } from 'vue';

const skuSeleccionado = ref('ALARMA-MOTO-01');
const mensaje = ref('¡Hola! Muchas gracias por elegirnos. Te adjuntamos el manual de instalación paso a paso de tu alarma. ¡Cualquier duda avisanos!');
const rutaPdf = ref('C:\\temp\\Manual-Alarma.pdf');

// Simular carga de datos si cambiás de SKU
watch(skuSeleccionado, (nuevoSku) => {
  if(nuevoSku === 'CANDADO-GPS-02') {
    mensaje.value = '¡Hola! Aquí tienes la guía de configuración de tu GPS...';
    rutaPdf.value = 'C:\\temp\\Guia-Candado.pdf';
  } else {
    mensaje.value = '¡Hola! Muchas gracias por elegirnos. Te adjuntamos el manual...';
    rutaPdf.value = 'C:\\temp\\Manual-Alarma.pdf';
  }
});

const guardarPostventa = () => {
  alert(`Se actualizó la mensajería automática para ${skuSeleccionado.value}`);
};
</script>

<template>
  <div class="vista-postventa">
    <div class="encabezado">
      <h2>Motor Postventa</h2>
      <p>Configurá los mensajes y manuales en PDF que recibe el cliente al comprar.</p>
    </div>

    <div class="panel-postventa">
      <div class="selector-producto">
        <label>Seleccionar Producto (SKU)</label>
        <select v-model="skuSeleccionado" class="input-select">
          <option value="ALARMA-MOTO-01">ALARMA-MOTO-01 (Alarma Inalámbrica)</option>
          <option value="CANDADO-GPS-02">CANDADO-GPS-02 (Candado U-Lock)</option>
        </select>
      </div>

      <div class="postventa-detalle" v-if="skuSeleccionado">
        <div class="grupo-input">
          <label>Mensaje de Agradecimiento</label>
          <textarea 
            v-model="mensaje" 
            class="input-textarea" 
            rows="4"
            placeholder="¡Hola! Gracias por tu compra..."
          ></textarea>
        </div>

        <div class="grupo-input">
          <label>Ruta local del Manual en PDF</label>
          <div class="input-con-ayuda">
            <input 
              type="text" 
              v-model="rutaPdf" 
              class="input-texto" 
              placeholder="Ej: C:\temp\Manual-Alarma.pdf" 
            />
            <small>Esta ruta debe coincidir con el archivo guardado en el servidor.</small>
          </div>
        </div>

        <button class="btn-guardar" @click="guardarPostventa">
          Guardar Configuración
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.vista-postventa { max-width: 800px; margin: 0 auto; }
.encabezado { margin-bottom: 2rem; }
.encabezado h2 { margin: 0; color: #333; }
.encabezado p { color: #666; margin-top: 5px; }

.panel-postventa {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
}

.selector-producto { margin-bottom: 2rem; display: flex; flex-direction: column; gap: 8px; }
.selector-producto label { font-weight: 600; color: #333; }

.input-select {
  padding: 12px;
  border: 1px solid #ccc;
  border-radius: 6px;
  font-size: 1rem;
  background-color: #f8f9fa;
  cursor: pointer;
}

.postventa-detalle {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  border-top: 1px solid #eee;
  padding-top: 1.5rem;
}

.grupo-input { display: flex; flex-direction: column; gap: 8px; }
.grupo-input label { font-weight: 600; color: #444; }

.input-texto, .input-textarea {
  padding: 12px;
  border: 1px solid #ccc;
  border-radius: 6px;
  font-size: 1rem;
  font-family: inherit;
}
.input-texto:focus, .input-textarea:focus, .input-select:focus { 
  outline: none; 
  border-color: #3483fa; 
  box-shadow: 0 0 0 2px rgba(52,131,250,0.2); 
}

.input-con-ayuda { display: flex; flex-direction: column; gap: 4px; }
.input-con-ayuda small { color: #888; font-size: 0.85rem; }

.btn-guardar {
  align-self: flex-start;
  background-color: #00a650; 
  color: white;
  border: none;
  padding: 12px 24px;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
  margin-top: 1rem;
}
.btn-guardar:hover { background-color: #008c44; }
</style>