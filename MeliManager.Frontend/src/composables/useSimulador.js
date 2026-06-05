import { ref, reactive, computed, onMounted } from 'vue';

export function useSimulador() {
  const productosBase = [
    { sku: 'ALARMA-MOTO-01', nombre: 'Alarma Inalámbrica', fob: 5.70, pesoKg: 0.1, largo: 10, ancho: 10, alto: 3 },
    { sku: 'CANDADO-GPS-02', nombre: 'Candado U-Lock', fob: 21.50, pesoKg: 1.2, largo: 25, ancho: 15, alto: 6 },
    { sku: 'LINGA-ACERO-03', nombre: 'Linga de Acero 1.5m', fob: 8.00, pesoKg: 0.8, largo: 20, ancho: 20, alto: 8 }
  ];

  const itemsEnPedido = ref([]);

  const form = reactive({
    costoServicio: 150.00,
    costoPorKg: 15.00,
    derechosPorcentaje: 16,
    ivaPorcentaje: 21
  });

  const api = reactive({ tipoCambio: 0, cargandoDolar: true });
  const historial = ref([]);

  const agregarProducto = (sku) => {
    const prod = productosBase.find(p => p.sku === sku) || 
                 { sku: 'CUSTOM', nombre: 'Producto Manual', fob: 0, pesoKg: 0, largo: 10, ancho: 10, alto: 10 };
    
    itemsEnPedido.value.push({
      ...prod,
      id: Date.now(),
      cantidad: 1
    });
  };

  const eliminarItem = (id) => {
    itemsEnPedido.value = itemsEnPedido.value.filter(item => item.id !== id);
  };

  const obtenerDolarAPI = async () => {
    try {
      const resp = await fetch('https://dolarapi.com/v1/dolares/oficial');
      const data = await resp.json();
      api.tipoCambio = data.venta;
    } catch { api.tipoCambio = 1050; } finally { api.cargandoDolar = false; }
  };

  onMounted(obtenerDolarAPI);

  // CÁLCULOS AGREGADOS (Agrupados por pedido)
  const fobTotal = computed(() => itemsEnPedido.value.reduce((acc, i) => acc + (i.fob * i.cantidad), 0));
  const pesoFisicoTotal = computed(() => itemsEnPedido.value.reduce((acc, i) => acc + (i.pesoKg * i.cantidad), 0));
  const pesoVolumetricoTotal = computed(() => itemsEnPedido.value.reduce((acc, i) => acc + (((i.largo * i.ancho * i.alto) / 5000) * i.cantidad), 0));
  const pesoCobradoTotal = computed(() => Math.max(pesoFisicoTotal.value, pesoVolumetricoTotal.value));
  
  const fleteCalculado = computed(() => form.costoServicio + (pesoCobradoTotal.value * form.costoPorKg));
  const baseCif = computed(() => fobTotal.value + fleteCalculado.value);
  const costoDerechos = computed(() => baseCif.value * (form.derechosPorcentaje / 100));
  const costoIva = computed(() => (baseCif.value + costoDerechos.value) * (form.ivaPorcentaje / 100));
  const totalImpuestos = computed(() => costoDerechos.value + costoIva.value);
  const totalUsd = computed(() => baseCif.value + totalImpuestos.value);
  const costoUnitarioArs = computed(() => (totalUsd.value / (itemsEnPedido.value.reduce((acc, i) => acc + i.cantidad, 0) || 1)) * api.tipoCambio);

  const guardarSimulacion = () => {
    historial.value.unshift({
      id: Date.now(),
      fecha: new Date().toLocaleDateString('es-AR'),
      totalUsd: totalUsd.value,
      costoUnitarioArs: costoUnitarioArs.value
    });
  };

  return { productosBase, itemsEnPedido, form, api, historial, fobTotal, pesoFisicoTotal, pesoVolumetricoTotal, pesoCobradoTotal, fleteCalculado, costoDerechos, costoIva, totalImpuestos, totalUsd, costoUnitarioArs, agregarProducto, eliminarItem, guardarSimulacion };
}