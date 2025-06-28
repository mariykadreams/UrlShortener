import React, { useState } from 'react';
import './LoginSignupForm.css';


function App({ onLoginSuccess = () => console.log('Login successful!') }) {
  const API_BASE_URL = 'https://localhost:7283';

  const [showLogin, setShowLogin] = useState(true);

  const [loginUsername, setLoginUsername] = useState('');
  const [loginPassword, setLoginPassword] = useState('');
  const [loginMessage, setLoginMessage] = useState('');

  const [signupUsername, setSignupUsername] = useState('');
  const [signupEmail, setSignupEmail] = useState('');
  const [signupPassword, setSignupPassword] = useState('');
  const [signupMessage, setSignupMessage] = useState('');

  const handleLogin = async (e) => {
    e.preventDefault();
    setLoginMessage('');

    if (!loginUsername || !loginPassword) {
      setLoginMessage('Please enter username and password.');
      return;
    }

    try {
      const response = await fetch(`${API_BASE_URL}/api/Account/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ username: loginUsername, password: loginPassword }),
      });

      if (response.ok) {
        const data = await response.json();
        localStorage.setItem('jwtToken', data.token);
        setLoginMessage('Login successful!');
        setLoginUsername('');
        setLoginPassword('');
        onLoginSuccess();
      } else {
        const errorText = await response.text();
        setLoginMessage(`Login error. Invalid username or password. Details: ${errorText}`);
      }
    } catch (error) {
      setLoginMessage(`Network error: ${error.message}. Please check your API server or API_BASE_URL.`);
      console.error('Error during login request:', error);
    }
  };

  const handleRegister = async (e) => {
    e.preventDefault();
    setSignupMessage('');

    if (!signupUsername || !signupEmail || !signupPassword) {
      setSignupMessage('Please fill in all registration fields.');
      return;
    }
    if (signupPassword.length < 6) {
      setSignupMessage('Password must be at least 6 characters.');
      return;
    }

    try {
      const response = await fetch(`${API_BASE_URL}/api/Account/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ username: signupUsername, email: signupEmail, password: signupPassword }),
      });

      if (response.ok) {
        setSignupMessage('Registration successful! You can now log in.');
        setSignupUsername('');
        setSignupEmail('');
        setSignupPassword('');
        setShowLogin(true);
      } else {
        const errorData = await response.json();
        if (errorData.errors && Array.isArray(errorData.errors)) {
          const errors = errorData.errors.map(err => err.description).join('\n');
          setSignupMessage(`Registration error:\n${errors}`);
        } else {
          setSignupMessage(errorData.message || 'Registration error. Please try again.');
        }
      }
    } catch (error) {
      setSignupMessage(`Network error: ${error.message}. Please check your API server or API_BASE_URL.`);
      console.error('Error during registration request:', error);
    }
  };

  return (
    <>
      <div className="wrapper">
        <div className="title-text">
          <div className={`title ${showLogin ? 'login' : 'signup-active'}`}>Login Form</div>
          <div className={`title ${!showLogin ? 'signup' : 'login-active'}`}>Signup Form</div>
        </div>
        <div className="form-container">
          <div className="slide-controls">
            <input
              type="radio"
              name="slide"
              id="login"
              checked={showLogin}
              onChange={() => setShowLogin(true)}
            />
            <input
              type="radio"
              name="slide"
              id="signup"
              checked={!showLogin}
              onChange={() => setShowLogin(false)}
            />
            <label htmlFor="login" className="slide login-label">Login</label>
            <label htmlFor="signup" className="slide signup-label">Signup</label>
            <div className={`slider-tab ${showLogin ? 'login-active-tab' : 'signup-active-tab'}`}></div>
          </div>
          <div className="form-inner">
            <form
              className={`login-form ${showLogin ? 'active-form' : ''}`}
              onSubmit={handleLogin}
            >
              <div className="field">
                <input
                  type="text"
                  placeholder="Username"
                  required
                  value={loginUsername}
                  onChange={(e) => setLoginUsername(e.target.value)}
                />
              </div>
              <div className="field">
                <input
                  type="password"
                  placeholder="Password"
                  required
                  value={loginPassword}
                  onChange={(e) => setLoginPassword(e.target.value)}
                />
              </div>
              <div className="pass-link"><a href="#" onClick={(e) => e.preventDefault()}>Forgot password?</a></div>
              <div className="field btn">
                <div className="btn-layer"></div>
                <input type="submit" value="Login" />
              </div>
              <div className="signup-link">
                Not a member? <a href="#" onClick={(e) => { e.preventDefault(); setShowLogin(false); }}>Signup now</a>
              </div>
              {loginMessage && <p className="form-message" style={{ color: loginMessage.includes('successful') ? 'green' : 'red' }}>{loginMessage}</p>}
            </form>

            <form
              className={`signup-form ${!showLogin ? 'active-form' : ''}`}
              onSubmit={handleRegister}
            >
              <div className="field">
                <input
                  type="text"
                  placeholder="Username"
                  required
                  value={signupUsername}
                  onChange={(e) => setSignupUsername(e.target.value)}
                />
              </div>
              <div className="field">
                <input
                  type="email"
                  placeholder="Email Address"
                  required
                  value={signupEmail}
                  onChange={(e) => setSignupEmail(e.target.value)}
                />
              </div>
              <div className="field">
                <input
                  type="password"
                  placeholder="Password"
                  required
                  value={signupPassword}
                  onChange={(e) => setSignupPassword(e.target.value)}
                />
              </div>
              <div className="field btn">
                <div className="btn-layer"></div>
                <input type="submit" value="Signup" />
              </div>
              {signupMessage && <p className="form-message" style={{ color: signupMessage.includes('successful') ? 'green' : 'red' }}>{signupMessage}</p>}
            </form>
          </div>
        </div>
      </div>
    </>
  );
}

export default App;