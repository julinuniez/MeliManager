<script setup>
import { computed } from 'vue';

const props = defineProps({
  producto: { type: Object, required: true }
});

// NUEVO: Declaramos que este componente puede emitir el evento 'editar'
const emit = defineEmits(['editar']);

const claseEstado = computed(() => props.producto.estado === 'publicado' ? 'badge-verde' : 'badge-gris');
const claseStock = computed(() => {
  if (props.producto.stock === 0) return 'stock-cero';
  if (props.producto.stock <= 5) return 'stock-alerta';
  return 'stock-ok';
});
</script>

<template>
  <div class="tarjeta-producto" :class="{'tarjeta-pausada': producto.estado === 'pausado'}">
    <div class="imagen-contenedor">
      <img :src="producto.imagenUrl" :alt="producto.nombre" class="imagen" />
      <span class="badge-estado" :class="claseEstado">{{ producto.estado === 'publicado' ? 'Activo' : 'Pausado' }}</span>
      <span class="badge-stock" :class="claseStock">{{ producto.stock }} un.</span>
    </div>
    <div class="info-contenedor">
      <p class="sku">{{ producto.sku }}</p>
      <h3 class="titulo" :title="producto.nombre">{{ producto.nombre }}</h3>
      <div class="precio-contenedor">
        <span class="precio">${{ producto.precioVenta.toLocaleString('es-AR') }}</span>
      </div>
      <div class="acciones">
        <button class="btn-accion btn-editar" @click="emit('editar', producto)">Editar Detalles</button>
        <button class="btn-accion btn-icon" title="Ver en Mercado Libre">↗</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Mantener tu CSS actual de la tarjeta, está perfecto */
.tarjeta-producto { background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.06); transition: all 0.2s ease; display: flex; flex-direction: column; border: 1px solid #eee; }
.tarjeta-producto:hover { box-shadow: 0 8px 16px rgba(0,0,0,0.1); transform: translateY(-4px); }
.tarjeta-pausada { opacity: 0.85; filter: grayscale(20%); }
.imagen-contenedor { width: 100%; aspect-ratio: 1 / 1; background-color: #f8f9fa; position: relative; display: flex; align-items: center; justify-content: center; border-bottom: 1px solid #f1f3f5; }
.imagen { width: 100%; height: 100%; object-fit: cover; }
.badge-estado { position: absolute; top: 12px; left: 12px; padding: 4px 10px; border-radius: 20px; font-size: 0.75rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; }
.badge-verde { background-color: #00a650; color: white; }
.badge-gris { background-color: #e9ecef; color: #666; }
.badge-stock { position: absolute; top: 12px; right: 12px; padding: 4px 10px; border-radius: 6px; font-size: 0.8rem; font-weight: 700; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
.stock-ok { background-color: #fff159; color: #333; }
.stock-alerta { background-color: #ff9800; color: white; }
.stock-cero { background-color: #dc3545; color: white; }
.info-contenedor { padding: 1.2rem; display: flex; flex-direction: column; flex: 1; }
.sku { color: #888; font-size: 0.75rem; font-family: monospace; margin: 0 0 6px 0; }
.titulo { margin: 0 0 1rem 0; font-size: 1.05rem; color: #333; font-weight: 600; line-height: 1.4; display: -webkit-box; -webkit-line-clamp: 2; line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
.precio-contenedor { margin-top: auto; margin-bottom: 1.2rem; }
.precio { font-size: 1.5rem; font-weight: bold; color: #333; }
.acciones { display: flex; gap: 8px; }
.btn-accion { background-color: #f1f3f5; color: #3483fa; border: none; padding: 10px; border-radius: 6px; font-weight: 600; cursor: pointer; transition: background 0.2s; font-size: 0.9rem; }
.btn-accion:hover { background-color: #e6f0ff; }
.btn-editar { flex: 1; }
.btn-icon { width: 40px; font-size: 1.2rem; display: flex; align-items: center; justify-content: center; }
</style>