<script setup>
import { ref, watch, computed } from 'vue';
import Swal from 'sweetalert2';
import axios from 'axios'; // <-- Importamos axios para conectarnos con C#

const props = defineProps({
  producto: Object,
  visible: Boolean
});

const emit = defineEmits(['cerrar', 'guardar']);

const tabActiva = ref('detalles');
const form = ref({});
const fotoActiva = ref(0);

// Computado que transforma el texto JSON de C# en una lista real de imágenes
const galeriaImagenes = computed(() => {
  if (!form.value || !form.value.galeriaJson) return [];
  try {
    return JSON.parse(form.value.galeriaJson);
  } catch (e) {
    return [];
  }
});

watch(() => props.producto, (nuevoProd) => {
  if (nuevoProd) {
    form.value = { ...nuevoProd };
    tabActiva.value = 'detalles'; 
    fotoActiva.value = 0; // Reiniciamos a la primera foto al abrir un producto nuevo
  }
}, { immediate: true });

const cerrar = () => emit('cerrar');

const guardar = async () => {
  try {
    // 1. Mostramos la pantalla de carga para que el usuario sepa que está trabajando
    Swal.fire({
      title: 'Sincronizando con Mercado Libre...',
      text: 'Enviando cambios al servidor e impactando en tu cuenta oficial.',
      allowOutsideClick: false,
      didOpen: () => {
        Swal.showLoading();
      }
    });

    // 2. Hacemos la petición PUT a nuestro C# enviando el SKU y los datos modificados
    const url = `http://localhost:7200/api/productos/${form.value.sku}/sincronizar-hacia-ml`;
    
    await axios.put(url, {
      nombre: form.value.nombre,
      precioVenta: form.value.precioVenta,
      stock: form.value.stock,
      estado: form.value.estado
    });

    // 3. Si C# y Mercado Libre aprueban el cambio, mostramos el éxito
    Swal.fire({
      title: '¡Sincronización Exitosa!',
      text: 'La publicación ya fue actualizada en tu base de datos y en Mercado Libre.',
      icon: 'success',
      confirmButtonText: 'Genial',
      confirmButtonColor: '#00a650'
    });
    
    // Avisamos a la vista principal para que actualice la tarjeta visualmente
    emit('guardar', form.value);
  } catch (error) {
    console.error(error);
    
    // Si hay un error (ej: el precio es muy bajo para ML, o el token expiró), capturamos el mensaje
    const mensajeError = error.response?.data?.error || 'No se pudieron guardar los cambios.';
    const detalleError = error.response?.data?.detalle || '';
    
    Swal.fire({
      title: 'Error al sincronizar',
      html: `<b>${mensajeError}</b><br><small>${detalleError}</small>`,
      icon: 'error',
      confirmButtonText: 'Revisar',
      confirmButtonColor: '#dc3545'
    });
  }
};
</script>

<template>
  <div class="modal-overlay" v-if="visible" @click.self="cerrar">
    <div class="modal-contenido">
      
      <div class="modal-header">
        <div>
          <span class="sku-badge">{{ form.sku }}</span>
          <h2>Editar Publicación</h2>
        </div>
        <button class="btn-cerrar" @click="cerrar">✕</button>
      </div>

      <div class="tabs">
        <button :class="['tab-btn', { activo: tabActiva === 'detalles' }]" @click="tabActiva = 'detalles'">Datos Principales</button>
        <button :class="['tab-btn', { activo: tabActiva === 'imagenes' }]" @click="tabActiva = 'imagenes'">Galería Visual</button>
        <button :class="['tab-btn', { activo: tabActiva === 'metricas' }]" @click="tabActiva = 'metricas'">Métricas de Crecimiento</button>
      </div>

      <div class="tab-content" v-if="tabActiva === 'detalles'">
        
        <div class="grupo-input">
          <div class="label-contador">
            <label>Título de la Publicación</label>
            <span class="contador" :class="{'limite-alcanzado': form.nombre?.length === 60}">
              {{ form.nombre?.length || 0 }} / 60
            </span>
          </div>
          <input type="text" v-model="form.nombre" class="input-texto" maxlength="60" placeholder="Ej: Alarma Moto Presencia..." />
          <small class="hint">Máximo 60 caracteres.</small>
        </div>

        <div class="fila-inputs">
          <div class="grupo-input">
            <label>Precio de Venta ($ ARS)</label>
            <input type="number" v-model="form.precioVenta" class="input-texto" />
          </div>
          <div class="grupo-input">
            <label>Stock Disponible</label>
            <input type="number" v-model="form.stock" class="input-texto" />
          </div>
          <div class="grupo-input">
            <label>Estado</label>
            <select v-model="form.estado" class="input-select">
              <option value="publicado">Publicado (Activo)</option>
              <option value="pausado">Pausado</option>
            </select>
          </div>
        </div>
      </div>

      <div class="tab-content" v-if="tabActiva === 'imagenes'">
        <p class="instruccion">Visualizá la galería completa en alta definición sincronizada desde Mercado Libre.</p>
        
        <div class="visor-galeria" v-if="galeriaImagenes.length > 0">
          
          <div class="contenedor-foto-principal">
            <img :src="galeriaImagenes[fotoActiva]" class="foto-principal-modal" />
            <span class="etiqueta-portada" v-if="fotoActiva === 0">Portada Oficial</span>
          </div>
          
          <div class="miniaturas-contenedor">
            <img 
              v-for="(foto, index) in galeriaImagenes" 
              :key="index" 
              :src="foto" 
              class="miniatura" 
              :class="{ activa: index === fotoActiva }"
              @click="fotoActiva = index"
              :alt="`Foto ${index + 1}`"
            />
          </div>
        </div>

        <div class="visor-galeria" v-else-if="form.imagenUrl">
          <div class="contenedor-foto-principal">
            <img :src="form.imagenUrl" class="foto-principal-modal" />
            <span class="etiqueta-portada">Portada Oficial</span>
          </div>
          <p class="hint" style="text-align: center; margin-top: 10px;">Volvé a sincronizar el catálogo para descargar el resto de la galería.</p>
        </div>

      </div>

      <div class="tab-content metricas" v-if="tabActiva === 'metricas'">
        <div class="metrica-tarjeta">
          <h4>Visitas (Últimos 30 días)</h4>
          <span class="valor">1,248</span>
          <span class="tendencia positiva">↑ 12% vs mes anterior</span>
        </div>
        <div class="metrica-tarjeta">
          <h4>Tasa de Conversión</h4>
          <span class="valor">2.4%</span>
          <span class="tendencia negativa">↓ 0.5% vs categoría</span>
          <small>Sugerencia: Mejorar las imágenes para subir conversión.</small>
        </div>
        <div class="metrica-tarjeta">
          <h4>Unidades Vendidas</h4>
          <span class="valor">30</span>
          <span class="tendencia positiva">Lote inicial agotado</span>
        </div>
      </div>

      <div class="modal-footer">
        <button class="btn-secundario" @click="cerrar">Cancelar</button>
        <button class="btn-primario" @click="guardar">Sincronizar con ML</button>
      </div>

    </div>
  </div>
</template>

<style scoped>
.modal-overlay { position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 1000; backdrop-filter: blur(2px); }
.modal-contenido { background: white; width: 90%; max-width: 650px; border-radius: 12px; box-shadow: 0 10px 25px rgba(0,0,0,0.2); display: flex; flex-direction: column; max-height: 90vh; overflow: hidden; }

.modal-header { padding: 1.5rem; border-bottom: 1px solid #eee; display: flex; justify-content: space-between; align-items: flex-start; }
.modal-header h2 { margin: 5px 0 0 0; color: #333; font-size: 1.4rem; }
.sku-badge { background: #f1f3f5; padding: 4px 8px; border-radius: 4px; font-size: 0.8rem; color: #666; font-family: monospace; font-weight: bold; }
.btn-cerrar { background: none; border: none; font-size: 1.5rem; color: #999; cursor: pointer; transition: color 0.2s; }
.btn-cerrar:hover { color: #333; }

.tabs { display: flex; border-bottom: 2px solid #eee; background: #f8f9fa; }
.tab-btn { flex: 1; padding: 12px 0; border: none; background: none; font-weight: 600; color: #666; cursor: pointer; border-bottom: 3px solid transparent; transition: all 0.2s; font-size: 0.95rem; }
.tab-btn:hover { color: #3483fa; background: rgba(52, 131, 250, 0.05); }
.tab-btn.activo { color: #3483fa; border-bottom-color: #3483fa; background: white; }

.tab-content { padding: 1.5rem; overflow-y: auto; flex: 1; min-height: 250px; }
.grupo-input { display: flex; flex-direction: column; gap: 6px; margin-bottom: 1rem; }

.label-contador { display: flex; justify-content: space-between; align-items: baseline; }
.label-contador label { font-weight: 600; color: #444; font-size: 0.9rem; }
.contador { font-size: 0.8rem; color: #888; font-family: monospace; transition: color 0.2s; }
.limite-alcanzado { color: #dc3545; font-weight: bold; }

.fila-inputs { display: grid; grid-template-columns: repeat(3, 1fr); gap: 1rem; }
.input-texto, .input-select { padding: 10px; border: 1px solid #ccc; border-radius: 6px; font-size: 1rem; font-family: inherit; width: 100%; box-sizing: border-box; }
.input-texto:focus, .input-select:focus { outline: none; border-color: #3483fa; box-shadow: 0 0 0 2px rgba(52,131,250,0.2); }
.hint { font-size: 0.8rem; color: #888; }
.instruccion { color: #555; font-size: 0.95rem; margin-top: 0; margin-bottom: 1.5rem;}

.visor-galeria { display: flex; flex-direction: column; gap: 15px; }
.contenedor-foto-principal { position: relative; background: #f8f9fa; border: 1px solid #eee; border-radius: 8px; overflow: hidden; display: flex; justify-content: center; align-items: center; padding: 10px; }
.foto-principal-modal { width: 100%; max-height: 350px; object-fit: contain; }
.etiqueta-portada { position: absolute; top: 10px; left: 10px; background: rgba(0, 166, 80, 0.9); color: white; padding: 4px 10px; border-radius: 4px; font-size: 0.8rem; font-weight: bold; box-shadow: 0 2px 5px rgba(0,0,0,0.2); }

.miniaturas-contenedor { display: flex; gap: 10px; overflow-x: auto; padding-bottom: 5px; scrollbar-width: thin; }
.miniaturas-contenedor::-webkit-scrollbar { height: 6px; }
.miniaturas-contenedor::-webkit-scrollbar-thumb { background: #ccc; border-radius: 3px; }
.miniatura { width: 65px; height: 65px; object-fit: cover; border-radius: 6px; cursor: pointer; border: 2px solid transparent; opacity: 0.5; transition: all 0.2s; background: #f8f9fa; }
.miniatura:hover { opacity: 0.8; }
.miniatura.activa { border-color: #3483fa; opacity: 1; box-shadow: 0 2px 5px rgba(52,131,250,0.3); }

.metricas { display: grid; grid-template-columns: repeat(2, 1fr); gap: 1rem; align-content: start; }
.metrica-tarjeta { background: #f8f9fa; padding: 1.2rem; border-radius: 8px; border: 1px solid #eee; display: flex; flex-direction: column; }
.metrica-tarjeta h4 { margin: 0 0 10px 0; color: #555; font-size: 0.9rem; font-weight: 600; text-transform: uppercase; }
.metrica-tarjeta .valor { font-size: 2rem; font-weight: bold; color: #2d3277; margin-bottom: 5px; }
.tendencia { font-size: 0.85rem; font-weight: 600; margin-bottom: 5px; }
.tendencia.positiva { color: #00a650; }
.tendencia.negativa { color: #dc3545; }
.metrica-tarjeta small { color: #888; font-size: 0.8rem; line-height: 1.3; }

.modal-footer { padding: 1.2rem 1.5rem; background: #f8f9fa; border-top: 1px solid #eee; display: flex; justify-content: flex-end; gap: 1rem; }
.btn-secundario { background: white; color: #666; border: 1px solid #ccc; padding: 10px 20px; border-radius: 6px; font-weight: 600; cursor: pointer; transition: all 0.2s; }
.btn-secundario:hover { background: #f1f3f5; color: #333; }
.btn-primario { background: #3483fa; color: white; border: none; padding: 10px 20px; border-radius: 6px; font-weight: 600; cursor: pointer; transition: background 0.2s; }
.btn-primario:hover { background: #2968c8; }
</style>