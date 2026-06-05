<script setup>
import { usePostventa } from '../composables/usePostventa';

const { 
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
} = usePostventa();
</script>

<template>
  <div class="vista-postventa">
    
    <div class="encabezado">
      <h2>Automatización Post-Venta</h2>
      <p>Configurá los mensajes y PDFs que se enviarán solos apenas el comprador pague y al recibir el producto.</p>
    </div>

    <div class="grid-layout">
      
      <!-- COLUMNA IZQUIERDA: CONFIGURACIÓN -->
      <div class="panel-edicion">
        
        <!-- Bloque Fijo INMEDIATO -->
        <div class="caja-configuracion global">
          <div class="header-caja">
            <h3>Saludo Universal (Inmediato)</h3>
            <span class="badge-info">Se envía al confirmar el pago</span>
          </div>
          <div class="grupo-input">
            <textarea v-model="mensajeFijo" class="input-textarea textarea-fijo" rows="2"></textarea>
          </div>
        </div>

        <!-- Bloque Dinámico (Por Producto) -->
        <div class="caja-configuracion dinamica">
          <div class="header-caja">
            <h3>Mensaje y Adjuntos Específicos</h3>
            <span class="badge-dinamico">Depende del producto</span>
          </div>
          
          <div class="grupo-input">
            <label>Seleccionar Producto a Configurar</label>
            <select v-model="skuSeleccionado" class="input-select">
              <option v-for="prod in productosBase" :key="prod.sku" :value="prod.sku">
                {{ prod.sku }} - {{ prod.nombre }}
              </option>
            </select>
          </div>

          <div class="grupo-input mt-2">
            <label>Instrucciones y Detalles</label>
            <textarea v-model="textoDinamicoActual" class="input-textarea" rows="4" placeholder="Escribí un mensaje específico para este SKU..."></textarea>
          </div>

          <div class="grupo-input mt-2">
            <label>Documento Adjunto (PDF)</label>
            <div v-if="!archivoActual" class="zona-subida">
              <input type="file" id="subida-pdf" accept=".pdf" @change="manejarSubidaArchivo" class="input-oculto" />
              <label for="subida-pdf" class="btn-subir">📎 Seleccionar Archivo PDF</label>
              <small class="hint">El sistema lo enviará automáticamente al chat de la compra.</small>
            </div>
            <div v-else class="archivo-cargado">
              <div class="info-archivo">
                <span class="icono-archivo">📄</span>
                <span class="nombre-archivo">{{ archivoActual }}</span>
              </div>
              <button class="btn-eliminar-archivo" @click="eliminarArchivo" title="Quitar archivo">✕</button>
            </div>
          </div>
        </div>

        <!-- NUEVO: Bloque Fijo SEGUIMIENTO -->
        <div class="caja-configuracion seguimiento">
          <div class="header-caja">
            <h3>Solicitud de Calificación</h3>
            <span class="badge-verde">Se envía 3 días post-entrega</span>
          </div>
          <div class="grupo-input">
            <textarea v-model="mensajeSeguimiento" class="input-textarea textarea-fijo" rows="4"></textarea>
            <small class="hint">Este mensaje busca asegurar calificaciones de 5 estrellas para potenciar tu reputación.</small>
          </div>
        </div>

        <button class="btn-primario" @click="guardarConfiguracion">
          Guardar Reglas Automáticas
        </button>
      </div>

      <!-- COLUMNA DERECHA: SIMULADOR DE CHAT ML -->
      <div class="panel-preview">
        <h3>Vista Previa del Comprador</h3>
        
        <div class="simulador-celular">
          <div class="header-chat">
            <div class="avatar-ml"></div>
            <div class="info-chat">
              <strong>Mensajería de la compra</strong>
              <span>{{ skuSeleccionado }}</span>
            </div>
          </div>
          
          <div class="cuerpo-chat">
            <div class="fecha-chat">Día de la compra</div>
            
            <div class="burbuja-vendedor">
              <p class="texto-burbuja">{{ mensajeFinalPreview }}</p>
              <span class="hora-burbuja">14:05 ✓✓</span>
            </div>
            
            <div class="adjunto-pdf" v-if="archivoActual">
              <div class="icono-pdf">📄</div>
              <div class="info-pdf">
                <span>{{ archivoActual }}</span>
                <small>PDF Automático</small>
              </div>
            </div>

            <!-- SIMULACIÓN DEL SALTO DE TIEMPO -->
            <div class="fecha-chat mt-chat">3 días después de la entrega</div>

            <!-- BURBUJA DE SEGUIMIENTO -->
            <div class="burbuja-vendedor">
              <p class="texto-burbuja">{{ mensajeSeguimiento }}</p>
              <span class="hora-burbuja">10:30 ✓✓</span>
            </div>

          </div>
          
          <div class="input-chat-falso">
            <span>Escribí un mensaje...</span>
            <div class="icono-enviar">➤</div>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<style scoped>
.vista-postventa { max-width: 1100px; margin: 0 auto; padding-bottom: 3rem; }
.encabezado { margin-bottom: 2rem; }
.encabezado h2 { margin: 0; color: #333; }
.encabezado p { color: #666; margin-top: 5px; }

.grid-layout { display: grid; grid-template-columns: 1.2fr 1fr; gap: 2.5rem; align-items: start; }

.panel-edicion { display: flex; flex-direction: column; gap: 1.5rem; }

.caja-configuracion { background: white; padding: 1.5rem; border-radius: 8px; border: 1px solid #eee; box-shadow: 0 1px 3px rgba(0,0,0,0.05); }
.caja-configuracion.global { border-left: 4px solid #3483fa; }
.caja-configuracion.dinamica { border-left: 4px solid #ff9800; }
.caja-configuracion.seguimiento { border-left: 4px solid #00a650; } /* Borde verde para el seguimiento */

.header-caja { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; border-bottom: 1px solid #f1f3f5; padding-bottom: 10px; }
.header-caja h3 { margin: 0; font-size: 1.1rem; color: #333; }

.badge-info { background: #e6f0ff; color: #3483fa; padding: 4px 8px; border-radius: 4px; font-size: 0.75rem; font-weight: bold; }
.badge-dinamico { background: #fff3e0; color: #e65100; padding: 4px 8px; border-radius: 4px; font-size: 0.75rem; font-weight: bold; }
.badge-verde { background: #e6f6eb; color: #00a650; padding: 4px 8px; border-radius: 4px; font-size: 0.75rem; font-weight: bold; }

.grupo-input { display: flex; flex-direction: column; gap: 6px; margin-bottom: 1rem; }
.mt-2 { margin-top: 1rem; }
.grupo-input label { font-weight: 600; color: #444; font-size: 0.9rem; }
.input-select { padding: 10px; border: 1px solid #ccc; border-radius: 6px; font-size: 1rem; cursor: pointer; background-color: #f8f9fa; }
.input-textarea { padding: 12px; border: 1px solid #ccc; border-radius: 6px; font-size: 0.95rem; font-family: inherit; resize: vertical; line-height: 1.5; }
.input-textarea:focus, .input-select:focus { outline: none; border-color: #3483fa; box-shadow: 0 0 0 2px rgba(52,131,250,0.2); }
.textarea-fijo { background-color: #f8f9fa; color: #333; font-weight: 500; }
.hint { font-size: 0.8rem; color: #888; }

.zona-subida { display: flex; flex-direction: column; align-items: flex-start; gap: 8px; }
.input-oculto { display: none; }
.btn-subir { background-color: #f8f9fa; color: #333; border: 1px dashed #ccc; padding: 10px 15px; border-radius: 6px; font-weight: 600; cursor: pointer; transition: all 0.2s; display: inline-block; font-size: 0.95rem; }
.btn-subir:hover { background-color: #f1f3f5; border-color: #3483fa; color: #3483fa; }

.archivo-cargado { display: flex; align-items: center; justify-content: space-between; background: #e6f0ff; border: 1px solid #a0c4ff; padding: 10px 15px; border-radius: 6px; }
.info-archivo { display: flex; align-items: center; gap: 8px; }
.icono-archivo { font-size: 1.2rem; }
.nombre-archivo { font-weight: 600; color: #2d3277; font-size: 0.9rem; }
.btn-eliminar-archivo { background: none; border: none; color: #dc3545; font-weight: bold; cursor: pointer; font-size: 1.2rem; padding: 0 5px; }
.btn-eliminar-archivo:hover { color: #a71d2a; }

.btn-primario { background-color: #3483fa; color: white; border: none; padding: 14px; border-radius: 6px; font-weight: 600; cursor: pointer; transition: background 0.2s; font-size: 1.05rem; }
.btn-primario:hover { background-color: #2968c8; }

/* PANEL DERECHO */
.panel-preview { position: sticky; top: 90px; }
.panel-preview h3 { margin: 0 0 1rem 0; color: #333; font-size: 1.1rem; }

.simulador-celular { background: #f5f5f5; border: 8px solid #333; border-radius: 24px; overflow: hidden; height: 500px; display: flex; flex-direction: column; box-shadow: 0 10px 20px rgba(0,0,0,0.15); }

.header-chat { background: #fff159; padding: 15px; display: flex; align-items: center; gap: 10px; border-bottom: 1px solid rgba(0,0,0,0.1); }
.avatar-ml { width: 35px; height: 35px; background: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; box-shadow: 0 1px 2px rgba(0,0,0,0.1); }
.avatar-ml::after { content: '🤝'; font-size: 1.2rem; }
.info-chat { display: flex; flex-direction: column; }
.info-chat strong { font-size: 0.9rem; color: #333; }
.info-chat span { font-size: 0.75rem; color: #666; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 180px; }

.cuerpo-chat { flex: 1; padding: 15px; overflow-y: auto; display: flex; flex-direction: column; background-image: url('data:image/svg+xml,%3Csvg width="20" height="20" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg"%3E%3Cg fill="%23000000" fill-opacity="0.03" fill-rule="evenodd"%3E%3Ccircle cx="3" cy="3" r="3"/%3E%3Ccircle cx="13" cy="13" r="3"/%3E%3C/g%3E%3C/svg%3E'); scroll-behavior: smooth; }
.fecha-chat { text-align: center; font-size: 0.75rem; color: #888; background: rgba(0,0,0,0.05); padding: 4px 10px; border-radius: 12px; align-self: center; margin-bottom: 15px; }
.mt-chat { margin-top: 15px; }

.burbuja-vendedor { background: #e6f0ff; padding: 12px 14px; border-radius: 12px 12px 0 12px; align-self: flex-end; max-width: 85%; margin-bottom: 10px; box-shadow: 0 1px 2px rgba(0,0,0,0.05); position: relative; }
.texto-burbuja { margin: 0; font-size: 0.9rem; color: #333; line-height: 1.4; white-space: pre-wrap; }
.hora-burbuja { display: block; text-align: right; font-size: 0.65rem; color: #888; margin-top: 5px; }

.adjunto-pdf { background: white; border: 1px solid #e2e8f0; border-radius: 8px; padding: 10px; display: flex; align-items: center; gap: 10px; align-self: flex-end; max-width: 75%; box-shadow: 0 1px 2px rgba(0,0,0,0.05); }
.icono-pdf { font-size: 1.8rem; }
.info-pdf { display: flex; flex-direction: column; }
.info-pdf span { font-size: 0.85rem; font-weight: bold; color: #333; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 150px; }
.info-pdf small { font-size: 0.7rem; color: #888; }

.input-chat-falso { background: white; padding: 12px 15px; display: flex; justify-content: space-between; align-items: center; border-top: 1px solid #eee; color: #999; font-size: 0.9rem; }
.icono-enviar { background: #3483fa; color: white; width: 30px; height: 30px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 0.8rem; }
</style>