import { ref, onMounted } from 'vue';
import { apiClient } from '../api/apiClient';
import Swal from 'sweetalert2';

export function useCatalogo() {
  const productos = ref([]);
  const cargando = ref(true);

  const cargarInventario = async () => {
    try {
      cargando.value = true;
      const data = await apiClient.get('/productos'); 
      productos.value = data;
    } catch (error) {
      console.error("Error de conexión con la API:", error);
    } finally {
      cargando.value = false;
    }
  };

  const sincronizarConMl = async () => {
    try {
      Swal.fire({
        title: 'Sincronizando tus productos reales...',
        allowOutsideClick: false,
        didOpen: () => { Swal.showLoading(); }
      });

      // Llamamos directo sin enviar token ni ID
      const resultado = await apiClient.post('/productos/sincronizar-mercadolibre');

      await cargarInventario();

      Swal.fire({
        title: '¡Sincronización Exitosa!',
        text: `${resultado.publicacionesActualizadas || 0} actualizadas y ${resultado.nuevosRegistrados || 0} nuevos.`,
        icon: 'success',
        confirmButtonColor: '#3483fa'
      });

    } catch (error) {
      Swal.fire({
        title: 'Error de Sincronización',
        text: `Detalle: ${error.message}`,
        icon: 'error',
        confirmButtonColor: '#dc3545'
      });
    }
  };

  onMounted(() => {
    cargarInventario();
  });

  return {
    productos,
    cargando,
    cargarInventario,
    sincronizarConMl // Exportamos la función para el botón
  };
}