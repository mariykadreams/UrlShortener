import API_BASE_URL from './config';

async function getAdminData() {
  const token = localStorage.getItem('jwtToken'); // Retrieve the stored token

  if (!token) {
    console.error('No token found. User not authenticated.');
    return null;
  }

  try {
    const response = await fetch(`${API_BASE_URL}/api/Admin`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`, // <-- Important: Include the Bearer token
      },
    });

    if (!response.ok) {
      if (response.status === 401) {
        console.error('Unauthorized: Token might be invalid or expired.');
        // Optionally, clear invalid token and redirect to login
        localStorage.removeItem('jwtToken');
      } else if (response.status === 403) {
        console.error('Forbidden: User does not have the required role (Admin).');
      }
      throw new Error('Failed to fetch admin data');
    }

    const data = await response.text(); // Or .json() if your API returns JSON
    console.log('Admin data:', data);
    return data;
  } catch (error) {
    console.error('Error fetching admin data:', error.message);
    return null;
  }
}

// Example usage:
// const handleGetAdminData = async () => {
//   await getAdminData();
// };