<script setup>
defineProps({
  historial: Array,
  eliminarFila: Function
});
</script>

<template>
  <div class="panel-historial" v-if="historial && historial.length > 0">
    <h3>Historial de Cálculos</h3>
    <div class="tabla-contenedor">
      <table class="tabla-excel">
        <thead>
          <tr>
            <th>Fecha</th>
            <th>Referencia</th>
            <th class="texto-derecha">Cant.</th>
            <th class="texto-derecha">FOB Total</th>
            <th class="texto-derecha">Flete+Aduana</th>
            <th class="texto-derecha">Total USD</th>
            <th class="texto-derecha">Costo Unit. ARS</th>
            <th class="texto-centro">Acción</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="fila in historial" :key="fila.id">
            <td>{{ fila.fecha }}</td>
            <td class="celda-destacada">{{ fila.referencia }}</td>
            <td class="texto-derecha">{{ fila.cantidad }}</td>
            <td class="texto-derecha">U$S {{ fila.fobTotal.toFixed(2) }}</td>
            <td class="texto-derecha">U$S {{ fila.fleteAduana.toFixed(2) }}</td>
            <td class="texto-derecha font-bold">U$S {{ fila.totalUsd.toFixed(2) }}</td>
            <td class="texto-derecha costo-ars">${{ fila.costoUnitarioArs.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}</td>
            <td class="texto-centro">
              <button class="btn-eliminar" @click="eliminarFila(fila.id)">✕</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.panel-historial { background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); animation: fadeIn 0.3s ease-in; }
.panel-historial h3 { margin: 0 0 1rem 0; color: #333; border-bottom: 2px solid #3483fa; padding-bottom: 8px; display: inline-block; }
.tabla-contenedor { overflow-x: auto; }
.tabla-excel { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
.tabla-excel th { background-color: #f1f3f5; color: #495057; padding: 12px 10px; font-weight: 600; border: 1px solid #dee2e6; }
.tabla-excel td { padding: 10px; border: 1px solid #dee2e6; color: #333; vertical-align: middle; }
.tabla-excel tr:hover td { background-color: #f8f9fa; }
.texto-derecha { text-align: right; }
.texto-centro { text-align: center; }
.font-bold { font-weight: bold; }
.celda-destacada { font-weight: 600; color: #2d3277; }
.costo-ars { font-weight: bold; color: #00a650; }
.btn-eliminar { background: none; border: none; color: #dc3545; font-weight: bold; cursor: pointer; padding: 4px 8px; border-radius: 4px; }
.btn-eliminar:hover { background-color: #ffeeba; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(-10px); } to { opacity: 1; transform: translateY(0); } }
</style>