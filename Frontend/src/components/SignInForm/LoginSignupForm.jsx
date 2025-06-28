import React, { useState } from 'react';

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
      <style>
        {`
          @import url('https://fonts.googleapis.com/css?family=Poppins:400,500,600,700&display=swap');
          *{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: 'Poppins', sans-serif;
          }
          html,body{
            display: grid;
            height: 100%;
            width: 100%;
            place-items: center;
            background: -webkit-linear-gradient(left, #003366,#004080,#0059b3, #0073e6);
          }
          ::selection{
            background: #1a75ff;
            color: #fff;
          }
          .wrapper{
            overflow: hidden;
            max-width: 390px;
            background: #fff;
            padding: 30px;
            border-radius: 15px;
            box-shadow: 0px 15px 20px rgba(0,0,0,0.1);
          }
          .wrapper .title-text{
            display: flex;
            width: 200%;
          }
          .wrapper .title{
            width: 50%;
            font-size: 35px;
            font-weight: 600;
            text-align: center;
            transition: all 0.6s cubic-bezier(0.68,-0.55,0.265,1.55);
          }
          .wrapper .title.signup-active {
            margin-left: -50%;
          }
          .wrapper .title.login-active {
            margin-left: 0%;
          }


          .wrapper .slide-controls{
            position: relative;
            display: flex;
            height: 50px;
            width: 100%;
            overflow: hidden;
            margin: 30px 0 10px 0;
            justify-content: space-between;
            border: 1px solid lightgrey;
            border-radius: 15px;
          }
          .slide-controls .slide{
            height: 100%;
            width: 100%;
            color: #fff;
            font-size: 18px;
            font-weight: 500;
            text-align: center;
            line-height: 48px;
            cursor: pointer;
            z-index: 1;
            transition: all 0.6s ease;
          }
          .slide-controls label.signup-label{
            color: #000;
          }
          .slide-controls .slider-tab{
            position: absolute;
            height: 100%;
            width: 50%;
            left: 0;
            z-index: 0;
            border-radius: 15px;
            background: -webkit-linear-gradient(left,#003366,#004080,#0059b3, #0073e6);
            transition: all 0.6s cubic-bezier(0.68,-0.55,0.265,1.55);
          }
          .slider-tab.signup-active-tab {
              left: 50%;
          }
          .slider-tab.login-active-tab {
              left: 0%;
          }


          input[type="radio"]{
            display: none;
          }
          #signup:checked ~ label.signup-label{
            color: #fff;
            cursor: default;
            user-select: none;
          }
          #signup:checked ~ label.login-label{
            color: #000;
          }
          #login:checked ~ label.signup-label{
            color: #000;
          }
          #login:checked ~ label.login-label{
            cursor: default;
            user-select: none;
          }


          .wrapper .form-container{
            width: 100%;
            overflow: hidden;
          }
          .form-container .form-inner{
            display: flex;
            width: 200%;
            transform: translateX(${showLogin ? '0%' : '-50%'});
            transition: all 0.6s cubic-bezier(0.68,-0.55,0.265,1.55);
          }
          .form-container .form-inner form{
            width: 50%;
          }

          .login-form.active-form {
          }

          .signup-form.active-form {
          }


          .form-inner form .field{
            height: 50px;
            width: 100%;
            margin-top: 20px;
          }
          .form-inner form .field input{
            height: 100%;
            width: 100%;
            outline: none;
            padding-left: 15px;
            border-radius: 15px;
            border: 1px solid lightgrey;
            border-bottom-width: 2px;
            font-size: 17px;
            transition: all 0.3s ease;
          }
          .form-inner form .field input:focus{
            border-color: #1a75ff;
          }
          .form-inner form .field input::placeholder{
            color: #999;
            transition: all 0.3s ease;
          }
          form .field input:focus::placeholder{
            color: #1a75ff;
          }
          .form-inner form .pass-link{
            margin-top: 5px;
            text-align: left;
          }
          .form-inner form .signup-link{
            text-align: center;
            margin-top: 30px;
          }
          .form-inner form .pass-link a,
          .form-inner form .signup-link a{
            color: #1a75ff;
            text-decoration: none;
          }
          .form-inner form .pass-link a:hover,
          .form-inner form .signup-link a:hover{
            text-decoration: underline;
          }
          form .btn{
            height: 50px;
            width: 100%;
            border-radius: 15px;
            position: relative;
            overflow: hidden;
          }
          form .btn .btn-layer{
            height: 100%;
            width: 300%;
            position: absolute;
            left: -100%;
            background: -webkit-linear-gradient(right,#003366,#004080,#0059b3, #0073e6);
            border-radius: 15px;
            transition: all 0.4s ease;;
          }
          form .btn:hover .btn-layer{
            left: 0;
          }
          form .btn input[type="submit"]{
            height: 100%;
            width: 100%;
            z-index: 1;
            position: relative;
            background: none;
            border: none;
            color: #fff;
            padding-left: 0;
            border-radius: 15px;
            font-size: 20px;
            font-weight: 500;
            cursor: pointer;
          }
          .form-message {
            margin-top: 15px;
            text-align: center;
            font-size: 14px;
            padding: 8px;
            border-radius: 8px;
            background-color: #f2f2f2;
          }
          .form-message[style*="color: green"] {
            background-color: #e6ffe6;
            border: 1px solid #00cc00;
          }
          .form-message[style*="color: red"] {
            background-color: #ffe6e6;
            border: 1px solid #cc0000;
          }
        `}
      </style>
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