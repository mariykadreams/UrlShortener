import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Navbar from './components/Navbar/Navbar';
import LoginSignupForm from './components/SignInForm/LoginSignupForm';
import ShortUrlsTable from './components/ShortUrlsTable/ShortUrlsTable';
import ShortURLInfo from './components/ShortURLInfo/ShortURLInfo';

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
      {/* Add a div with className="App" to apply the padding-top */}
      <div className="App">
        <Routes>
          {/* Страница доступна всем, независимо от авторизации */}
          <Route path="/ShortUrlsTable" element={<ShortUrlsTable isLoggedIn={isAuthenticated} onNavigate={() => {}} />} />
          
          <Route path="/ShortUrlInfo/:id" element={<ShortURLInfo />} />
          {/* Страница входа */}
          <Route path="/SignInForm" element={<LoginSignupForm onLoginSuccess={handleLoginSuccess} />} />

          {/* Можно добавить главную или другие страницы */}
          <Route path="/" element={<ShortUrlsTable />} /> {/* Add a default route if needed */}
        </Routes>
      </div>
    </Router>
  );
}

export default App;