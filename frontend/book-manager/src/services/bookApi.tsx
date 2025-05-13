import AuthTokens from '../types/AuthTokens';
import Book from '../types/Book';
import User from '../types/User';
import { configureBookApiWithBasicAuth, configureBookApiWithJwtAuth } from './authService';


// Use the API_URL from the authService
export const bookApi = {
  getAll: async (): Promise<Book[]> => {
    const api = configureBookApiWithJwtAuth();
    const response = await api.get<Book[]>(`/publisherbooks`);
    return response.data;
  },

  getById: async (id: number): Promise<Book> => {
    const api = configureBookApiWithJwtAuth();
    const response = await api.get<Book>(`/books/${id}`);
    return response.data;
  },

  create: async (book: Book): Promise<Book> => {
    const api = configureBookApiWithJwtAuth();
    console.log('Sending book data:', JSON.stringify(book, null, 2));
    try {
      const response = await api.post<Book>(`/books`, book);
      return response.data;
    } catch (error) {
      console.error('Error creating book:', error.response?.data);
      throw error;
    }
  },

  update: async (id: number, book: Book): Promise<void> => {
    const api = configureBookApiWithJwtAuth();
    await api.put(`/books/${id}`, book);
  },

  updateAvailability: async (id: number, isAvailable: boolean): Promise<void> => {
    const api = configureBookApiWithJwtAuth();
    await api.patch(`/books/${id}/availability?isAvailable=${isAvailable}`);
  },

  delete: async (id: number): Promise<void> => {
    const api = configureBookApiWithJwtAuth();
    await api.delete(`/books/${id}`);
  },

  registerUser: async (user: User): Promise<User> => {
    const api = configureBookApiWithBasicAuth();
    const response = await api.post<User>(`/register`, user);
    return response.data;
  },

  loginUser: async (user: User): Promise<boolean> => {
    const api = configureBookApiWithBasicAuth();
    const tokens: AuthTokens = await api.post<AuthTokens>(`/login`, user);//await response.json();
    localStorage.setItem('accessToken', tokens.data.accessToken);
    localStorage.setItem('refreshToken', tokens.data.refreshToken);
    return true;
  }
};