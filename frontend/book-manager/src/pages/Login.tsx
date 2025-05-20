// frontend/book-manager/src/components/Login.tsx
import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { setBasicAuth, handleOAuthCallback, initiateGoogleLogin } from '../services/authService';
import { bookApi} from '../services/bookApi';

const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [fullname, setFullName] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [isNewUser, setIsNewUser] = useState(false);
  
  const navigate = useNavigate();
  const location = useLocation();

   const handleGoogleLogin = (e: React.MouseEvent) => {
    e.preventDefault();
    initiateGoogleLogin();
  };

 // Check if we're returning from OAuth redirect
  const handleOAuthRedirect = async () => {
    const queryParams = new URLSearchParams(location.search);
    const code = queryParams.get('code');
    
    if (code) {
      setLoading(true);
      const success = await handleOAuthCallback(code);
      if (success) {
        navigate('/');
      } else {
        setError('Failed to authenticate with Google');
      }
      setLoading(false);
    }
  };

  // Call this in useEffect
  useState(() => {
    handleOAuthRedirect();
  }, [location]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    
    try {
      // If new user, register the user
      if (isNewUser) {
        //const creds = setBasicAuth(email, password);
        
        await bookApi.registerUser(
          { email, password: setBasicAuth(email, password), name: fullname }
        );
        // Optionally, you can handle the response here
      }
      // Set the basic auth credentials
      setBasicAuth(email, password);
      await bookApi.loginUser({ email, password });
      
      // Test credentials by making a request to the API
      await bookApi.getAll();
      
      // If request was successful, redirect to books list
      navigate('/books');
    } catch (err) {
      // Clear invalid credentials on failure
      localStorage.removeItem('basicAuth');
      setError('Invalid email or password');
      console.error('Login failed:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-form">
      <h2>{isNewUser ? 'Register' : 'Login'} to Book Manager</h2>
      
      {error && <div className="error-message">{error}</div>}
      
      <div className="oauth-options">
        <p>Or login with:</p>
        <button 
          onClick={handleGoogleLogin} 
          className="btn-google"
          disabled={loading}
        >
          Login with Google
        </button>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="isNewUser">
          <input
            type="checkbox"
            id="isNewUser"
            checked={isNewUser}
            onChange={(e) => setIsNewUser(e.target.checked)}
          />
          I am a new User</label>
        </div>

        {isNewUser && (<div className="form-group">
          <label htmlFor="fullname">Name</label>
          <input
            type="text"
            id="fullname"
            value={fullname}
            onChange={(e) => setFullName(e.target.value)}
            required={isNewUser}
          />
        </div>
        )}

        <div className="form-group">
          <label htmlFor="email">Email</label>
          <input
            type="text"
            id="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
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