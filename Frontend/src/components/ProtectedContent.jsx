import React, { useState } from 'react';
import API_BASE_URL from '../config';

/**
 * ProtectedContent component attempts to fetch data from authenticated
 * backend endpoints (Admin and User roles) using the stored JWT token.
 * It provides buttons to trigger these fetches and displays the results
 * or any errors encountered (e.g., unauthorized, forbidden).
 */
function ProtectedContent() {
  const [adminMessage, setAdminMessage] = useState('');
  const [userMessage, setUserMessage] = useState('');
  const [errorMessage, setErrorMessage] = useState('');

  /**
   * Fetches data from a specified protected API endpoint.
   * Requires a JWT token to be present in localStorage.
   *
   * @param {string} endpoint - The API endpoint to fetch from (e.g., 'Admin', 'User').
   * @param {function} setter - The state setter function to update with the fetched message.
   */
  const fetchProtectedData = async (endpoint, setter) => {
    setErrorMessage(''); // Clear general error messages
    setter(''); // Clear previous message for the specific endpoint

    const token = localStorage.getItem('jwtToken'); // Retrieve the JWT token

    if (!token) {
      setErrorMessage('No token found. Please log in.');
      return;
    }

    try {
      const response = await fetch(`${API_BASE_URL}/api/${endpoint}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`, // IMPORTANT: Include the JWT token in the Authorization header
        },
      });

      if (response.ok) {
        // Assuming your Admin and User controllers return plain text, not JSON
        const data = await response.text();
        setter(data); // Display the successful message from the API
      } else if (response.status === 401) {
        setErrorMessage(`Unauthorized for /api/${endpoint}. Token might be invalid or expired. Please log in again.`);
        localStorage.removeItem('jwtToken'); // Clear potentially invalid token
      } else if (response.status === 403) {
        setErrorMessage(`Forbidden for /api/${endpoint}. You do not have the required role to access this resource.`);
      } else {
        const errorText = await response.text(); // Get raw error text for other errors
        setErrorMessage(`Failed to fetch from /api/${endpoint}: ${errorText || response.statusText}`);
      }
    } catch (error) {
      setErrorMessage(`Network error fetching from /api/${endpoint}: ${error.message}`);
      console.error(`Fetch error for /api/${endpoint}:`, error);
    }
  };

  return (
    <div style={{ padding: '20px', border: '1px solid #ccc', borderRadius: '8px', marginTop: '20px', backgroundColor: '#f9f9f9' }}>
      <h2>Protected Content</h2>
      <p>Try fetching data from authenticated endpoints:</p>
      <div style={{ marginBottom: '10px' }}>
        <button
          onClick={() => fetchProtectedData('Admin', setAdminMessage)}
          style={{
            padding: '10px 15px',
            backgroundColor: '#007bff',
            color: 'white',
            border: 'none',
            borderRadius: '5px',
            cursor: 'pointer',
            marginRight: '10px'
          }}
        >
          Fetch Admin Data
        </button>
        {adminMessage && <p style={{ display: 'inline-block', marginLeft: '10px', color: '#0056b3', fontWeight: 'bold' }}>{adminMessage}</p>}
      </div>
      <div>
        <button
          onClick={() => fetchProtectedData('User', setUserMessage)}
          style={{
            padding: '10px 15px',
            backgroundColor: '#28a745',
            color: 'white',
            border: 'none',
            borderRadius: '5px',
            cursor: 'pointer'
          }}
        >
          Fetch User Data
        </button>
        {userMessage && <p style={{ display: 'inline-block', marginLeft: '10px', color: '#1e7e34', fontWeight: 'bold' }}>{userMessage}</p>}
      </div>
      {errorMessage && <p style={{ marginTop: '15px', color: 'red', fontWeight: 'bold' }}>{errorMessage}</p>}
    </div>
  );
}

export default ProtectedContent;
