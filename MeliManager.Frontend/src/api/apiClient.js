const API_BASE_URL = 'https://localhost:7200/api'; 

export const apiClient = {
  async get(endpoint) {
    try {
      const response = await fetch(`${API_BASE_URL}${endpoint}`);
      if (!response.ok) throw new Error(`Error HTTP: ${response.status}`);
      return await response.json();
    } catch (error) {
      console.error(`Error GET en ${endpoint}:`, error);
      throw error;
    }
  },

  async post(endpoint, data = {}, headersExtras = {}) {
    try {
      const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
          ...headersExtras 
        },
        body: JSON.stringify(data)
      });
      
      // NUEVO: Si C# explota, capturamos el motivo exacto
      if (!response.ok) {
        const errorBody = await response.json().catch(() => ({}));
        throw new Error(errorBody.Detalle || errorBody.Error || errorBody.title || `Error HTTP ${response.status}`);
      }
      
      return await response.json();
    } catch (error) {
      console.error(`Error POST en ${endpoint}:`, error);
      throw error; // Propagamos el error real hacia Vue
    }
  }
};