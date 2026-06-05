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
  
  // ESTAS 4 LÍNEAS FALTABAN PARA QUE MUESTRE LA CUENTA
  comisionMlArs: Number,
  iibbArs: Number,
  gananciaNetaArs: Number,
  margenPorcentaje: Number,
  
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
      </div>

      <div class="caja-logistica">
        <div class="logistica-header">Dimensiones y Peso Unitario</div>
        <div class="fila-medidas">
          <div class="grupo-input-pequeno">
            <label>Peso (kg)</label>
            <input type="number" v-model.number="form.pesoKg" class="input-texto sin-flechas" step="0.01" :readonly="form.skuSeleccionado !== 'CUSTOM'" :class="{'solo-lectura': form.skuSeleccionado !== 'CUSTOM'}" />
          </div>
          <div class="grupo-input-pequeno">
            <label>Largo (cm)</label>
            <input type="number" v-model.number="form.largo" class="input-texto sin-flechas" :readonly="form.skuSeleccionado !== 'CUSTOM'" :class="{'solo-lectura': form.skuSeleccionado !== 'CUSTOM'}" />
          </div>
          <div class="grupo-input-pequeno">
            <label>Ancho (cm)</label>
            <input type="number" v-model.number="form.ancho" class="input-texto sin-flechas" :readonly="form.skuSeleccionado !== 'CUSTOM'" :class="{'solo-lectura': form.skuSeleccionado !== 'CUSTOM'}" />
          </div>
          <div class="grupo-input-pequeno">
            <label>Alto (cm)</label>
            <input type="number" v-model.number="form.alto" class="input-texto sin-flechas" :readonly="form.skuSeleccionado !== 'CUSTOM'" :class="{'solo-lectura': form.skuSeleccionado !== 'CUSTOM'}" />
          </div>
        </div>
      </div>

      <div class="fila-medidas">
        <div class="grupo-input">
          <label>Cantidad (Unidades)</label>
          <input type="number" v-model.number="form.cantidad" class="input-texto sin-flechas" min="1" />
        </div>
        <div class="grupo-input">
          <label>Precio FOB Unitario</label>
          <input type="number" v-model.number="form.fobUnitario" class="input-texto sin-flechas" step="0.01" />
        </div>
      </div>

      <div class="caja-logistica">
        <div class="logistica-header">Costos de Envío (Logística)</div>
        <div class="fila-medidas">
          <div class="grupo-input-pequeno">
            <label>Servicio Fijo (USD)</label>
            <input type="number" v-model.number="form.costoServicio" class="input-texto sin-flechas" step="1" />
          </div>
          <div class="grupo-input-pequeno">
            <label>Tasa x Kg (USD)</label>
            <input type="number" v-model.number="form.costoPorKg" class="input-texto sin-flechas" step="1" />
          </div>
        </div>
      </div>

      <div class="caja-logistica aduanas">
        <div class="logistica-header">Impuestos de Nacionalización (Aduana)</div>
        <div class="fila-medidas">
          <div class="grupo-input-pequeno">
            <label>Derechos (%)</label>
            <input type="number" v-model.number="form.derechosPorcentaje" class="input-texto sin-flechas" step="0.1" :readonly="form.skuSeleccionado !== 'CUSTOM'" :class="{'solo-lectura': form.skuSeleccionado !== 'CUSTOM'}" />
          </div>
          <div class="grupo-input-pequeno">
            <label>IVA/Perc (%)</label>
            <input type="number" v-model.number="form.ivaPorcentaje" class="input-texto sin-flechas" step="0.1" />
          </div>
        </div>
      </div>

      <div class="caja-logistica mercadolibre">
        <div class="logistica-header ml-header">Proyección Mercado Libre</div>
        <div class="fila-medidas">
          <div class="grupo-input">
            <label>Precio de Venta ($)</label>
            <input type="number" v-model.number="form.precioVentaArs" class="input-texto sin-flechas" step="100" />
          </div>
          <div class="grupo-input">
            <label>Costo Envío ML ($)</label>
            <input type="number" v-model.number="form.envioMlArs" class="input-texto sin-flechas" step="100" />
          </div>
        </div>
        <div class="fila-medidas" style="margin-top: 10px;">
          <div class="grupo-input">
            <label>Exposición</label>
            <select v-model.number="form.tipoPublicacion" class="input-select">
              <option :value="15">Clásica (~15%)</option>
              <option :value="28">Premium (~28%)</option>
            </select>
          </div>
          <div class="grupo-input">
            <label>IIBB (%)</label>
            <input type="number" v-model.number="form.iibbPorcentaje" class="input-texto sin-flechas" step="0.1" />
          </div>
        </div>
      </div>
    </div>

    <div class="resultado-panel">
      <h3>Costo Landed Unitario</h3>
      <div class="monto-destacado">
        <span v-if="api.cargandoDolar">...</span>
        <span v-else>${{ (costoUnitarioArs || 0).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}</span>
      </div>
      <div class="info-tc">
        <div class="tc-contenedor">
          <span>TC Aplicado: </span>
          <span v-if="api.cargandoDolar" class="badge-cargando">Actualizando...</span>
          <span v-else><strong>${{ api.tipoCambio }}</strong></span>
        </div>
      </div>

      <div class="panel-rentabilidad" :class="gananciaNetaArs > 0 ? 'rentable' : 'perdida'">
        <h3>Rentabilidad por Unidad</h3>
        <div class="desglose-mini">
          <div class="item-mini"><span>Precio Venta:</span><span>${{ (form.precioVentaArs || 0).toLocaleString('es-AR') }}</span></div>
          <div class="item-mini texto-rojo"><span>Comisión ML:</span><span>-${{ (comisionMlArs || 0).toLocaleString('es-AR', {maximumFractionDigits: 0}) }}</span></div>
          <div class="item-mini texto-rojo"><span>IIBB ({{form.iibbPorcentaje}}%):</span><span>-${{ (iibbArs || 0).toLocaleString('es-AR', {maximumFractionDigits: 0}) }}</span></div>
          <div class="item-mini texto-rojo"><span>Envío ML:</span><span>-${{ (form.envioMlArs || 0).toLocaleString('es-AR', {maximumFractionDigits: 0}) }}</span></div>
          <div class="item-mini texto-rojo"><span>Costo Producto:</span><span>-${{ (costoUnitarioArs || 0).toLocaleString('es-AR', {maximumFractionDigits: 0}) }}</span></div>
        </div>
        <hr class="divisor-rentabilidad">
        <div class="resultado-final">
          <div class="metrica">
            <span class="etiqueta">Ganancia Neta</span>
            <span class="monto">${{ (gananciaNetaArs || 0).toLocaleString('es-AR', {maximumFractionDigits: 0}) }}</span>
          </div>
          <div class="metrica margen-caja">
            <span class="etiqueta">Margen</span>
            <span class="monto">{{ (margenPorcentaje || 0).toFixed(1) }}%</span>
          </div>
        </div>
      </div>
      
      <button class="btn-primario" @click="guardarSimulacion" :disabled="api.cargandoDolar">
        Guardar en Historial
      </button>
    </div>
  </div>
</template>

<style scoped>
/* ACÁ VAN EXACTAMENTE LOS MISMOS ESTILOS QUE TE PASÉ EN EL MENSAJE ANTERIOR */
.panel-calculadora { display: grid; grid-template-columns: minmax(0, 1.2fr) minmax(0, 1fr); gap: 2rem; background: white; padding: 2rem; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); margin-bottom: 2rem; }
.formulario { display: flex; flex-direction: column; gap: 1.2rem; }
.grupo-input { display: flex; flex-direction: column; gap: 5px; }
.grupo-input label { font-weight: 600; color: #444; font-size: 0.9rem; }
.input-texto, .input-select { width: 100%; box-sizing: border-box; padding: 10px; border: 1px solid #ccc; border-radius: 6px; font-size: 1rem; font-family: inherit; }
.input-select { cursor: pointer; background-color: #f8f9fa; }
.input-texto:focus, .input-select:focus { outline: none; border-color: #3483fa; box-shadow: 0 0 0 2px rgba(52,131,250,0.2); }

input[type="number"].sin-flechas::-webkit-outer-spin-button,
input[type="number"].sin-flechas::-webkit-inner-spin-button { -webkit-appearance: none; margin: 0; }
input[type="number"].sin-flechas { -moz-appearance: textfield; }

.solo-lectura { background-color: #f1f3f5 !important; color: #999 !important; cursor: not-allowed; border-color: #e9ecef !important; }
.solo-lectura:focus { outline: none !important; box-shadow: none !important; }

.caja-logistica { background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 6px; padding: 1rem; display: flex; flex-direction: column; gap: 10px; }
.logistica-header { font-weight: bold; color: #2d3277; font-size: 0.9rem; border-bottom: 1px solid #e2e8f0; padding-bottom: 5px; margin-bottom: 5px; }
.caja-logistica.aduanas { background-color: #fcf4e3; border-color: #ffeeba; }
.caja-logistica.mercadolibre { background-color: #fdfcee; border-color: #fff159; }
.ml-header { color: #877100; }

.fila-medidas { display: grid; grid-template-columns: repeat(2, 1fr); gap: 12px; }
.grupo-input-pequeno { display: flex; flex-direction: column; gap: 4px; }
.grupo-input-pequeno label { font-size: 0.8rem; color: #555; font-weight: 600; }

.resultado-panel { background-color: #f8f9fa; padding: 1.5rem; border-radius: 8px; border: 1px solid #eee; display: flex; flex-direction: column; height: 100%; box-sizing: border-box; }
.resultado-panel h3 { margin: 0 0 5px 0; color: #333; font-size: 1.1rem; }
.monto-destacado { font-size: 2.5rem; font-weight: bold; color: #2d3277; margin-bottom: 5px; }
.info-tc { display: flex; flex-direction: column; gap: 6px; margin-bottom: 1.5rem; background-color: white; padding: 10px; border-radius: 6px; border: 1px solid #e2e8f0; }
.tc-contenedor { color: #444; font-size: 0.9rem; display: flex; align-items: center; gap: 6px; }
.badge-cargando { background-color: #fff159; color: #333; padding: 2px 6px; border-radius: 4px; font-size: 0.7rem; font-weight: bold; }

.panel-rentabilidad { background: white; border-radius: 8px; padding: 1.2rem; margin-bottom: auto; border: 2px solid transparent; transition: border-color 0.3s; }
.panel-rentabilidad.rentable { border-color: #00a650; box-shadow: 0 4px 10px rgba(0, 166, 80, 0.1); }
.panel-rentabilidad.perdida { border-color: #dc3545; box-shadow: 0 4px 10px rgba(220, 53, 69, 0.1); }
.desglose-mini { display: flex; flex-direction: column; gap: 6px; margin-top: 10px; }
.item-mini { display: flex; justify-content: space-between; font-size: 0.95rem; color: #555; }
.texto-rojo { color: #dc3545 !important; }
.divisor-rentabilidad { border: 0; border-top: 1px dashed #ccc; margin: 12px 0; }
.resultado-final { display: flex; justify-content: space-between; align-items: center; }
.metrica { display: flex; flex-direction: column; }
.metrica .etiqueta { font-size: 0.85rem; color: #666; font-weight: bold; text-transform: uppercase; }
.metrica .monto { font-size: 1.4rem; font-weight: bold; }
.rentable .monto { color: #00a650; }
.perdida .monto { color: #dc3545; }
.margen-caja { background: rgba(0,0,0,0.05); padding: 5px 10px; border-radius: 6px; align-items: flex-end; }

.btn-primario { background-color: #3483fa; color: white; border: none; padding: 12px; border-radius: 6px; font-weight: 600; cursor: pointer; transition: background 0.2s; margin-top: 1.5rem; width: 100%; font-size: 1.05rem; }
.btn-primario:hover:not(:disabled) { background-color: #2968c8; }
.btn-primario:disabled { background-color: #a0c4ff; cursor: not-allowed; }
</style>