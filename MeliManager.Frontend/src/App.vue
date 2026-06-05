<template>
  <div class="layout-principal">
    <header class="navbar">
      <div class="navbar-contenedor">
        
        <div class="logo-contenedor">
          <h1>MeliManager</h1>
        </div>

        <nav class="navegacion">
          <router-link to="/" class="nav-item">📦 Catálogo</router-link>
          <router-link to="/simulador" class="nav-item">📊 Simulador Costos</router-link>
          <router-link to="/postventa" class="nav-item">⚙️ Postventa</router-link>
        </nav>

      </div>
    </header>

    <main class="contenido-principal">
      <router-view></router-view>
    </main>
  </div>
</template>

<style>
/* Reseteo básico */
body, html {
  margin: 0;
  padding: 0;
  height: 100%;
  font-family: 'Proxima Nova', -apple-system, Roboto, Arial, sans-serif;
  background-color: #ebebeb;
}

.layout-principal {
  display: flex;
  flex-direction: column; /* Cambiamos a formato vertical */
  min-height: 100vh;
}

/* --- ESTILOS DE LA BARRA SUPERIOR --- */
.navbar {
  background-color: #fff159; /* Amarillo suave ML */
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
  position: sticky;
  top: 0;
  z-index: 100;
}

.navbar-contenedor {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 2rem;
  height: 70px;
}

.logo-contenedor h1 {
  margin: 0;
  font-size: 1.5rem;
  color: #2d3277;
  font-weight: 900;
  letter-spacing: -0.5px;
}

.navegacion {
  display: flex;
  gap: 1rem; /* Espacio entre los botones */
}

.nav-item {
  padding: 0.6rem 1.2rem;
  border-radius: 6px;
  color: #333333;
  text-decoration: none;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
}

.nav-item:hover {
  background-color: rgba(0, 0, 0, 0.04);
}

.router-link-active {
  background-color: white;
  color: #3483fa;
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
}

/* --- ÁREA DE CONTENIDO --- */
.contenido-principal {
  flex: 1;
  padding: 2.5rem 2rem;
  max-width: 1200px; /* Mantiene todo alineado al centro en pantallas gigantes */
  margin: 0 auto;
  width: 100%;
  box-sizing: border-box;
}
</style>