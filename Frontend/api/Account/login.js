import API_BASE_URL from './config';

async function loginUser(username, password) {
  try {
    const response = await fetch(`${API_BASE_URL}/api/Account/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ username, password }),
    });

    if (!response.ok) {
      throw new Error('Login failed');
    }

    const data = await response.json();
    const token = data.token;
    console.log('Login successful. Token:', token);
    // Store the token securely (e.g., in localStorage or sessionStorage)
    localStorage.setItem('jwtToken', token);
    return token;
  } catch (error) {
    console.error('Login error:', error.message);
    return null;
  }
}

// Example usage:
// const handleLogin = async () => {
//   const token = await loginUser('testuser', 'Password123');
//   if (token) {
//     // User is logged in, redirect to dashboard or fetch protected data
//   }
// };