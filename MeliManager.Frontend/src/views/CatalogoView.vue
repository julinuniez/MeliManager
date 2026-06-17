<script setup>
import { ref } from 'vue';
import axios from 'axios';
import Swal from 'sweetalert2';
import { useCatalogo } from '../composables/useCatalogo';
import ProductoCard from '../components/catalogo/ProductoCard.vue';
import ProductoModal from '../components/catalogo/ProductoModal.vue';

const { productos, cargando, sincronizarConMl } = useCatalogo();

const modalVisible = ref(false);
const productoSeleccionado = ref(null);

// Control visual del panel de conexión y el link
const mostrarCredenciales = ref(false);
const linkGoogle = ref('');

const abrirModal = (producto) => {
  productoSeleccionado.value = producto;
  modalVisible.value = true;
};

const cerrarModal = () => {
  modalVisible.value = false;
  setTimeout(() => { productoSeleccionado.value = null; }, 200);
};

// Función que extrae el código del link de Google y se lo manda a C#
const procesarLinkGoogle = async () => {
  try {
    if (!linkGoogle.value.includes('code=')) {
      Swal.fire('Error', 'El link no parece válido. Asegurate de copiar la URL completa de Google que contiene el "code=".', 'error');
      return;
    }

    // Extraemos el código limpio de la URL de Google
    const urlObjeto = new URL(linkGoogle.value);
    const codigoExtraido = urlObjeto.searchParams.get('code');

    Swal.fire({ 
      title: 'Vinculando...', 
      text: 'Hablando con Mercado Libre', 
      allowOutsideClick: false,
      didOpen: () => Swal.showLoading() 
    });

    // Se lo mandamos a tu controlador MeliAuthController (Verificá que el puerto 7200 sea el de tu C#)
    await axios.post('https://localhost:7200/api/meliauth/intercambiar-codigo', { code: codigoExtraido });

    Swal.fire('¡Conectado!', 'Ya tenés la llave maestra. Ahora podés sincronizar tu inventario.', 'success');
    linkGoogle.value = ''; // Limpiamos el casillero

  } catch (error) {
    console.error("Error al conectar:", error);
    Swal.fire('Error', 'No se pudo validar el código. Revisá que tu servidor C# esté corriendo.', 'error');
  }
};

const ejecutarSincronizacion = () => {
  sincronizarConMl();
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
          🔄 Cuenta Mercado Libre
        </button>
        <button class="btn-primario">+ Nuevo Producto</button>
      </div>
    </div>

    <div class="panel-credenciales" v-if="mostrarCredenciales">
      <h3>Conexión Oficial con Mercado Libre</h3>
      
      <div class="fila-credenciales" style="flex-direction: column; align-items: flex-start;">
        
        <div style="margin-bottom: 1.5rem;">
          <p class="descripcion-auth"><strong>Paso 1:</strong> Hacé clic en este botón. Te va a pedir iniciar sesión y luego te llevará a una página de Google.</p>
          <a 
            href="https://auth.mercadolibre.com.ar/authorization?response_type=code&client_id=5839421422105357&redirect_uri=https://www.google.com" 
            target="_blank"
            class="btn-login-meli"
          >
            🔐 1. Iniciar Sesión en Mercado Libre
          </a>
        </div>

        <div style="width: 100%; margin-bottom: 1.5rem;">
          <p class="descripcion-auth"><strong>Paso 2:</strong> Cuando llegues a Google, copiá la dirección completa de arriba (la que empieza con https y tiene el código) y pegala acá abajo:</p>
          <div style="display: flex; gap: 10px; margin-top: 8px;">
            <input 
              type="text" 
              v-model="linkGoogle" 
              placeholder="https://www.google.com/?code=TG-..." 
              style="flex: 1; padding: 12px; border-radius: 6px; border: 1px solid #ccc; font-size: 0.95rem;" 
            />
            <button class="btn-validar" @click="procesarLinkGoogle">Validar Código</button>
          </div>
        </div>

        <div>
          <p class="descripcion-auth"><strong>Paso 3:</strong> Una vez validado, ya podés descargar tus productos pausados y activos.</p>
          <button class="btn-ejecutar" @click="ejecutarSincronizacion">
            ⚡ 3. Sincronizar Inventario Local
          </button>
        </div>

      </div>
    </div>

    <div v-if="cargando" class="estado-carga">
      <div class="spinner"></div>
      <p>Consultando base de datos local...</p>
    </div>

    <div v-else-if="productos.length === 0" class="sin-productos">
      <p>Tu base de datos local está vacía. Desplegá el botón "Cuenta Mercado Libre" para iniciar sesión e importar tus publicaciones reales por primera vez.</p>
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

/* ESTILOS DEL PANEL DE CONEXIÓN OAUTH */
.panel-credenciales { background: #f4f7fe; border: 1px solid #ccd7f7; padding: 1.5rem; border-radius: 8px; margin-bottom: 2rem; }
.panel-credenciales h3 { margin: 0 0 1rem 0; font-size: 1.1rem; color: #1e3a8a; }
.descripcion-auth { margin: 0; font-size: 0.95rem; color: #4b5563; line-height: 1.5; }
.fila-credenciales { display: flex; gap: 1.5rem; }

.btn-login-meli { text-decoration: none; display: inline-flex; align-items: center; background-color: #fff159; color: #333; padding: 11px 20px; border-radius: 6px; font-weight: 600; font-size: 0.95rem; border: 1px solid #e6d830; transition: background 0.2s; cursor: pointer; margin-top: 8px;}
.btn-login-meli:hover { background-color: #ebd913; }

.btn-validar { background: #00a650; color: white; border: none; padding: 0 20px; border-radius: 6px; font-weight: 600; font-size: 0.95rem; cursor: pointer; transition: background 0.2s; }
.btn-validar:hover { background: #008741; }

.btn-ejecutar { background: #3483fa; color: white; border: none; padding: 11px 20px; border-radius: 6px; font-weight: 600; font-size: 0.95rem; cursor: pointer; transition: background 0.2s; margin-top: 8px;}
.btn-ejecutar:hover { background: #1e6ee2; }

.grilla-productos { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 1.5rem; }
.estado-carga { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 5rem 0; color: #666; }
.sin-productos { text-align: center; padding: 4rem 2rem; background: white; border-radius: 8px; color: #777; border: 1px dashed #ccc; font-size: 1.05rem; line-height: 1.6; }
.spinner { border: 4px solid rgba(0, 0, 0, 0.1); border-left-color: #3483fa; border-radius: 50%; width: 40px; height: 40px; animation: girar 1s linear infinite; margin-bottom: 1rem; }
@keyframes girar { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
</style>