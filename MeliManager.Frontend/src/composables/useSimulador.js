import { ref, reactive, computed, onMounted, watch } from 'vue';

export function useSimulador() {
  const productosBase = [
    { sku: 'ALARMA-MOTO-01', nombre: 'Alarma Inalámbrica', fob: 5.70, pesoKg: 0.1, largo: 10, ancho: 10, alto: 3 },
    { sku: 'CANDADO-GPS-02', nombre: 'Candado U-Lock', fob: 21.50, pesoKg: 1.2, largo: 25, ancho: 15, alto: 6 },
    { sku: 'LINGA-ACERO-03', nombre: 'Linga de Acero 1.5m', fob: 8.00, pesoKg: 0.8, largo: 20, ancho: 20, alto: 8 },
    { sku: 'CUSTOM', nombre: '+ Producto sin registrar (Ingreso Manual)', fob: 0, pesoKg: 0, largo: 0, ancho: 0, alto: 0 }
  ];

  const form = reactive({
    skuSeleccionado: productosBase[0].sku,
    cantidad: 30, // Ideal para testear
    fobUnitario: productosBase[0].fob,
    pesoKg: productosBase[0].pesoKg,
    largo: productosBase[0].largo,
    ancho: productosBase[0].ancho,
    alto: productosBase[0].alto,
    costoServicio: 150.00, // Ahora se llama Costo de Servicio
    costoPorKg: 15.00,
    derechosPorcentaje: 16, // Promedio para repuestos/accesorios
    ivaPorcentaje: 21 // IVA estándar
  });

  const api = reactive({ tipoCambio: 0, cargandoDolar: true });
  const historial = ref([]);

  watch(() => form.skuSeleccionado, (nuevoSku) => {
    const prod = productosBase.find(p => p.sku === nuevoSku);
    if (prod && nuevoSku !== 'CUSTOM') {
      form.fobUnitario = prod.fob;
      form.pesoKg = prod.pesoKg;
      form.largo = prod.largo;
      form.ancho = prod.ancho;
      form.alto = prod.alto;
    } else if (nuevoSku === 'CUSTOM') {
      form.fobUnitario = 0;
      form.pesoKg = 0;
      form.largo = 0;
      form.ancho = 0;
      form.alto = 0;
    }
  });

  const obtenerDolarAPI = async () => {
    try {
      api.cargandoDolar = true;
      const respuesta = await fetch('https://dolarapi.com/v1/dolares/oficial');
      const data = await respuesta.json();
      api.tipoCambio = data.venta;
    } catch (error) {
      api.tipoCambio = 1050; 
    } finally {
      api.cargandoDolar = false;
    }
  };

  onMounted(() => obtenerDolarAPI());

  // Logística
  const pesoVolumetricoUnitario = computed(() => (form.largo * form.ancho * form.alto) / 5000);
  const pesoFisicoTotal = computed(() => form.pesoKg * form.cantidad);
  const pesoVolumetricoTotal = computed(() => pesoVolumetricoUnitario.value * form.cantidad);
  const pesoCobradoTotal = computed(() => Math.max(pesoFisicoTotal.value, pesoVolumetricoTotal.value));

  // El flete total ahora suma el servicio base + el costo por peso
  const fleteCalculado = computed(() => form.costoServicio + (pesoCobradoTotal.value * form.costoPorKg));
  const fobTotal = computed(() => form.cantidad * form.fobUnitario);
  
  // TRIBUTOS (Aduana Argentina calcula sobre CIF: Costo + Seguro + Flete. Omitimos seguro para simplificar)
  const baseCif = computed(() => fobTotal.value + fleteCalculado.value);
  
  const costoDerechos = computed(() => baseCif.value * ((form.derechosPorcentaje || 0) / 100));
  // El IVA aduanero suele calcularse sobre la base CIF + Derechos
  const baseIva = computed(() => baseCif.value + costoDerechos.value);
  const costoIva = computed(() => baseIva.value * ((form.ivaPorcentaje || 0) / 100));
  
  const totalImpuestos = computed(() => costoDerechos.value + costoIva.value);

  const totalUsd = computed(() => fobTotal.value + fleteCalculado.value + totalImpuestos.value);
  const costoUnitarioUsd = computed(() => totalUsd.value / form.cantidad);
  const costoUnitarioArs = computed(() => costoUnitarioUsd.value * api.tipoCambio);

  const guardarSimulacion = () => {
    historial.value.unshift({
      id: Date.now(),
      fecha: new Date().toLocaleDateString('es-AR'),
      referencia: form.skuSeleccionado === 'CUSTOM' ? 'Simulación Manual' : form.skuSeleccionado,
      cantidad: form.cantidad,
      fobTotal: fobTotal.value,
      fleteAduana: fleteCalculado.value + totalImpuestos.value,
      totalUsd: totalUsd.value,
      tipoCambio: api.tipoCambio,
      costoUnitarioArs: costoUnitarioArs.value
    });
  };

  const eliminarFila = (id) => historial.value = historial.value.filter(item => item.id !== id);

  return {
    productosBase, form, api, historial,
    pesoFisicoTotal, pesoVolumetricoTotal, pesoCobradoTotal, fleteCalculado,
    fobTotal, costoDerechos, costoIva, totalImpuestos, totalUsd, costoUnitarioArs,
    guardarSimulacion, eliminarFila
  };
}