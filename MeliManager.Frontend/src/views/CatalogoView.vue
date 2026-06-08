<script setup>
import { ref } from 'vue';
import { useCatalogo } from '../composables/useCatalogo';
import ProductoCard from '../components/catalogo/ProductoCard.vue';
import ProductoModal from '../components/catalogo/ProductoModal.vue';

const { productos, cargando, sincronizarConMl } = useCatalogo();

const modalVisible = ref(false);
const productoSeleccionado = ref(null);

// Variables para tus credenciales de Mercado Libre
const mostrarCredenciales = ref(false);
const mlToken = ref('');
const mlSellerId = ref('');

const abrirModal = (producto) => {
  productoSeleccionado.value = producto;
  modalVisible.value = true;
};

const cerrarModal = () => {
  modalVisible.value = false;
  setTimeout(() => { productoSeleccionado.value = null; }, 200);
};

const ejecutarSincronizacion = () => {
  if (!mlToken.value || !mlSellerId.value) {
    alert("Por favor, completá tu Access Token y tu Seller ID para continuar.");
    return;
  }
  sincronizarConMl(mlToken.value, mlSellerId.value);
};
</script>

<template>
  <div class="vista-catalogo">
    
    <div class="encabezado">
      <div class="textos">
        <h2>Catálogo de Productos</h2>
        <p>Sincronizá tus publicaciones directamente desde la API oficial de Mercado Libre.</p>
      </div>
      <div class="controles">
        <button class="btn-sincronizar" @click="mostrarCredenciales = !mostrarCredenciales">
          🔄 Sincronizar Cuenta
        </button>
        <button class="btn-primario">+ Nuevo Producto</button>
      </div>
    </div>

    <div class="panel-credenciales" v-if="mostrarCredenciales">
      <h3>Credenciales de Conexión de Mercado Libre</h3>
      <div class="fila-credenciales">
        <div class="campo">
          <label>Seller ID (ID de Usuario)</label>
          <input type="text" v-model="mlSellerId" placeholder="Ej: 123456789" class="input-texto" />
        </div>
        <div class="campo token-campo">
          <label>Access Token (Bearer)</label>
          <input type="password" v-model="mlToken" placeholder="APP_USR-..." class="input-texto" />
        </div>
        <button class="btn-ejecutar" @click="ejecutarSincronizacion">Ejecutar Sincronización</button>
      </div>
      <small class="hint">Podés obtener tus credenciales temporales desde el portal de Mercado Libre Developers.</small>
    </div>

    <div v-if="cargando" class="estado-carga">
      <div class="spinner"></div>
      <p>Consultando base de datos local...</p>
    </div>

    <div v-else-if="productos.length === 0" class="sin-productos">
      <p>Tu base de datos local está vacía. Desplegá el botón "Sincronizar Cuenta" para importar tus publicaciones de Mercado Libre por primera vez.</p>
    </div>

    <div v-else class="grilla-productos">
      <ProductoCard 
        v-for="item in productos" 
        :key="item.sku" 
        :producto="item"
        @editar="abrirModal" 
      />
    </div>

    <ProductoModal :visible="modalVisible" :producto="productoSeleccionado" @cerrar="cerrarModal" />

  </div>
</template>

<style scoped>
.vista-catalogo { max-width: 1200px; margin: 0 auto; padding-bottom: 3rem; }
.encabezado { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; background: white; padding: 1.5rem 2rem; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.05); gap: 1rem; }
.encabezado h2 { margin: 0; color: #333; }
.encabezado p { margin: 5px 0 0 0; color: #666; font-size: 0.95rem; }
.controles { display: flex; gap: 1rem; }

.btn-primario { background-color: #3483fa; color: white; border: none; padding: 10px 20px; border-radius: 6px; font-weight: 600; cursor: pointer; }
.btn-sincronizar { background-color: #f1f3f5; color: #333; border: 1px solid #ccc; padding: 10px 20px; border-radius: 6px; font-weight: 600; cursor: pointer; transition: background 0.2s; }
.btn-sincronizar:hover { background-color: #e6e9ec; }

/* ESTILOS DEL PANEL DE CONEXIÓN */
.panel-credenciales { background: #fffde8; border: 1px solid #f5e0a3; padding: 1.5rem; border-radius: 8px; margin-bottom: 2rem; }
.panel-credenciales h3 { margin: 0 0 1rem 0; font-size: 1rem; color: #7d6608; }
.fila-credenciales { display: flex; gap: 1rem; align-items: flex-end; }
.campo { display: flex; flex-direction: column; gap: 5px; }
.token-campo { flex: 1; }
.campo label { font-size: 0.85rem; font-weight: bold; color: #555; }
.input-texto { padding: 10px; border: 1px solid #ccc; border-radius: 6px; font-size: 0.95rem; box-sizing: border-box; }
.btn-ejecutar { background: #00a650; color: white; border: none; padding: 11px 20px; border-radius: 6px; font-weight: 600; cursor: pointer; height: 41px; }
.btn-ejecutar:hover { background: #008741; }
.hint { color: #7c7247; font-size: 0.8rem; display: block; margin-top: 8px; }

.grilla-productos { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 1.5rem; }
.estado-carga { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 5rem 0; color: #666; }
.sin-productos { text-align: center; padding: 4rem 2rem; background: white; border-radius: 8px; color: #777; border: 1px dashed #ccc; font-size: 1.05rem; line-height: 1.6; }
.spinner { border: 4px solid rgba(0, 0, 0, 0.1); border-left-color: #3483fa; border-radius: 50%; width: 40px; height: 40px; animation: girar 1s linear infinite; margin-bottom: 1rem; }
@keyframes girar { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
</style>