<script setup>
import { useCatalogo } from '../composables/useCatalogo';
import ProductoCard from '../components/catalogo/ProductoCard.vue';

// Traemos la lógica separada
const { productos, cargando, abrirNuevoProducto } = useCatalogo();
</script>

<template>
  <div class="vista-catalogo">
    <div class="encabezado">
      <div>
        <h2>Catálogo de Productos</h2>
        <p class="subtitulo">Gestioná tu inventario y costos base.</p>
      </div>
      <button class="btn-primario" @click="abrirNuevoProducto">
        + Nuevo Producto
      </button>
    </div>

    <div v-if="cargando" class="estado-carga">
      <div class="spinner"></div>
      <p>Cargando inventario...</p>
    </div>

    <div v-else class="grilla-productos">
      <ProductoCard 
        v-for="item in productos" 
        :key="item.sku" 
        :producto="item" 
      />
    </div>
  </div>
</template>

<style scoped>
.vista-catalogo {
  width: 100%;
}

.encabezado {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  background: white;
  padding: 1.5rem 2rem;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
}

.encabezado h2 { margin: 0; color: #333; }
.subtitulo { margin: 5px 0 0 0; color: #666; font-size: 0.95rem; }

.btn-primario {
  background-color: #3483fa;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
}
.btn-primario:hover { background-color: #2968c8; }

.grilla-productos {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 1.5rem;
}

/* Animación de carga simple */
.estado-carga {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4rem 0;
  color: #666;
}

.spinner {
  border: 4px solid rgba(0, 0, 0, 0.1);
  border-left-color: #3483fa;
  border-radius: 50%;
  width: 40px;
  height: 40px;
  animation: girar 1s linear infinite;
  margin-bottom: 1rem;
}

@keyframes girar {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}
</style>