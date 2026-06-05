<script setup>
import { ref } from 'vue';
import { useCatalogo } from '../composables/useCatalogo';
import ProductoCard from '../components/catalogo/ProductoCard.vue';
import ProductoModal from '../components/catalogo/ProductoModal.vue'; // <-- Importamos el modal

const { productos, cargando } = useCatalogo();

// Lógica de control del Modal
const modalVisible = ref(false);
const productoSeleccionado = ref(null);

const abrirModal = (producto) => {
  productoSeleccionado.value = producto;
  modalVisible.value = true;
};

const cerrarModal = () => {
  modalVisible.value = false;
  setTimeout(() => { productoSeleccionado.value = null; }, 200); // Limpiar después de cerrar
};

const guardarCambios = (datosActualizados) => {
  // En el futuro, acá mandás el POST a C#.
  // Por ahora, actualizamos la vista localmente:
  const index = productos.value.findIndex(p => p.id === datosActualizados.id);
  if (index !== -1) {
    productos.value[index] = datosActualizados;
  }
  cerrarModal();
};
</script>

<template>
  <div class="vista-catalogo">
    
    <div class="encabezado">
      <div class="textos">
        <h2>Catálogo de Productos</h2>
        <p>Gestioná tus publicaciones, stock y precios de venta.</p>
      </div>
      <div class="controles">
        <input type="text" class="buscador" placeholder="Buscar por SKU o Nombre..." />
        <button class="btn-primario">+ Nuevo Producto</button>
      </div>
    </div>

    <div v-if="cargando" class="estado-carga">
      <div class="spinner"></div>
      <p>Sincronizando inventario...</p>
    </div>

    <div v-else class="grilla-productos">
      <ProductoCard 
        v-for="item in productos" 
        :key="item.id" 
        :producto="item"
        @editar="abrirModal" 
      />
    </div>

    <ProductoModal 
      :visible="modalVisible" 
      :producto="productoSeleccionado" 
      @cerrar="cerrarModal" 
      @guardar="guardarCambios" 
    />

  </div>
</template>

<style scoped>
/* Mantener tu CSS actual de la vista principal */
.vista-catalogo { max-width: 1200px; margin: 0 auto; padding-bottom: 3rem; }
.encabezado { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; background: white; padding: 1.5rem 2rem; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.05); flex-wrap: wrap; gap: 1rem; }
.encabezado h2 { margin: 0; color: #333; }
.encabezado p { margin: 5px 0 0 0; color: #666; font-size: 0.95rem; }
.controles { display: flex; gap: 1rem; flex: 1; justify-content: flex-end; }
.buscador { padding: 10px 15px; border: 1px solid #ccc; border-radius: 6px; font-size: 0.95rem; min-width: 250px; outline: none; transition: border-color 0.2s; }
.buscador:focus { border-color: #3483fa; }
.btn-primario { background-color: #3483fa; color: white; border: none; padding: 10px 20px; border-radius: 6px; font-weight: 600; cursor: pointer; transition: background 0.2s; white-space: nowrap; }
.btn-primario:hover { background-color: #2968c8; }
.grilla-productos { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 1.5rem; }
.estado-carga { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 5rem 0; color: #666; }
.spinner { border: 4px solid rgba(0, 0, 0, 0.1); border-left-color: #3483fa; border-radius: 50%; width: 40px; height: 40px; animation: girar 1s linear infinite; margin-bottom: 1rem; }
@keyframes girar { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
</style>