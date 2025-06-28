import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './Navbar.css';

const Navbar = ({ isAuthenticated, onLogout }) => {
  const navigate = useNavigate();

  const handleLogoutClick = (e) => { // <-- Add 'e' here
    e.preventDefault();
    onLogout();
    navigate('/'); // або перенаправити на логін
  };

  return (
    <header className="header">
      <Link to="/" className="logo">Url Shortener</Link>

      <nav className="navbar">
        <Link to="/ShortUrlsTable">Short URLs Table</Link>
        <Link to="/About">About</Link>

        {isAuthenticated ? (
          <a href="#" onClick={handleLogoutClick} className="logout-link">Log Out</a>
        ) : (
          <Link to="/SignInForm">Log In</Link>
        )}
      </nav>
    </header>
  );
};

export default Navbar;
