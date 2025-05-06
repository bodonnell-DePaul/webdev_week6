// frontend/book-manager/src/components/Login.tsx
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { setBasicAuth } from '../services/authService';
import { bookApi } from '../services/bookApi';

const Login = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    
    try {
      // Set the basic auth credentials
      setBasicAuth(username, password);
      
      // Test credentials by making a request to the API
      await bookApi.getAll();
      
      // If request was successful, redirect to books list
      navigate('/books');
    } catch (err) {
      // Clear invalid credentials on failure
      localStorage.removeItem('basicAuth');
      setError('Invalid username or password');
      console.error('Login failed:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-form">
      <h2>Login to Book Manager</h2>
      
      {error && <div className="error-message">{error}</div>}
      
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="username">Username</label>
          <input
            type="text"
            id="username"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
          />
        </div>
        
        <div className="form-group">
          <label htmlFor="password">Password</label>
          <input
            type="password"
            id="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>
        
        <button 
          type="submit" 
          className="btn-primary"
          disabled={loading}
        >
          {loading ? 'Logging in...' : 'Login'}
        </button>
      </form>
    </div>
  );
};

export default Login;