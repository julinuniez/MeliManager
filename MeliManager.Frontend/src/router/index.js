import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'catalogo',
      component: () => import('../views/CatalogoView.vue')
    },
    {
      path: '/simulador',
      name: 'simulador',
      component: () => import('../views/SimuladorView.vue')
    },
    {
      path: '/postventa',
      name: 'postventa',
      component: () => import('../views/PostventaView.vue') // Nombre actualizado
    }
  ]
})

export default router