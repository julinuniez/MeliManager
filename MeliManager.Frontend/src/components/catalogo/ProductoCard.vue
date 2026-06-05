<script setup>
defineProps({
  producto: {
    type: Object,
    required: true
  }
});
</script>

<template>
  <div class="tarjeta-producto">
    <div class="imagen-contenedor">
      <img :src="producto.imagenUrl" :alt="producto.nombre" class="imagen" />
      <span class="etiqueta-stock">Stock: {{ producto.stock }} un.</span>
    </div>
    
    <div class="info-contenedor">
      <p class="sku">{{ producto.sku }}</p>
      <h3 class="titulo">{{ producto.nombre }}</h3>
      
      <div class="metricas">
        <div class="dato">
          <span class="etiqueta">Landed Cost</span>
          <span class="valor">${{ producto.costoUnitario.toLocaleString('es-AR', { minimumFractionDigits: 2 }) }}</span>
        </div>
      </div>
      
      <div class="acciones">
        <button class="btn-secundario">Editar</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.tarjeta-producto {
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
  transition: box-shadow 0.2s, transform 0.2s;
  display: flex;
  flex-direction: column;
}

.tarjeta-producto:hover {
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
  transform: translateY(-2px);
}

.imagen-contenedor {
  width: 100%;
  height: 220px;
  background-color: #f5f5f5;
  position: relative;
}

.imagen { width: 100%; height: 100%; object-fit: cover; }

.etiqueta-stock {
  position: absolute;
  top: 10px;
  right: 10px;
  background-color: #fff159; /* Amarillo ML */
  color: #333;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 0.8rem;
  font-weight: bold;
}

.info-contenedor { padding: 1.2rem; display: flex; flex-direction: column; flex: 1; }
.sku { color: #999; font-size: 0.8rem; margin: 0 0 4px 0; }
.titulo { margin: 0 0 1rem 0; font-size: 1.1rem; color: #333; font-weight: 600; line-height: 1.3; }

.metricas { margin-top: auto; border-top: 1px solid #eee; padding-top: 1rem; margin-bottom: 1rem; }
.dato { display: flex; justify-content: space-between; align-items: center; }
.dato .etiqueta { color: #666; font-size: 0.9rem; }
.dato .valor { font-weight: bold; color: #2d3277; }

.acciones { display: flex; gap: 10px; }
.btn-secundario {
  width: 100%;
  background-color: rgba(52, 131, 250, 0.1);
  color: #3483fa;
  border: none;
  padding: 8px;
  border-radius: 4px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
}
.btn-secundario:hover { background-color: rgba(52, 131, 250, 0.2); }
</style>