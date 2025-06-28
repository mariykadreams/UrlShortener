import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Navbar from './components/Navbar/Navbar';
import LoginSignupForm from './components/SignInForm/LoginSignupForm';
import ShortUrlsTable from './components/ShortUrlsTable/ShortUrlsTable';
import ShortURLInfo from './components/ShortURLInfo/ShortURLInfo';
import About from './components/About/About';
import './App.css';

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem('jwtToken');
    setIsAuthenticated(!!token);
  }, []);

  const handleLoginSuccess = () => {
    setIsAuthenticated(true);
  };

  const handleLogout = () => {
    localStorage.removeItem('jwtToken');
    setIsAuthenticated(false);
  };

  return (
    <Router>
      <Navbar isAuthenticated={isAuthenticated} onLogout={handleLogout} />
      <div className="App">
        <Routes>
          <Route path="/ShortUrlsTable" element={<ShortUrlsTable isLoggedIn={isAuthenticated} />} />
          <Route path="/ShortUrlInfo/:id" element={<ShortURLInfo />} />
          <Route path="/About" element={<About />} />
          <Route path="/SignInForm" element={<LoginSignupForm onLoginSuccess={handleLoginSuccess} />} />
          <Route path="/" element={<ShortUrlsTable isLoggedIn={isAuthenticated} />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;
