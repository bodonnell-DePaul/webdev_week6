import Book from '../types/Book';
import { configureBookApiWithBasicAuth } from './authService';

// Use the API_URL from the authService
export const bookApi = {
  getAll: async (): Promise<Book[]> => {
    const api = configureBookApiWithBasicAuth();
    const response = await api.get<Book[]>(`/publisherbooks`);
    return response.data;
  },

  getById: async (id: number): Promise<Book> => {
    const api = configureBookApiWithBasicAuth();
    const response = await api.get<Book>(`/books/${id}`);
    return response.data;
  },

  create: async (book: Book): Promise<Book> => {
    const api = configureBookApiWithBasicAuth();
    const response = await api.post<Book>(`/books`, book);
    return response.data;
  },

  update: async (id: number, book: Book): Promise<void> => {
    const api = configureBookApiWithBasicAuth();
    await api.put(`/books/${id}`, book);
  },

  updateAvailability: async (id: number, isAvailable: boolean): Promise<void> => {
    const api = configureBookApiWithBasicAuth();
    await api.patch(`/books/${id}/availability?isAvailable=${isAvailable}`);
  },

  delete: async (id: number): Promise<void> => {
    const api = configureBookApiWithBasicAuth();
    await api.delete(`/books/${id}`);
  }
};