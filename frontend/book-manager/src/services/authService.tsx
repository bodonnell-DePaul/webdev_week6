// frontend/book-manager/src/services/authService.tsx
import axios, { AxiosInstance } from 'axios';
import UserClaims from '../types/UserClaims';
import { jwtDecode } from 'jwt-decode';

// Store a single instance of the authenticated API
let apiInstance: AxiosInstance | null = null;

// Basic auth helper for Book Manager
export const setBasicAuth = (username: string, password: string) => {
  const credentials = btoa(`${username}:${password}`);
  localStorage.setItem('basicAuth', credentials);
  
  // Reset the API instance when credentials change
  apiInstance = null;
  
  return credentials;
};

// Clear authentication on logout
export const clearAuth = () => {
  localStorage.removeItem('basicAuth');
  apiInstance = null;
};

// Check if user is authenticated
export const isAuthenticated = () => {
  return !!localStorage.getItem('basicAuth');
};

// Add OAuth login function
export const initiateGoogleLogin = () => {
  // Redirect to the backend endpoint that will start the OAuth flow
  window.location.href = 'http://localhost:5137/auth/google';
};

// Handle OAuth callback
export const handleOAuthCallback = async (code: string): Promise<boolean> => {
  try {
    const response = await fetch('http://localhost:5137/auth/google/callback', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ code }),
    });
    
    if (!response.ok) {
      return false;
    }
    
    const tokens = await response.json();
    localStorage.setItem('accessToken', tokens.accessToken);
    localStorage.setItem('refreshToken', tokens.refreshToken);
    
    return true;
  } catch (error) {
    console.error('OAuth callback error:', error);
    return false;
  }
};

// Get a configured API instance with Basic Auth
export const configureBookApiWithBasicAuth = () => {
  // Return existing instance if available
  if (apiInstance) return apiInstance;
  
  // Create new instance if none exists
  apiInstance = axios.create({
    baseURL: 'http://localhost:5137/api',
  });

  apiInstance.interceptors.request.use(config => {
    const credentials = localStorage.getItem('basicAuth');
    if (credentials) {
      config.headers.Authorization = `Basic ${credentials}`;
    }
    return config;
  });

  return apiInstance;
};

export const configureBookApiWithJwtAuth = () => {
  // Return existing instance if available
  if (apiInstance) return apiInstance;
  
  // Create new instance if none exists
  apiInstance = axios.create({
    baseURL: 'http://localhost:5137/api',
  });

  apiInstance.interceptors.request.use(config => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  });

  return apiInstance;
};

// Create an API client that includes the token from OAuth
export const createOAuthBookApi = () => {
  const api = axios.create({
    baseURL: 'http://localhost:5137/api',
  });

  api.interceptors.request.use(config => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  });

  return api;
};

export const getUser = (): UserClaims | null => {
  const token = localStorage.getItem('accessToken');
  if (!token) return null;
  
  try {
    return jwtDecode<UserClaims>(token);
  } catch {
    return null;
  }
};