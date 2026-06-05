<script setup>
import { useSimulador } from '../composables/useSimulador';
import CalculadoraForm from '../components/simulador/CalculadoraForm.vue';
import HistorialTabla from '../components/simulador/HistorialTabla.vue';

// 1. Agregamos las variables de impuestos y sacamos productoActivo
const { 
  productosBase, form, api, historial, 
  pesoFisicoTotal, pesoVolumetricoTotal, pesoCobradoTotal, fleteCalculado,
  fobTotal, costoDerechos, costoIva, totalImpuestos, totalUsd, costoUnitarioArs, 
  guardarSimulacion, eliminarFila 
} = useSimulador();
</script>

<template>
  <div class="vista-simulador">
    <div class="encabezado">
      <h2>Simulador de Landed Cost</h2>
      <p>Calculá el costo real evaluando el Peso Volumétrico vs Físico e Impuestos.</p>
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