// frontend/book-manager/src/services/authService.tsx
import axios from 'axios';

// Basic auth helper for Book Manager
export const setBasicAuth = (username: string, password: string) => {
  const credentials = btoa(`${username}:${password}`);
  localStorage.setItem('basicAuth', credentials);
  return credentials;
};

// Update your existing bookApi to use Basic Auth
export const configureBookApiWithBasicAuth = () => {
  const api = axios.create({
    baseURL: 'http://localhost:5137/api',
  });

  api.interceptors.request.use(config => {
    const credentials = localStorage.getItem('basicAuth');
    if (credentials) {
      config.headers.Authorization = `Basic ${credentials}`;
    }
    return config;
  });

  return api;
};