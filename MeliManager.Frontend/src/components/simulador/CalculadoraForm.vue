<script setup>
defineProps({
  productosBase: Array,
  form: Object,
  api: Object,
  pesoFisicoTotal: Number,
  pesoVolumetricoTotal: Number,
  pesoCobradoTotal: Number,
  fleteCalculado: Number,
  fobTotal: Number,
  costoDerechos: Number,
  costoIva: Number,
  totalImpuestos: Number,
  totalUsd: Number,
  costoUnitarioArs: Number,
  guardarSimulacion: Function
});
</script>

<template>
  <div class="panel-calculadora">
    <div class="formulario">
      
      <div class="grupo-input">
        <label>Seleccionar Producto</label>
        <select v-model="form.skuSeleccionado" class="input-select">
          <option v-for="prod in productosBase" :key="prod.sku" :value="prod.sku">
            {{ prod.sku }} - {{ prod.nombre }}
          </option>
        </select>
        <div class="info-producto" v-if="form.skuSeleccionado !== 'CUSTOM'">
          <small>Medidas base: {{ form.largo }}x{{ form.ancho }}x{{ form.alto }}cm | Peso base: {{ form.pesoKg }}kg</small>
        </div>
      </div>

      <div class="caja-logistica" v-if="form.skuSeleccionado === 'CUSTOM'">
        <div class="logistica-header">Medidas del Producto Nuevo</div>
        <div class="fila-medidas">
          <div class="grupo-input-pequeno">
            <label>Peso (kg)</label>
            <input type="number" v-model="form.pesoKg" class="input-texto" step="0.01" />
          </div>
          <div class="grupo-input-pequeno">
            <label>Largo (cm)</label>
            <input type="number" v-model="form.largo" class="input-texto" />
          </div>
          <div class="grupo-input-pequeno">
            <label>Ancho (cm)</label>
            <input type="number" v-model="form.ancho" class="input-texto" />
          </div>
          <div class="grupo-input-pequeno">
            <label>Alto (cm)</label>
            <input type="number" v-model="form.alto" class="input-texto" />
          </div>
        </div>
      </div>

      <div class="grupo-input">
        <label>Cantidad de Unidades</label>
        <input type="number" v-model="form.cantidad" class="input-texto" min="1" />
      </div>
      
      <div class="grupo-input">
        <label>Precio FOB Unitario (Editable)</label>
        <input type="number" v-model="form.fobUnitario" class="input-texto" step="0.01" />
        <small class="texto-subtotal">FOB Total: <strong>U$S {{ (fobTotal || 0).toFixed(2) }}</strong></small>
      </div>

      <div class="caja-logistica">
        <div class="logistica-header">Costos de Envío (Logística)</div>
        <div class="fila-medidas">
          <div class="grupo-input-pequeno">
            <label>Servicio Fijo (USD)</label>
            <input type="number" v-model="form.costoServicio" class="input-texto" step="1" />
          </div>
          <div class="grupo-input-pequeno">
            <label>Tasa x Kg (USD)</label>
            <input type="number" v-model="form.costoPorKg" class="input-texto" step="1" />
          </div>
        </div>
        <div class="comparativa-pesos">
          <div class="peso-item">
            <span>Físico Total</span>
            <strong>{{ (pesoFisicoTotal || 0).toFixed(2) }} kg</strong>
          </div>
          <div class="peso-item">
            <span>Volumétrico</span>
            <strong>{{ (pesoVolumetricoTotal || 0).toFixed(2) }} kg</strong>
          </div>
        </div>
        <div class="resumen-flete">
          <span>Chargeable Wgt: <strong class="texto-destaque">{{ (pesoCobradoTotal || 0).toFixed(2) }} kg</strong></span>
          <span>Flete Estimado: <strong class="texto-destaque">U$S {{ (fleteCalculado || 0).toFixed(2) }}</strong></span>
        </div>
      </div>

      <div class="caja-logistica aduanas">
        <div class="logistica-header">Impuestos de Nacionalización (Aduana)</div>
        <div class="fila-medidas">
          <div class="grupo-input-pequeno">
            <label>Derechos (%)</label>
            <input type="number" v-model="form.derechosPorcentaje" class="input-texto" step="0.1" />
          </div>
          <div class="grupo-input-pequeno">
            <label>IVA/Perc (%)</label>
            <input type="number" v-model="form.ivaPorcentaje" class="input-texto" step="0.1" />
          </div>
        </div>
      </div>
    </div>

    <div class="resultado-panel">
      <h3>Costo Unitario Final</h3>
      <div class="monto-destacado">
        <span v-if="api.cargandoDolar">...</span>
        <span v-else>${{ (costoUnitarioArs || 0).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}</span>
      </div>
      
      <div class="info-tc">
        <span class="detalle-unidades">Landed Cost por {{ form.cantidad }} unidades.</span>
        <div class="tc-contenedor">
          <span>TC Aplicado: </span>
          <span v-if="api.cargandoDolar" class="badge-cargando">Actualizando...</span>
          <span v-else><strong>${{ api.tipoCambio }}</strong> <span class="badge-api">Vía DolarAPI</span></span>
        </div>
      </div>
      
      <div class="desglose">
        <div class="item-desglose"><span>Subtotal FOB:</span><span>U$S {{ (fobTotal || 0).toFixed(2) }}</span></div>
        <div class="item-desglose"><span>Flete y Servicio:</span><span>U$S {{ (fleteCalculado || 0).toFixed(2) }}</span></div>
        <div class="item-desglose sub-item"><small>Derechos Imp.:</small><small>U$S {{ (costoDerechos || 0).toFixed(2) }}</small></div>
        <div class="item-desglose sub-item"><small>IVA y Percepciones:</small><small>U$S {{ (costoIva || 0).toFixed(2) }}</small></div>
        <div class="item-desglose"><span>Total Impuestos:</span><span>U$S {{ (totalImpuestos || 0).toFixed(2) }}</span></div>
        <div class="item-desglose destacado"><span>Costo Total Importación:</span><span>U$S {{ (totalUsd || 0).toFixed(2) }}</span></div>
      </div>
      
      <button class="btn-primario" @click="guardarSimulacion" :disabled="api.cargandoDolar">
        Guardar en Historial
      </button>
    </div>
  </div>
</template>

<style scoped>
/* Copiar y pegar todo el CSS anterior del CalculadoraForm.vue (el que tiene el grid 2x2).
   Añadimos solo estas dos reglas para emprolijar el desglose de aduanas: */
.panel-calculadora { display: grid; grid-template-columns: minmax(0, 1.2fr) minmax(0, 1fr); gap: 2rem; background: white; padding: 2rem; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); margin-bottom: 2rem; }
.formulario { display: flex; flex-direction: column; gap: 1.2rem; }
.grupo-input { display: flex; flex-direction: column; gap: 5px; }
.grupo-input label { font-weight: 600; color: #444; font-size: 0.9rem; }
.texto-subtotal { color: #3483fa; font-size: 0.85rem; margin-top: 4px; }
.input-texto, .input-select { width: 100%; box-sizing: border-box; padding: 10px; border: 1px solid #ccc; border-radius: 6px; font-size: 1rem; font-family: inherit; }
.input-select { cursor: pointer; background-color: #f8f9fa; }
.input-texto:focus, .input-select:focus { outline: none; border-color: #3483fa; box-shadow: 0 0 0 2px rgba(52,131,250,0.2); }
.info-producto { color: #666; font-size: 0.85rem; padding-top: 4px; }
.caja-logistica { background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 6px; padding: 1rem; display: flex; flex-direction: column; gap: 10px; }
.logistica-header { font-weight: bold; color: #2d3277; font-size: 0.9rem; border-bottom: 1px solid #e2e8f0; padding-bottom: 5px; margin-bottom: 5px; }
.caja-logistica.aduanas { background-color: #fcf4e3; border-color: #ffeeba; }
.fila-medidas { display: grid; grid-template-columns: repeat(2, 1fr); gap: 12px; }
.grupo-input-pequeno { display: flex; flex-direction: column; gap: 4px; }
.grupo-input-pequeno label { font-size: 0.8rem; color: #555; font-weight: 600; }
.comparativa-pesos { display: flex; gap: 1rem; font-size: 0.85rem; color: #555; }
.peso-item { display: flex; flex-direction: column; background: white; padding: 6px 10px; border-radius: 4px; border: 1px solid #e2e8f0; flex: 1; }
.resumen-flete { display: flex; justify-content: space-between; align-items: center; margin-top: 10px; background-color: #e6f0ff; padding: 8px 10px; border-radius: 4px; font-size: 0.9rem; }
.texto-destaque { color: #3483fa; font-size: 1rem; }
.resultado-panel { background-color: #f8f9fa; padding: 1.5rem; border-radius: 8px; border: 1px solid #eee; display: flex; flex-direction: column; height: 100%; box-sizing: border-box; }
.resultado-panel h3 { margin: 0 0 5px 0; color: #333; }
.monto-destacado { font-size: 2.5rem; font-weight: bold; color: #2d3277; margin-bottom: 5px; }
.info-tc { display: flex; flex-direction: column; gap: 6px; margin-bottom: 1.5rem; background-color: white; padding: 10px; border-radius: 6px; border: 1px solid #e2e8f0; }
.detalle-unidades { color: #666; font-size: 0.9rem; }
.tc-contenedor { color: #444; font-size: 0.9rem; display: flex; align-items: center; gap: 6px; }
.badge-api { background-color: #e6f0ff; color: #3483fa; padding: 2px 6px; border-radius: 4px; font-size: 0.7rem; font-weight: bold; margin-left: 4px; }
.badge-cargando { background-color: #fff159; color: #333; padding: 2px 6px; border-radius: 4px; font-size: 0.7rem; font-weight: bold; }
.desglose { margin-bottom: auto; display: flex; flex-direction: column; gap: 8px; }
.item-desglose { display: flex; justify-content: space-between; border-bottom: 1px solid #ddd; padding-bottom: 8px; color: #444; font-size: 0.95rem; }
.sub-item { border-bottom: none; padding-bottom: 0; margin-top: -4px; color: #888; }
.item-desglose.destacado { font-weight: bold; color: #333; border-bottom: none; padding-top: 8px; }
.btn-primario { background-color: #3483fa; color: white; border: none; padding: 12px; border-radius: 6px; font-weight: 600; cursor: pointer; transition: background 0.2s; margin-top: 1.5rem; width: 100%; }
.btn-primario:hover:not(:disabled) { background-color: #2968c8; }
.btn-primario:disabled { background-color: #a0c4ff; cursor: not-allowed; }
</style>