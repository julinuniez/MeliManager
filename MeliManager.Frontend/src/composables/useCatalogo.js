import { ref, onMounted } from 'vue';

export function useCatalogo() {
  const productos = ref([]);
  const cargando = ref(true);

  const cargarInventario = async () => {
    cargando.value = true;
    
    // Simulamos la carga desde tu futura base de datos (C#)
    setTimeout(() => {
      productos.value = [
        {
          id: 1,
          sku: 'ALARMA-MOTO-01',
          nombre: 'Alarma Inalámbrica con Sensor de Movimiento',
          precioVenta: 36000,
          stock: 30,
          estado: 'publicado',
          // Espacio para tus renders de alta calidad
          imagenUrl: 'https://http2.mlstatic.com/D_NQ_NP_2X_735492-MLA46610738092_072021-F.webp'
        },
        {
          id: 2,
          sku: 'CANDADO-GPS-02',
          nombre: 'Candado U-Lock Blindado Acero Templado',
          precioVenta: 55000,
          stock: 0,
          estado: 'pausado',
          imagenUrl: 'https://http2.mlstatic.com/D_NQ_NP_2X_811059-MLA46610738094_072021-F.webp'
        },
        {
          id: 3,
          sku: 'LINGA-ACERO-03',
          nombre: 'Linga de Acero Trenzado 1.5m Alta Seguridad',
          precioVenta: 18500,
          stock: 15,
          estado: 'publicado',
          imagenUrl: 'https://http2.mlstatic.com/D_NQ_NP_2X_770177-MLA48113702581_112021-F.webp'
        }
      ];
      cargando.value = false;
    }, 600);
  };

  onMounted(() => {
    cargarInventario();
  });

  return {
    productos,
    cargando
  };
}