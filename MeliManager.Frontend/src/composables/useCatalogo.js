import { ref, onMounted } from 'vue';

export function useCatalogo() {
  // Variables reactivas
  const productos = ref([]);
  const cargando = ref(true);

  // Función para obtener el inventario (futura llamada a C#)
  const cargarInventario = async () => {
    cargando.value = true;
    
    // Simulamos el tiempo de respuesta del servidor (medio segundo)
    setTimeout(() => {
      productos.value = [
        {
          sku: 'ALARMA-MOTO-01',
          nombre: 'Alarma Inalámbrica para Moto',
          costoUnitario: 12542.86,
          stock: 30, // Tu lote de prueba
          imagenUrl: 'https://http2.mlstatic.com/D_NQ_NP_2X_735492-MLA46610738092_072021-F.webp'
        },
        {
          sku: 'CANDADO-GPS-02',
          nombre: 'Candado U-Lock Blindado',
          costoUnitario: 21500.00,
          stock: 15,
          imagenUrl: 'https://http2.mlstatic.com/D_NQ_NP_2X_811059-MLA46610738094_072021-F.webp' // Imagen ilustrativa
        }
      ];
      cargando.value = false;
    }, 500);
  };

  // Se ejecuta automáticamente al abrir la pantalla
  onMounted(() => {
    cargarInventario();
  });

  const abrirNuevoProducto = () => {
    alert("Próximamente: Modal para agregar nuevo producto al catálogo.");
  };

  return {
    productos,
    cargando,
    abrirNuevoProducto
  };
}