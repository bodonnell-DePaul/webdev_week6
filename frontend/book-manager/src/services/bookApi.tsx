import axios from 'axios';
import Book from '../types/Book';
import { configureBookApiWithBasicAuth } from './authService';

const API_URL = 'http://localhost:5137/api'; // Adjust port to match your .NET API

export const bookApi = {
  getAll: async (): Promise<Book[]> => {
    configureBookApiWithBasicAuth();
    const response = await axios.get<Book[]>(`${API_URL}/publisherbooks`);
    console.log(response.data);
    return response.data;
  },

  getById: async (id: number): Promise<Book> => {
    configureBookApiWithBasicAuth();
    const response = await axios.get<Book>(`${API_URL}/books/${id}`);
    console.log(response.data);
    return response.data;
  },

  create: async (book: Book): Promise<Book> => {
    configureBookApiWithBasicAuth();
    const response = await axios.post<Book>(`${API_URL}/books`, book);
    console.log(response.data);
    return response.data;
  },

  update: async (id: number, book: Book): Promise<void> => {
    configureBookApiWithBasicAuth();
    await axios.put(`${API_URL}/books/${id}`, book);
  },

  updateAvailability: async (id: number, isAvailable: boolean): Promise<void> => {
    configureBookApiWithBasicAuth();
    await axios.patch(`${API_URL}/books/${id}/availability?isAvailable=${isAvailable}`);
  },

  delete: async (id: number): Promise<void> => {
    configureBookApiWithBasicAuth();
    await axios.delete(`${API_URL}/books/${id}`);
  }
};