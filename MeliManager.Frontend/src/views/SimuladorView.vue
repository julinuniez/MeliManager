<script setup>
import { useSimulador } from '../composables/useSimulador';
import CalculadoraForm from '../components/simulador/CalculadoraForm.vue';
import HistorialTabla from '../components/simulador/HistorialTabla.vue';

const { 
  productosBase, form, api, historial,
  pesoFisicoTotal, pesoVolumetricoTotal, pesoCobradoTotal, fleteCalculado,
  fobTotal, costoDerechos, costoIva, totalImpuestos, totalUsd, costoUnitarioArs, 
  comisionMlArs, iibbArs, gananciaNetaArs, margenPorcentaje,
  guardarSimulacion, eliminarFila
} = useSimulador();
</script>

<template>
  <div class="vista-simulador">
    <div class="encabezado">
      <h2>Simulador de Costos y Rentabilidad</h2>
      <p>Calculá el Landed Cost y proyectá tu ganancia neta vendiendo en Mercado Libre.</p>
    </div>

    <CalculadoraForm 
      :productosBase="productosBase"
      :form="form"
      :api="api"
      :pesoFisicoTotal="pesoFisicoTotal"
      :pesoVolumetricoTotal="pesoVolumetricoTotal"
      :pesoCobradoTotal="pesoCobradoTotal"
      :fleteCalculado="fleteCalculado"
      :fobTotal="fobTotal"
      :costoDerechos="costoDerechos"
      :costoIva="costoIva"
      :totalImpuestos="totalImpuestos"
      :totalUsd="totalUsd"
      :costoUnitarioArs="costoUnitarioArs"
      :comisionMlArs="comisionMlArs"
      :iibbArs="iibbArs"
      :gananciaNetaArs="gananciaNetaArs"
      :margenPorcentaje="margenPorcentaje"
      :guardarSimulacion="guardarSimulacion"
    />

    <HistorialTabla 
      :historial="historial"
      :eliminarFila="eliminarFila"
    />
  </div>
</template>

<style scoped>
.vista-simulador { max-width: 1100px; margin: 0 auto; padding-bottom: 3rem; }
.encabezado { margin-bottom: 2rem; }
.encabezado h2 { margin: 0; color: #333; }
.encabezado p { color: #666; margin-top: 5px; }
</style>