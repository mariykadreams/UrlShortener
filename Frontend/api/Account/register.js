import API_BASE_URL from './config';

async function registerUser(username, email, password) {
  try {
    const response = await fetch(`${API_BASE_URL}/api/Account/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ username, email, password }),
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Registration failed');
    }

    const data = await response.json();
    console.log('Registration successful:', data.message);
    return true; // Or return data
  } catch (error) {
    console.error('Registration error:', error.message);
    return false;
  }
}

// Example usage in a React component:
// const handleRegister = async () => {
//   const success = await registerUser('testuser', 'test@example.com', 'Password123');
//   if (success) {
//     // Redirect to login or show success message
//   }
// };